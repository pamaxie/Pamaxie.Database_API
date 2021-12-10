using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Pamaxie.Database.Design;
using Pamaxie.Database.Extensions.ServerSide;
using Spectre.Console;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pamaxie.Database.Redis
{
    internal class PamaxieDatabaseConfig : IPamaxieDatabaseConfiguration
    {
        PamaxieDatabaseDriver owner;
        ConfigurationOptions configurationOptions;


        /// <summary>
        /// Creates a new instance of the Pamaxie Database config for Redis
        /// </summary>
        /// <param name="owner"></param>
        internal PamaxieDatabaseConfig(PamaxieDatabaseDriver owner)
        {
            this.owner = owner;
        }

        /// <inheritdoc/>
        public Guid DatabaseDriverGuid { get => owner.DatabaseTypeGuid; }

        /// <inheritdoc/>
        public string GenerateConfig()
        {
            var ruler = new Rule(@"[red]Redis setup[/]");
            ruler.Alignment = Justify.Left;
            AnsiConsole.Render(ruler);
            AnsiConsole.WriteLine(
@"Hello and welcome, this is the Redis database setup guide.
We will guide you through the installation with ease.
Please ensure all configurations are setup correctly otherwise connecting to the database might not work.
If you make a mistake you will be able to corrrect it at the end of the setup. So no worries!");
            ConfigurationOptions options = new ConfigurationOptions();
            GenerateSentinelOptions(options);
            GenerateClientNameOptions(options);
            GeneratePasswordOptions(options);
            GenerateAllowAdminOptions(options);
            GenerateAzureOptions(options);
            GenerateSslOptions(options);
            string[] instances = GenerateInstances();

            while (true)
            {
                Console.Clear();
                ruler = new Rule(@"[red]Redis setup[/]");
                ruler.Alignment = Justify.Left;
                AnsiConsole.Render(ruler);
                DrawOptions(options, instances);
                DrawAdvancedOptions(options);

                string[] choices = new[] { "[Green]Confirm Settings[/]", "Advanced Options", "Edit Settings", "[Red]Cancel[/]" };
                var confirmSettings = AnsiConsole.Prompt(new SelectionPrompt<string>()
                                                    .Title("Please select one of these options")
                                                    .AddChoices(choices));


                if (confirmSettings == choices[1])
                {
                    OpenAdvancedOptionsMenu(options, instances);
                    continue;

                }
                else if (confirmSettings == choices[2])
                {
                    OpenEditOptionsMenu(options, ref instances);
                    continue;
                }
                else if (confirmSettings == choices[3])
                {
                    if (AnsiConsole.Ask("[red]Are you sure you wish to proceed. " +
                        "Cancelling this setup will exit this software.[/] " +
                        "We require a database to be setup for this software to work.", false))
                    {
                        AnsiConsole.Write("Exiting...");
                        Environment.Exit(-1);
                    }

                    continue;
                }
                else
                {
                    foreach(var item in instances)
                    {
                        options.EndPoints.Add(item);
                    }

                    PamaxieDatabaseClientSettings settings = new PamaxieDatabaseClientSettings();
                    settings.DatabaseDriverGuid = DatabaseDriverGuid;
                    settings.Settings = options.ToString();
                    return JsonConvert.SerializeObject(settings, Formatting.Indented);
                }
            }

        }

        /// <inheritdoc/>
        public void LoadConfig(string config)
        {
            configurationOptions = ConfigurationOptions.Parse(config);
        }

        /// <summary>
        /// Using the ToString to return this items string representation.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
            => configurationOptions.ToString();

        #region General Options Generation
        private void DrawOptions(ConfigurationOptions options, string[] instances)
        {
            var root = new Tree("Generated Database Options:");
            var databaseInstancesTreeRoot = root.AddNode("[yellow]Specified Redis Instances[/]");

            for (int i = 0; i < instances.Length; i++)
            {
                databaseInstancesTreeRoot.AddNode($"{i + 1} -- {instances[i]}");
            }

            var connectionParametersTreeRoot = root.AddNode("[yellow]Connection Parameters[/]");
            bool usingSentinel = !string.IsNullOrWhiteSpace(options.ServiceName);
            DrawBoolNode(connectionParametersTreeRoot, "Sentinel", usingSentinel);

            if (usingSentinel)
            {
                connectionParametersTreeRoot.AddNode($"Sentinal Host Name: {options.ServiceName}");
            }

            connectionParametersTreeRoot.AddNode($"Client Name: {options.ClientName}");
            DrawBoolNode(connectionParametersTreeRoot, nameof(options.Password), !string.IsNullOrWhiteSpace(options.Password));
            DrawBoolNode(connectionParametersTreeRoot, "Admin Mode", options.AllowAdmin);

            var isAzureInstance = options.DefaultVersion == new Version("3.0");
            DrawBoolNode(connectionParametersTreeRoot, "Azure Instance", isAzureInstance);

            if (isAzureInstance)
            {
                connectionParametersTreeRoot.AddNode($"Client Name: {options.DefaultVersion}");
                DrawBoolNode(connectionParametersTreeRoot, nameof(options.AbortOnConnectFail), options.AbortOnConnectFail);
            }

            DrawBoolNode(connectionParametersTreeRoot, nameof(options.Ssl), options.Ssl);
            AnsiConsole.Render(root);
        }

        private void OpenEditOptionsMenu(ConfigurationOptions options, ref string[] instances)
        {
            while (true)
            {
                Console.Clear();
                var ruler = new Rule(@"[red]Redis setup[/]");
                ruler.Alignment = Justify.Left;
                AnsiConsole.Render(ruler);

                DrawOptions(options, instances);
                DrawAdvancedOptions(options);

                string[] choices = new[] { "Sentinel", "Client Name", "Password", "Allow Admin", "Use Azure", "Use SSl", "Instances", "[red]Exit Options[/]" };
                var confirmSettings = AnsiConsole.Prompt(new SelectionPrompt<string>()
                                                    .Title("Please select which setting you would like to edit")
                                                    .AddChoices(choices));
                if (confirmSettings == choices[0])
                {
                    GenerateSentinelOptions(options);
                    Console.Clear();
                }
                else if (confirmSettings == choices[1])
                {
                    GenerateClientNameOptions(options);
                    Console.Clear();
                }
                else if (confirmSettings == choices[2])
                {
                    GeneratePasswordOptions(options);
                    Console.Clear();
                }
                else if (confirmSettings == choices[3])
                {
                    GenerateAllowAdminOptions(options);
                    Console.Clear();
                }
                else if (confirmSettings == choices[4])
                {
                    GenerateAzureOptions(options);
                    Console.Clear();
                }
                else if (confirmSettings == choices[5])
                {
                    GenerateSslOptions(options);
                    Console.Clear();
                }
                else if (confirmSettings == choices[6])
                {
                    instances = GenerateInstances();
                    Console.Clear();
                }
                else if (confirmSettings == choices[7])
                {
                    break;
                }
            }
        }

        private void GenerateSentinelOptions(ConfigurationOptions options)
        {
            if (AnsiConsole.Ask("Do you want to use a sentinel master service", false))
            {
                options.ServiceName = AnsiConsole.Ask<string>("Please enter your service name: ");
            }
            else
            {
                options.ServiceName = null;
            }
        }

        private void GenerateClientNameOptions(ConfigurationOptions options) 
            => options.ClientName = AnsiConsole.Ask<string>("Please enter a name for your client. It is used to identify this client within Redis", "Pamaxie");

        private void GeneratePasswordOptions(ConfigurationOptions options)
        {
            if (AnsiConsole.Ask("Does your service use a password for protection. [red] WE HIGHLY RECOMMNED SETTING ONE IF YOU HAVEN'T[/]", true))
            {
                while (true)
                {
                    var pw = AnsiConsole.Prompt(new TextPrompt<string>("Enter your password. (This will be stored in [red]cleartext[/] in the configuration): ").PromptStyle("red").Secret());
                    var pwConfirm = AnsiConsole.Prompt(new TextPrompt<string>("Enter your password. (Confirm): ").PromptStyle("red").Secret());
                    if (pw == pwConfirm)
                    {
                        options.Password = pw;
                        break;
                    }
                    else
                    {
                        AnsiConsole.WriteLine("Passwords do not match. Please try again");
                    }
                }
            }
            else
            {
                options.Password = null;
            }
        }

        private void GenerateAllowAdminOptions(ConfigurationOptions options) 
            => options.AllowAdmin = AnsiConsole.Ask("Do you want to enable Admin mode, we recommend doing this. This allows us to execute commands that are considered Risky", true);

        private void GenerateAzureOptions(ConfigurationOptions options)
        {
            if (AnsiConsole.Ask("Are you running this redis instance in Azure. This requires some adjustments we need to make in the configuration", false))
            {
                options.AbortOnConnectFail = true;
                options.DefaultVersion = new Version("3.0");
            }
            else
            {
                options.AbortOnConnectFail = false;
                options.DefaultVersion = new Version("2.0");
            }
        }

        private void GenerateSslOptions(ConfigurationOptions options) 
            => options.Ssl = AnsiConsole.Ask("Do you want SSL encryption to be used for connecting to the database. We recommend enabling this setting", true);

        private string[] GenerateInstances()
        {
            while (true)
            {
                int instanceCount = AnsiConsole.Prompt(new TextPrompt<int>("Please enter the amount of instances you have: "));
                if (instanceCount <= 0)
                {
                    AnsiConsole.WriteLine("We require one or more instances to be configured. The number you entered is less than that.");
                    continue;
                }

                string[] instances = new string[instanceCount];

                for (int i = 0; i < instanceCount; i++)
                {
                    short port = AnsiConsole.Ask<short>("Please enter the port of the service: ", 6379);
                    if (port <= 0)
                    {
                        AnsiConsole.WriteLine("Invalid port specified");
                        i--;
                        continue;
                    }
                    string hostName = AnsiConsole.Prompt(new TextPrompt<string>("Please enter the hostname of your redis instances: "));

                    if (string.IsNullOrWhiteSpace(hostName))
                    {
                        AnsiConsole.WriteLine("Invalid hostname");
                        i--;
                        continue;
                    }

                    instances[i] = $"{hostName}:{port}";
                }

                return instances;
            }
        }

        private void DrawBoolNode(TreeNode node, string parameterName, bool isTrue)
        {
            if (isTrue)
            {
                node.AddNode($"Using {parameterName}: [green]true[/]");
            }
            else
            {
                node.AddNode($"Using {parameterName}: [red]false[/]");
            }
        }
        #endregion General Options Generation

        #region Advanced Options Generation

        private void DrawAdvancedOptions(ConfigurationOptions options)
        {
            var root = new Tree("Advanced Database Options:");
            var connectionParametersTreeRoot = root.AddNode("[yellow]Connection Parameters[/]");


            connectionParametersTreeRoot.AddNode($"Connection Timeout: {options.ConnectTimeout} ms");
            connectionParametersTreeRoot.AddNode($"Connection Retry Attempts: {options.ConnectRetry}");
            DrawBoolNode(connectionParametersTreeRoot, nameof(options.CheckCertificateRevocation), options.CheckCertificateRevocation);
            connectionParametersTreeRoot.AddNode($"Channel Prefix: {options.ChannelPrefix}");
            connectionParametersTreeRoot.AddNode($"Default Database Index: {options.DefaultDatabase}");
            AnsiConsole.Render(root);
        }

        private void OpenAdvancedOptionsMenu(ConfigurationOptions options, string[] instances)
        {
            while (true)
            {
                Console.Clear();
                var ruler = new Rule(@"[red]Redis setup[/]");
                ruler.Alignment = Justify.Left;
                AnsiConsole.Render(ruler);

                DrawOptions(options, instances);
                DrawAdvancedOptions(options);

                string[] choices = new[] { "Connection Timeout", "Reconnection Attempts", "Check Certificate Revocation", "Channel Prefix", "Default Database", "[red]Exit Advanced Options[/]" };
                var confirmSettings = AnsiConsole.Prompt(new SelectionPrompt<string>()
                                                    .Title("Please select which setting you would like to edit")
                                                    .AddChoices(choices));
                if (confirmSettings == choices[0])
                {
                    GenerateConnectionTimeout(options);
                    Console.Clear();
                }
                else if (confirmSettings == choices[1])
                {
                    GenerateReconnectionAttempts(options);
                    Console.Clear();
                }
                else if (confirmSettings == choices[2])
                {
                    GenerateCheckCertificateRevocation(options);
                    Console.Clear();
                }
                else if (confirmSettings == choices[3])
                {
                    GenerateChannelPrefix(options);
                    Console.Clear();
                }
                else if (confirmSettings == choices[4])
                {
                    GenerateDefaultDatabase(options);
                    Console.Clear();
                }
                else if (confirmSettings == choices[5])
                {
                    break;
                }
            }
        }

        private void GenerateConnectionTimeout(ConfigurationOptions options)
            => options.ConnectTimeout = AnsiConsole.Ask("Please specify a conneciton timeout (ms) for connect operations", 5000);

        private void GenerateReconnectionAttempts(ConfigurationOptions options)
            => options.ConnectRetry = AnsiConsole.Ask("Please specify the number of retry attempts that are doing during connection", 3);

        private void GenerateCheckCertificateRevocation(ConfigurationOptions options)
            => options.CheckCertificateRevocation = AnsiConsole.Ask("Please specify whether the certificate revocation list is checked during authentication.", true);

        private void GenerateChannelPrefix(ConfigurationOptions options)
            => options.ChannelPrefix = AnsiConsole.Ask<string>("Please specify a channel prefix that should be used during pub/sub operations", null);

        private void GenerateDefaultDatabase(ConfigurationOptions options)
            => options.DefaultDatabase = AnsiConsole.Ask("Default database index (please make sure this does not exceed the amount of databases you have)", 1) - 1;

        #endregion
    }
}
