using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Pamaxie.Authentication;
using Pamaxie.Database.Extensions;
using Spectre.Console;
using System;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text.Json.Nodes;
using Newtonsoft.Json.Linq;
using SendGrid;

namespace Pamaxie.Database.Api;

/// <summary>
/// Configuration setup for the Database Api
/// </summary>
public class AppConfigManagement
{
    [Flags]
    private enum MissingSettings : ushort
    {
        None = 0,
        Database = 1,
        JwtBearer = 2,
        All = 128,
        SendGrid
    }
        
    private const string DbSettingsNode = "DbSettings";
    private const string JwtSettingsNode = "JwtSettings";
    
    
    public const string SendGridEnvVar = DockerEnvVars.SendGridEnvVar;
    private static readonly string SettingsFileName = "PamSettings.json";

    /// <summary>
    /// Holds the Database settings
    /// </summary>
    internal static DbSettings DbSettings;

    /// <summary>
    /// Holds the settings for Jwt token Generation
    /// </summary>
    internal static JwtTokenConfig JwtSettings;

    /// <summary>
    /// Validates that the settings exist and are correct
    /// </summary>
    /// <returns></returns>
    public static bool ValidateConfiguration(out string additionalIssue)
    {
        var ruler = new Rule("[blue]Pamaxie Database API[/]");
        AnsiConsole.Write(ruler);
        additionalIssue = string.Empty;
        AnsiConsole.MarkupLine("[green]Validating Config...[/]");

        if (DockerEnvVars.InDocker)
        {
            var dbSettings = Environment.GetEnvironmentVariable(DockerEnvVars.DbSettingsEnvVar, EnvironmentVariableTarget.Machine);
            var jwtSettings = Environment.GetEnvironmentVariable(DockerEnvVars.JwtSettingsEnvVar, EnvironmentVariableTarget.Machine);
            var sendGridToken= Environment.GetEnvironmentVariable(DockerEnvVars.SendGridEnvVar, EnvironmentVariableTarget.Machine);

            if (string.IsNullOrEmpty(sendGridToken))
            {
                AnsiConsole.MarkupLine("[red]We could not find a sendgrid api token. This is required for the service to function[/]");
                return false;
            }

            if (!string.IsNullOrWhiteSpace(dbSettings) && 
                !string.IsNullOrWhiteSpace(jwtSettings)) return true;
                
            AnsiConsole.MarkupLine("[yellow]We could not find part of your configuration. " +
                                   "We will try to regenerate it now for you[/]");

            if (string.IsNullOrWhiteSpace(dbSettings))
            {
                if (GenerateConfig(MissingSettings.Database))
                {
                    additionalIssue =
                        "[red]Could not generate the Missing Database settings from the existing environment" +
                        "variables. Please recreate the docker container to fix this issue.[/]";
                    return false;
                }
            }

            if (string.IsNullOrWhiteSpace(jwtSettings))
            {
                if (!GenerateConfig(MissingSettings.JwtBearer)) return true;

                additionalIssue =
                    "[red]Could not generate the missing JwtBearer settings from the existing environment" +
                    "variables. Please recreate the docker container to fix this issue.[/]";

                return false;
            }
            
            if (string.IsNullOrWhiteSpace(sendGridToken))
            {
                if (!GenerateConfig(MissingSettings.SendGrid)) return true;

                additionalIssue =
                    "[red]Could not generate the missing Sendgrid settings from the existing environment" +
                    "variables. Please recreate the docker container to fix this issue.[/]";

                return false;
            }

            return true;
        }

        if (!File.Exists(SettingsFileName))
        {
            AnsiConsole.MarkupLine("[yellow]We could not find any existing configuration. Creating one now[/]");
                
            if (!GenerateConfig(MissingSettings.All))
            {
                additionalIssue =
                    "[red]Could not successfully generate client settings. " +
                    "Please ensure your input was correct and you didn't interrupt the configuration generation.[/]";
                return false;
            }
        }

        var fileText = File.ReadAllText(SettingsFileName);

        if (string.IsNullOrEmpty(fileText))
        {
            AnsiConsole.MarkupLine("[yellow]The configuration file is empty and needs to be regenerated[/]");

            if (!GenerateConfig(MissingSettings.All))
            {
                additionalIssue =
                    "[red]Could not successfully generate client settings. " +
                    "Please ensure your input was correct and you didn't interrupt the configuration generation.[/]";
                return false;
            }
        }
            
        JObject jObject = JObject.Parse(fileText);
        var jsonDbSettings = jObject.SelectToken(DbSettingsNode);
        var jsonJwtSettings = jObject.SelectToken(JwtSettingsNode);
        var jsonSendgridToken = jObject.SelectToken(SendGridEnvVar);

        if (jsonDbSettings == null)
        {
            AnsiConsole.MarkupLine("[yellow]We could not find the database settings in the configuration file. " +
                                   "Regenerating them now...[/]");
                
            if (!GenerateConfig(MissingSettings.JwtBearer))
            {
                additionalIssue =
                    "[red]Could not successfully regenerate database settings. " +
                    "Please ensure your input was correct and you didn't interrupt the configuration generation.[/]";
                return false;
            }
        }

        if (jsonJwtSettings == null)
        {
            AnsiConsole.MarkupLine("[yellow]We could not find the jwt settings in the configuration file. " +
                                   "Regenerating them now...[/]");

            if (GenerateConfig(MissingSettings.JwtBearer)) return true;

            additionalIssue =
                "[red]Could not successfully regenerate jwt settings. " +
                "Please ensure your input was correct and you didn't interrupt the configuration generation.[/]";

            return false;
        }
        
        if (jsonSendgridToken == null)
        {
            AnsiConsole.Markup("[yellow]We could not find your Sendgrid APi token. Asking you to add one now[/]");

            if (GenerateConfig(MissingSettings.SendGrid)) return true;
            
            additionalIssue = 
                "[red]Could not successfully generate your Sendgrid Api token. " +
                "Please ensure your input was correct and you didn't interrupt the configuration generation.[/]";
        }
        
        return true;
    }

