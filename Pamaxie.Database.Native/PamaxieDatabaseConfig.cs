using System;
using System.Diagnostics;
using System.Text.Json.Nodes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Pamaxie.Database.Extensions;

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
    public Guid DatabaseDriverGuid { get => _owner.DatabaseTypeGuid; }

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
            Console.WriteLine("Thank you for choosing Pamaxie. To run our Database Service you require a Postgres and redis instance." +
                              "Please see our wiki on how to configure them and how to get a connection string");
            Console.WriteLine("Press any key to start");
            Console.ReadKey();

            string pgstring;
            do
            {
                Console.Clear();
                Console.WriteLine("Please enter your Postgres connection string (may not be empty)");
                pgstring = Console.ReadLine();
            } while (string.IsNullOrWhiteSpace(pgstring));

            string redisString;
            do
            {
                Console.Clear();
                Console.WriteLine("Please enter your Postgres connection string (may not be empty)");
                redisString = Console.ReadLine();
            } while (string.IsNullOrWhiteSpace(redisString));

            Db1Config = redisString;
            Db2Config = pgstring;
            
            Console.WriteLine("Pamaxie's Native DB driver was successfully configured.");
        }
        
        return this.ToString();
    }
        
    private static void InvalidConfigError()
        => throw new InvalidOperationException($"Please ensure the {SqlEnvVarName} and {RedisEnvVarName} variable are set and properly configured." +
                                               $"Make sure during docker run you specify -e {SqlEnvVarName}=<PGSqlConnectionString> and -e {RedisEnvVarName}=<RedisConnectionString> for more detail please" +
                                               $"visit our wiki at \n>> https://wiki.pamaxie.com\n and go to the \"Self Hosting\" section. Please know that we do not provide support apart from our " +
                                               $"wiki entries for self hosting instances for free. We apologize for this inconvenience.");
        
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
        var jObj = new JsonObject
        {
            {nameof(Db1Config), Db1Config},
            {nameof(Db2Config), Db2Config}
        };

        return jObj.ToJsonString();
    }
}