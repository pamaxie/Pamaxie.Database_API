using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Pamaxie.Authentication;
using Pamaxie.Database.Extensions;
using Spectre.Console;
using System;
using System.IO;
using System.Linq;
using System.Net.Mime;
using Tomlyn;

namespace Pamaxie.Database.Api
{
    /// <summary>
    /// Configuration setup for the Database Api
    /// </summary>
    public class AppConfigManagment
    {
        
        internal static bool InDocker => Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";
        
        /// <summary>
        /// Settings that hold the configuration for database access
        /// </summary>
        internal const string DbSettingsEnvVar = "7812_PamaxieDbApi_DbSettings";

        /// <summary>
        /// Settings that hold the configuration for the jwt settings
        /// </summary>
        internal const string JwtSettingsEnvVar = "7812_PamaxieDbApi_AuthSettings";
        
        /// <summary>
        /// Settings file name for getting it
        /// </summary>
        internal const string SettingsFileName = "PamSettings.conf";

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
            AnsiConsole.Write("[green]Validating Config...[/]");

            if (InDocker)
            {
                var dbSettings = Environment.GetEnvironmentVariable(DbSettingsEnvVar, EnvironmentVariableTarget.User);
                var jwtSettings = Environment.GetEnvironmentVariable(JwtSettingsEnvVar, EnvironmentVariableTarget.User);

                if (string.IsNullOrWhiteSpace(dbSettings) || string.IsNullOrWhiteSpace(jwtSettings))
                {
                    AnsiConsole.Write("[yellow]We could not find part of your configuration. " +
                                      "We will try to regenerate it now for you[/]");

                    if (string.IsNullOrWhiteSpace(dbSettings))
                    {
                        if (GenerateConfig(MissingSettings.Database))
                        {
                            additionalIssue =
                                "Could not generate the Missing Database settings from the existing environment" +
                                "variables. Please recreate the docker container to fix this issue.";
                            return false;
                        }
                    }

                    if (string.IsNullOrWhiteSpace(jwtSettings))
                    {
                        if (GenerateConfig(MissingSettings.JwtBearer))
                        {
                            additionalIssue =
                                "Could not generate the missing JwtBearer settings from the existing environment" +
                                "variables. Please recreate the docker container to fix this issue.";
                            return false;
                        }
                    }
                }

                return true;
            }

            if (!File.Exists(SettingsFileName))
            {
                AnsiConsole.Write("[yellow]We could not find any existing configuration. Creating one now[/]");
                
                if (!GenerateConfig(MissingSettings.None))
                {
                    additionalIssue =
                        "Could not successfully generate client settings. " +
                        "Please ensure your input was correct and you didn't interrupt the configuration generation.";
                    return false;
                }
            }
            
            

            return true;
        }