    /// <summary>
    /// Loads the configuration
    /// </summary>
    /// <returns></returns>
    internal static void LoadConfiguration()
    {
        if (DockerEnvVars.InDocker)
        {
            var dbSettings = Environment.GetEnvironmentVariable(DockerEnvVars.DbSettingsEnvVar, EnvironmentVariableTarget.Machine);
            var jwtSettings = Environment.GetEnvironmentVariable(DockerEnvVars.JwtSettingsEnvVar, EnvironmentVariableTarget.Machine);
            var sendgridToken =
                Environment.GetEnvironmentVariable(DockerEnvVars.SendGridEnvVar, EnvironmentVariableTarget.Machine);

            if (string.IsNullOrWhiteSpace(dbSettings) || string.IsNullOrWhiteSpace(jwtSettings) || string.IsNullOrWhiteSpace(sendgridToken))
            {
                throw new Exception("The settings could not be found. Please ensure the docker container " +
                                    "was configured properly and that the environment User-Level " +
                                    "Environment-Variables with the name: {DbSettingsEnvVar} " +
                                    "and {JwtSettingsEnvVar} are set.");
            }

            JwtSettings = JsonConvert.DeserializeObject<JwtTokenConfig>(jwtSettings);
            DbSettings = JsonConvert.DeserializeObject<DbSettings>(dbSettings);
            Environment.SetEnvironmentVariable(SendGridEnvVar, sendgridToken, EnvironmentVariableTarget.Process);
            return;
        }

        if (File.Exists(SettingsFileName))
        {
            var configText = File.ReadAllText(SettingsFileName);

            if (string.IsNullOrWhiteSpace(configText))
            {
                throw new Exception("After reading the configuration file it was empty. Please ensure" +
                                    "the configuration is correct and properly stored.");
            }

            JObject jObject = JObject.Parse(configText);
            var dbSettings = jObject.SelectToken(DbSettingsNode);
                
            if (dbSettings != null)
            {
                var dbSettingsString =  dbSettings.ToObject<string>();
                DbSettings = JsonConvert.DeserializeObject<DbSettings>(dbSettingsString);
            }
            else
            {
                throw new Exception(
                    "Invalid formatting inside of the settings file. Please validate the settings are valid Json");
            }
                        

            var jwtSettings = jObject.SelectToken(JwtSettingsNode);
                
            if (jwtSettings != null)
            {
                var jwtSettingsString = jwtSettings.ToObject<string>();
                JwtSettings = JsonConvert.DeserializeObject<JwtTokenConfig>(jwtSettingsString);
            }
            else
            {
                throw new Exception(
                    "Invalid formatting inside of the settings file. Please validate the settings are valid Json");
            }

            var sendgridToken = jObject.SelectToken(SendGridEnvVar);
            if (sendgridToken != null)
            {
                var token = sendgridToken.ToObject<string>();
                Environment.SetEnvironmentVariable(SendGridEnvVar, token, EnvironmentVariableTarget.Process);
            }

        }
        else
        {
            throw new Exception(
                $"The settings file could not be read. Please ensure the settings " +
                $"file with the name {SettingsFileName} exists in the executable directory");
        }
    }

