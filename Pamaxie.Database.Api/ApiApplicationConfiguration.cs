using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Pamaxie.Authentication;
using Pamaxie.Database.Extensions;
using Pamaxie.Database.Extensions.ServerSide;
using Pamaxie.Helpers;
using Spectre.Console;
using System;
using System.IO;
using System.Linq;

namespace Pamaxie.Database.Api
{
    /// <summary>
    /// Configuration setup for the Database Api
    /// </summary>
    public class ApiApplicationConfiguration
    {
        /// <summary>
        /// Settings that hold the configuration for database access
        /// </summary>
        public const string DbSettingsEnvVar = "7812_PamaxieDbApi_DbSettings";

        /// <summary>
        /// Settings that hold the configuration for the jwt settings
        /// </summary>
        public const string JwtSettingsEnvVar = "7812_PamaxieDbApi_AuthSettings";

        /// <summary>
        /// Holds the Database settings
        /// </summary>
        public static string DbSettings;

        /// <summary>
        /// Holds the settings for Jwt token Generation
        /// </summary>
        public static string JwtSettings;

        /// <summary>
        /// Validates that the settings exist and are correct
        /// </summary>
        /// <returns></returns>
        public static bool ValidateConfiguration(IConfiguration configuration, out string additionalIssue)

        {
            var ruler = new Rule("[blue]Pamaxie Database API[/]");
            ruler.Alignment = Justify.Left;
            AnsiConsole.Render(ruler);


            additionalIssue = string.Empty;

            var dbSettings = Environment.GetEnvironmentVariable(DbSettingsEnvVar, EnvironmentVariableTarget.User);
            var jwtSettings = Environment.GetEnvironmentVariable(JwtSettingsEnvVar, EnvironmentVariableTarget.User);
            if (string.IsNullOrWhiteSpace(jwtSettings) || string.IsNullOrWhiteSpace(dbSettings))
            {
                ruler = new Rule("Settings validation");
                ruler.Alignment = Justify.Left;
                AnsiConsole.Render(ruler);

                if (!AnsiConsole.Confirm("[yellow]It seems like parts of the conifugration are missing or it hasn't been created yet. \n" +
                    "We require a configuration for the JwtBearer settings and database to run this software. \n" +
                    "Do you want to create the missing settings now?[/]"))
                {
                    AnsiConsole.WriteLine("No configuration exists, please manually or automatically create a configuration before continuing. Startup failed.");
                    return false;
                }

                MissingSettings missingSettings = 0;

                if (string.IsNullOrWhiteSpace(dbSettings))
                {
                    missingSettings += (int)MissingSettings.Database;
                }
                if (string.IsNullOrWhiteSpace(jwtSettings))
                {
                    missingSettings += (int)MissingSettings.JwtBearer;
                }

                while (true)
                {
                   if (GenerateConfig(missingSettings))
                    {
                        break;
                    }
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
            DbSettings = Environment.GetEnvironmentVariable(DbSettingsEnvVar, EnvironmentVariableTarget.User);
            JwtSettings = Environment.GetEnvironmentVariable(JwtSettingsEnvVar, EnvironmentVariableTarget.User);
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