        /// <summary>
        /// Loads the configuration
        /// </summary>
        /// <returns></returns>
        internal static void LoadConfiguration()
        {
            if (InDocker)
            {
                var dbSettings = Environment.GetEnvironmentVariable(DbSettingsEnvVar, EnvironmentVariableTarget.Machine);
                var jwtSettings = Environment.GetEnvironmentVariable(JwtSettingsEnvVar, EnvironmentVariableTarget.Machine);
                
                if (string.IsNullOrWhiteSpace(dbSettings) || string.IsNullOrWhiteSpace(jwtSettings))
                {
                    throw new Exception("The settings could not be found. Please ensure the docker container " +
                                        "was configured properly and that the enviorement User-Level " +
                                        "Environment-Variables with the name: {DbSettingsEnvVar} " +
                                        "and {JwtSettingsEnvVar} are set.");
                }

                JwtSettings = JsonConvert.DeserializeObject<JwtTokenConfig>(jwtSettings);
                DbSettings = JsonConvert.DeserializeObject<DbSettings>(dbSettings);
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
                
                var settingsModel = Toml.ToModel(configText);
                var dbSettings = (string) settingsModel["DbSettings"]!;
                var jwtSettings = (string) settingsModel["JwtSettings"]!;

                if (string.IsNullOrWhiteSpace(dbSettings) || (string.IsNullOrWhiteSpace(jwtSettings)))
                {
                    throw new Exception("After reading the database file we could not find the configuration " +
                                        "path for the Database or Jwt Settings. Please ensure both of them exist.");
                }

                DbSettings = Toml.ToModel<DbSettings>(dbSettings);
                JwtSettings = Toml.ToModel<JwtTokenConfig>(jwtSettings);
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
            string databaseConnectionString = Environment.GetEnvironmentVariable(DbSettingsEnvVar);
            if (missingSettings.HasFlag(MissingSettings.Database))
            {
                databaseConnectionString = GenerateDatabaseConfig();
            }

            string jwtBearerConfig = Environment.GetEnvironmentVariable(JwtSettingsEnvVar);
            if (missingSettings.HasFlag(MissingSettings.JwtBearer))
            {
                jwtBearerConfig = GenerateJwtConfig();
            }
            Console.Clear();

            var ruler = new Rule("[yellow]Configuration Setup[/]");
            ruler.Alignment = Justify.Left;
            AnsiConsole.Render(ruler);
            ruler.Title = "[yellow]Finishing touches[/]";
            ruler.Alignment = Justify.Left;
            AnsiConsole.Render(ruler);


            AnsiConsole.WriteLine("The generated configuration look like this:");

            if (missingSettings.HasFlag(MissingSettings.Database))
            {
                AnsiConsole.Render(new Markup($"[blue]Database Settings: {databaseConnectionString}[/]\n")); ;
            }

            if (missingSettings.HasFlag(MissingSettings.JwtBearer))
            {
                AnsiConsole.Render(new Markup($"[green]Jwt Settings: {jwtBearerConfig}[/]\n"));
            }
            
            if (AnsiConsole.Ask("Do you want to use the configured settings?", true))
            {
                if (missingSettings.HasFlag(MissingSettings.JwtBearer))
                {
                    Environment.SetEnvironmentVariable(JwtSettingsEnvVar, jwtBearerConfig, EnvironmentVariableTarget.User);
                }


                if (missingSettings.HasFlag(MissingSettings.Database))
                {
                    Environment.SetEnvironmentVariable(DbSettingsEnvVar, databaseConnectionString, EnvironmentVariableTarget.User);
                }

                AnsiConsole.Render(new Markup("We stored the configuration in the enviorement variables for you.\n" +
                    "Thank you for using Pamaxies Services.If you require help using this service please see our wiki at [blue]https://wiki.pamaxie.com[/]\n"));
                return true;
            }

            return false;
        }

        [Flags]
        private enum MissingSettings : ushort
        {
            None = 0,
            Database = 1,
            JwtBearer = 2
        }

        /// <summary>
        /// This generates the Jwt Bearer configuration that the API should use
        /// </summary>
        /// <returns></returns>
        private static string GenerateJwtConfig()
        {
            var ruler = new Rule("[yellow]Configuration Setup[/]");
            ruler.Alignment = Justify.Left;
            AnsiConsole.Render(ruler);
            ruler.Title = "[yellow]Jwt Bearer Setup[/]";
            AnsiConsole.Render(ruler);
            string secret;

            if (!AnsiConsole.Confirm("Do you want to automatically generate a secret for the JWT bearer creation?"))
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

            if (AnsiConsole.Confirm($"Should we show you the token now? [yellow]Otherwise it will be visible in the enviorement varibles under the envvar {JwtSettingsEnvVar}[/]", 
                                    false))
            {
                AnsiConsole.WriteLine(secret);
            }

            var timeout = AnsiConsole.Ask<int>(
                "How long in minutes should the timeout for the Jwt bearer be? \n" +
                "[yellow]We usually recommend anywhere between 5 - 15 minutes lifespan[/]", 10);

            JwtTokenConfig authSettings = new JwtTokenConfig();
            authSettings.Secret = secret;
            authSettings.ExpiresInMinutes = timeout;

            return JsonConvert.SerializeObject(authSettings, Formatting.Indented);
        }

        /// <summary>
        /// This does some magic to automatically detect the different database drivers available and then tries to generate a config for them.
        /// </summary>
        /// <returns></returns>
        private static string GenerateDatabaseConfig()
        {
            Console.Clear();
            var ruler = new Rule("[yellow]Database Configuration[/]");
            ruler.Alignment = Justify.Left;
            AnsiConsole.Render(ruler);

            var drivers = DbDriverManager.LoadDatabaseDrivers();
            var driverNames = drivers.Select(x => $"{x.DatabaseTypeName} ;; \0{x.DatabaseTypeGuid}").ToArray();

            var selectedDatabaseDriver = AnsiConsole.Prompt(
                        new SelectionPrompt<string>()
                            .Title("Please select which database you want to use")
                            .PageSize(10)
                            .MoreChoicesText("[grey](Move up and down to show more database drivers)[/]")
                            .AddChoices(driverNames));

            var guid = new Guid(selectedDatabaseDriver.Split('\0').LastOrDefault());
            var driver = drivers.FirstOrDefault(x => x.DatabaseTypeGuid == guid);
            return driver.Configuration.GenerateConfig();
        }
    }
}