    /// <summary>
    /// Generates the configuration for the api
    /// </summary>
    /// <returns></returns>
    private static bool GenerateConfig(MissingSettings missingSettings)
    {
        Console.Clear();

        var dbConnectionConfig = string.Empty;
        var jwtBearerConfig = string.Empty;
        var sendgridConfig = string.Empty;
            
        if (DockerEnvVars.InDocker)
        {
            dbConnectionConfig = Environment.GetEnvironmentVariable(DockerEnvVars.DbSettingsEnvVar);
            jwtBearerConfig = Environment.GetEnvironmentVariable(DockerEnvVars.JwtSettingsEnvVar);
        }
        else
        {
            if (!File.Exists(SettingsFileName))
            {
                AnsiConsole.MarkupLine("[red]We could not find the settings file. Creating one now! The new file will be stored" +
                                       $"in the application directory with the name {SettingsFileName}.[/]");
                File.WriteAllText(SettingsFileName, string.Empty);
            }
            else
            {
                var configFileText = File.ReadAllText(SettingsFileName);

                if (string.IsNullOrWhiteSpace(configFileText))
                {
                    AnsiConsole.MarkupLine("Config file is empty. We'll create completely new settings now");
                }
                else
                {
                    JObject jObject = JObject.Parse(configFileText);
                    var dbSettings = jObject.Property(DbSettingsNode);
                        
                    if (dbSettings != null)
                    {
                        dbConnectionConfig = dbSettings.ToString();
                    }
                        

                    var jwtSettings = jObject.Property(JwtSettingsNode);
                        
                    if (jwtSettings != null)
                    {
                        jwtBearerConfig = jwtSettings.ToString();
                    }
                }
            }
        }

        if (missingSettings.HasFlag(MissingSettings.Database) || missingSettings.HasFlag(MissingSettings.All))
        {
            dbConnectionConfig = GenerateDatabaseConfig();
                
            if (string.IsNullOrWhiteSpace(dbConnectionConfig))
            {
                return false;
            }
        }
            
        if (missingSettings.HasFlag(MissingSettings.JwtBearer) || missingSettings.HasFlag(MissingSettings.All))
        {
            jwtBearerConfig = GenerateJwtConfig();
        }

        if (missingSettings.HasFlag(MissingSettings.SendGrid) || missingSettings.HasFlag(MissingSettings.All))
        {
            if (DockerEnvVars.InDocker)
            {
                throw new InvalidOperationException(
                    "Cannot regenerate the settings for sendgrid in docker. You need to specify the env var during creation of the container.");
            }
            else
            {
                while (true)
                {
                    var token = AnsiConsole.Ask<string>("Please enter your Twilio Sendgrid Token");
                
                    try
                    {
                        var client = new SendGridClient(token);
                    }
                    catch (Exception ex)
                    {
                       AnsiConsole.WriteException(ex);
                       var retry = AnsiConsole.Ask("Could not validate your token do you want to try again, or just continue" +
                                                   "with this token?", true);
                       if (retry)
                       {
                           continue;
                       }
                    }
                    
                    sendgridConfig = token;
                    break;
                }
            }
            
        }

        Console.Clear();

        var ruler = new Rule("[yellow]Configuration Setup[/]") {Alignment = Justify.Left};
        AnsiConsole.Write(ruler);
        ruler.Title = "[yellow]Finishing touches[/]";
        ruler.Alignment = Justify.Left;
        AnsiConsole.Write(ruler);


        AnsiConsole.MarkupLine("The generated configuration look like this:");

        if (missingSettings.HasFlag(MissingSettings.Database) || missingSettings.HasFlag(MissingSettings.All))
        {
            AnsiConsole.MarkupLine($"[blue]Database Settings: {dbConnectionConfig}[/]\n");
        }

        if (missingSettings.HasFlag(MissingSettings.JwtBearer) || missingSettings.HasFlag(MissingSettings.All))
        {
            AnsiConsole.MarkupLine($"[green]Jwt Settings: {jwtBearerConfig}[/]");
        }
        
        if (missingSettings.HasFlag(MissingSettings.SendGrid) || missingSettings.HasFlag(MissingSettings.All))
        {
            AnsiConsole.MarkupLine($"[green]Sendgrid Token: {sendgridConfig}[/]");
        }
        
        if (DockerEnvVars.InDocker)
        {
            Environment.SetEnvironmentVariable(DockerEnvVars.JwtSettingsEnvVar, jwtBearerConfig,
                EnvironmentVariableTarget.User);
            Environment.SetEnvironmentVariable(DockerEnvVars.DbSettingsEnvVar, dbConnectionConfig,
                EnvironmentVariableTarget.User);
        }
        else
        {
            if (!AnsiConsole.Confirm("Do you want to use the configured settings?"))
            {
                return false;
            }
        }

        if (!DockerEnvVars.InDocker)
        {
            var jObject1 = new JObject();
            jObject1.Add(DbSettingsNode, dbConnectionConfig);
            jObject1.Add(JwtSettingsNode, jwtBearerConfig);
            jObject1.Add(SendGridEnvVar, sendgridConfig);
            File.WriteAllText(SettingsFileName, JsonConvert.SerializeObject(jObject1, Formatting.Indented));
        }

        AnsiConsole.MarkupLine("Successfully created your configuration.\n" +
                               "Thank you for using Pamaxie's Services. If you require help using this service " +
                               "please see our wiki with all documentation at [blue]https://wiki.pamaxie.com[/]\n");

        if (DockerEnvVars.InDocker)
        {
            return true;
        }
        
        AnsiConsole.WriteLine("[yellow]We will now have to exit the program because we require a restart to load the config properly.[/]\n" +
                              "Press any key to exit...");
        Console.ReadKey();
        Environment.Exit(-1);
        return true;
    }
        
