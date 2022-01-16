using Newtonsoft.Json;
using Pamaxie.Database.Design;
using Pamaxie.Database.Extensions.ServerSide;
using Spectre.Console;
using StackExchange.Redis;
using System;

namespace Pamaxie.Database.Redis
{
    internal class PamaxieDatabaseConfig : IPamaxieDatabaseConfiguration
    {
        PamaxieDatabaseDriver owner;


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
            return string.Empty;
        }

        public void LoadConfig(string config)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return base.ToString();
        }
    }
}
