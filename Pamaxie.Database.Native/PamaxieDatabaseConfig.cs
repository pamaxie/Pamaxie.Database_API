using System;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Pamaxie.Database.Extensions;
using Spectre.Console;
using StackExchange.Redis;

namespace Pamaxie.Database.Native;

/// <inheritdoc cref="IPamaxieDatabaseConfiguration"/>
internal class PamaxieDatabaseConfig : IPamaxieDatabaseConfiguration
{
    private readonly PamaxieDatabaseDriver _owner;

    private const string SqlEnvVarName = "PamPgSqlDbVar";
    private const string RedisEnvVarName = "PamRedisDbVar";
    private bool InDocker => Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";

    /// <summary>
    /// Creates a new instance of the Pamaxie Database config for Redis
    /// </summary>
    /// <param name="owner"></param>
    internal PamaxieDatabaseConfig(PamaxieDatabaseDriver owner)
    {
        this._owner = owner;
    }

    /// <inheritdoc cref="IPamaxieDatabaseConfiguration.Db1Config"/>
    public string Db1Config { get; private set; }

    /// <inheritdoc cref="IPamaxieDatabaseConfiguration.Db2Config"/>
    public string Db2Config { get; private set; }

    /// <inheritdoc cref="IPamaxieDatabaseConfiguration.DatabaseDriverGuid"/>
    public Guid DatabaseDriverGuid
    {
        get => _owner.DatabaseTypeGuid;
    }

    /// <inheritdoc cref="IPamaxieDatabaseConfiguration.GenerateConfig"/>
    public string GenerateConfig()
    {
        if (InDocker)
        {
            var dockerPgSqlConf = Environment.GetEnvironmentVariable(SqlEnvVarName);

            if (string.IsNullOrWhiteSpace(dockerPgSqlConf))
            {
                InvalidConfigError();
            }

            var dockerRedisConf = Environment.GetEnvironmentVariable(RedisEnvVarName);

            if (string.IsNullOrEmpty(dockerRedisConf))
            {
                InvalidConfigError();
            }

            Db1Config = dockerRedisConf;
            Db2Config = dockerPgSqlConf;
        }
        else
        {
            AnsiConsole.MarkupLine(
                "Thank you for choosing Pamaxie. To run our Database Service you require a [blue]Postgres[/] and [red]Redis[/] instance.\n" +
                "Both [red]Redis[/] and [blue]Postgres[/] require a connection string to be created so we can connect to them\n" +
                "[yellow]Please see our wiki ([blue]https://wiki.pamaxie.com[/]) if you require help during setup[/]");
            if (!AnsiConsole.Confirm(
                    "Do you want to enter the connection strings now, or exit so you can setup the servers required?"))
            {
                Environment.Exit(-1);
            }

            string pgString;
            do
            {
                Console.Clear();
                AnsiConsole.MarkupLine("Please enter your [red]Redis[/] connection string (may not be empty)");
                pgString = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(pgString))
                {
                    AnsiConsole.MarkupLine(
                        "You may not enter an empty connection string. Please press any key to try again.");
                    Console.ReadKey();
                    continue;
                }

                AnsiConsole.MarkupLine(
                    "[yellow]We will now test if we can connect to your redis instance with your connection string.[/]");

                var wasSuccess = false;
                var retry = false;

                do
                {
                    try
                    {
                        var connection = ConnectionMultiplexer.Connect(pgString);
                        wasSuccess = connection.IsConnected;
                        AnsiConsole.MarkupLine("[green] Successfully Connected![/]\n " +
                                               "Press any key to enter the connection string for [blue]postgres[/].");
                        Console.ReadKey();
                    }
                    catch (Exception ex)
                    {
                        AnsiConsole.WriteException(ex);
                        retry = AnsiConsole.Confirm(
                            "Do you want to retry the connection or enter a new connection string?");
                    }
                } while (retry);

                if (wasSuccess)
                {
                    break;
                }
            } while (true);

            string redisString;

            do
            {
                Console.Clear();
                AnsiConsole.MarkupLine("Please enter your [blue]Postgres[/] connection string (may not be empty)");
                redisString = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(redisString))
                {
                    AnsiConsole.MarkupLine(
                        "You may not enter an empty connection string. Please press any key to try again.");
                    Console.ReadKey();
                    continue;
                }

                AnsiConsole.MarkupLine(
                    "[yellow]We will now test if we can connect to your postgres instance with your connection string.[/]");
                PgSqlContext.SqlConnectionString = redisString;

                var retry = false;
                var wasSuccess = false;
                do
                {
                    try
                    {
                        using PgSqlContext sqlContext = new PgSqlContext();
                        var isNpgsql = sqlContext.Database.CanConnect();

                        if (isNpgsql)
                        {
                            AnsiConsole.MarkupLine("[green] Successfully Connected![/]\n" +
                                                   "Press any key to view your connection strings and exit setup.");
                            Console.ReadKey();
                            wasSuccess = true;
                        }
                        else
                        {
                            AnsiConsole.MarkupLine(
                                "[red]Invalid configuration.[/] Could not connect to the database via the specified" +
                                "connection string.");
                            retry = AnsiConsole.Confirm(
                                "Do you want to retry the connection or enter a new connection string?");
                        }
                    }
                    catch (Exception ex)
                    {
                        AnsiConsole.WriteException(ex);
                        retry = AnsiConsole.Confirm(
                            "Do you want to retry the connection or enter a new connection string?");
                    }
                } while (retry);

                if (wasSuccess)
                {
                    break;
                }
            } while (true);

            Db1Config = redisString;
            Db2Config = pgString;

            AnsiConsole.MarkupLine("[green]Pamaxie's Native DB driver was successfully configured.[/]");
        }

        AnsiConsole.MarkupLine($"The generated configuration is:\n {this.ToString()}");
        AnsiConsole.MarkupLine("Press any key to continue");
        Console.ReadKey();
        return this.ToString();
    }

    private static void InvalidConfigError()
    {
        AnsiConsole.MarkupLine(
            $"Please ensure the {SqlEnvVarName} and {RedisEnvVarName} variable are set and properly configured." +
            $"Make sure during docker run you specify -e {SqlEnvVarName}=<PGSqlConnectionString> and -e {RedisEnvVarName}=<RedisConnectionString>. " +
            $"For more detail visit our wiki at: https://wiki.pamaxie.com");
        Environment.Exit(-1);
    }


    /// <inheritdoc cref="IPamaxieDatabaseConfiguration.LoadConfig"/>
    public void LoadConfig(string config)
    {
        dynamic pConfig = JObject.Parse(config);
        Db1Config = pConfig.Db1Config;
        Db2Config = pConfig.Db2Config;
    }

    /// <inheritdoc cref="IPamaxieDatabaseConfiguration.ToString"/>
    public override string ToString()
    {
        var jObject = new JObject();
        jObject.Add(nameof(Db1Config), Db1Config);
        jObject.Add(nameof(Db2Config), Db2Config);

        if (InDocker)
        {
            return JsonConvert.SerializeObject(jObject);
        }

        return JsonConvert.SerializeObject(jObject, Formatting.Indented);
    }
}