    /// <summary>
    /// This generates the Jwt Bearer configuration that the API should use
    /// </summary>
    /// <returns></returns>
    private static string GenerateJwtConfig()
    {
        var ruler = new Rule("[yellow]Configuration Setup[/]") {Alignment = Justify.Left};
        AnsiConsole.Write(ruler);
        ruler.Title = "[yellow]Jwt Bearer Setup[/]";
        AnsiConsole.Write(ruler);
        string secret;

        if (!DockerEnvVars.InDocker && !AnsiConsole.Confirm("Do you want to automatically generate a secret for the JWT bearer creation?"))
        {
            secret = AnsiConsole.Prompt(
                new TextPrompt<string>("Please enter the applications JWT bearer secret. \n" +
                                       "Please ensure its at least 16 Characters in length [yellow](preferably 64)[/]")
                    .Secret()
                    .PromptStyle("red")
                    .Validate(token =>
                    {
                        if (token.Length < 16)
                            return ValidationResult.Error(
                                "[red]The entered token was too short, please ensure its at least 16 Characters.[/]");
                        return ValidationResult.Success();
                    }));
        }

        secret = JwtTokenGenerator.GenerateSecret();    
        AnsiConsole.MarkupLine($"Your new Jwt Bearer secret is: {secret}");

        int timeout , longLivedTimeout;
            
        if (!DockerEnvVars.InDocker)
        {
            timeout = AnsiConsole.Ask<int>(
                "How long in minutes should the timeout for the Jwt bearer be? \n" +
                "[yellow]We usually recommend anywhere between 15 - 60 minutes lifespan[/]", 10);
        }
        else
        {
            var tokenTimeoutEnvVar = Environment.GetEnvironmentVariable(DockerEnvVars.TokenTimeoutSettingsEnvVar);
                
            if (string.IsNullOrWhiteSpace(tokenTimeoutEnvVar))
            {
                AnsiConsole.MarkupLine($"[yellow]Setting the default token lifespan of 60 minutes. This can be changed by creating a container with the flag:" +
                                       $"-e {DockerEnvVars.LongLivedTokenTimeoutSettingEnvVar}={{TimeoutInDays}}");
            }
                
            int.TryParse(tokenTimeoutEnvVar, out timeout);

            if (timeout <= 0)
            {
                AnsiConsole.MarkupLine("[red]Invalid token timeout specified. Please ensure the timeout is at least 1 minute[/]" +
                                       "(We recommend values between 15-60 minutes)");
            }
        }

        if (!DockerEnvVars.InDocker)
        {
            longLivedTimeout = AnsiConsole.Ask<int>(
                "How long in minutes should the timeout for the long lived Jwt bearer be? \n" +
                "[yellow]We recommend anywhere between 30-60 days of lifespan[/]", 30);
        }
        else
        {
            var longLivedTokenTimeoutEnvVar = Environment.GetEnvironmentVariable(DockerEnvVars.LongLivedTokenTimeoutSettingEnvVar);
                
            if (string.IsNullOrWhiteSpace(longLivedTokenTimeoutEnvVar))
            {
                AnsiConsole.MarkupLine($"[yellow]Setting the default long lived token lifespan of 60 days. This can be changed by creating a container with the flag:" +
                                       $"-e {DockerEnvVars.LongLivedTokenTimeoutSettingEnvVar}={{TimeoutInDays}}");
            }

            int.TryParse(longLivedTokenTimeoutEnvVar, out longLivedTimeout);

            if (longLivedTimeout <= 0)
            {
                AnsiConsole.MarkupLine("[red]Invalid long lived token timeout specified. Please ensure the timeout is at least 1 day[/]\n" +
                                       "(We recommend values between 30-60 days)");
            }
        }
            
        var authSettings = new JwtTokenConfig
        {
            Secret = secret,
            ExpiresInMinutes = timeout,
            LongLivedExpiresInDays = longLivedTimeout
        };

        return JsonConvert.SerializeObject(authSettings, Formatting.Indented);
    }

    /// <summary>
    /// This does some magic to automatically detect the different database drivers available and then tries to generate a config for them.
    /// </summary>
    /// <returns></returns>
    private static string GenerateDatabaseConfig()
    {
        //TODO: add downloading our native driver from a file server to reduce delivery package if someone needs their own database driver.
            
        Console.Clear();

        if (DockerEnvVars.InDocker)
        {
            //In a docker container we always assume our default driver if you require a different one please
            //change this line to your database driver Guid
            return DbDriverManager
                .LoadDatabaseDriver(new Guid("283f10c3-5022-4cda-a5a7-e491a1e84b32"))
                .Configuration.GenerateConfig();
        }
            
        var ruler = new Rule("[yellow]Database Configuration[/]") {Alignment = Justify.Left};
        AnsiConsole.Render(ruler);
        var drivers = DbDriverManager.LoadDatabaseDrivers();
        IPamaxieDatabaseDriver driver;
            
        if (drivers.Count() > 1)
        {
            var driverNames = drivers.Select(x => $"[yellow]{x.DatabaseTypeName} ;; \0{x.DatabaseTypeGuid}[/]").ToArray();
            var selectedDatabaseDriver = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Please select which database you want to use")
                    .PageSize(10)
                    .MoreChoicesText("[grey](Move up and down to show more database drivers)[/]")
                    .AddChoices(driverNames));
            var guid = new Guid(selectedDatabaseDriver.Split('\0').LastOrDefault() ?? string.Empty);
            driver = DbDriverManager.LoadDatabaseDriver(guid);
        }
        else
        {
            driver = drivers.First();
            AnsiConsole.MarkupLine($"Found only the following dB driver: [yellow]{driver.DatabaseTypeName} ;; \0 {driver.DatabaseTypeGuid}[/]");

            var useDriver = AnsiConsole.Confirm(
                $"Do you want to use this driver? (Not agreeing will exit this software since a driver is required");

            if (!useDriver)
            {
                return null;
            }
        }
            
        var generatedConfig = driver.Configuration.GenerateConfig();
        var dbObject = new DbSettings();
        dbObject.Settings = generatedConfig;
        dbObject.DatabaseDriverGuid = driver.DatabaseTypeGuid;
        return JsonConvert.SerializeObject(dbObject);
    }
}