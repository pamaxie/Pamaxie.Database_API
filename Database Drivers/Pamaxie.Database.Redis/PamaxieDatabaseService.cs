using Pamaxie.Database.Design;
using Pamaxie.Database.Redis.DataInteraction;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pamaxie.Database.Redis
{
    internal sealed class PamaxieDatabaseService : IPamaxieDatabaseService
    {
        PamaxieDatabaseDriver owner;

        internal PamaxieDatabaseService(PamaxieDatabaseDriver owner)
        {
            this.owner = owner;
            PamaxieUserData = new PamaxieUserDataInteraction(owner);
            PamaxieApplicationData = new PamaxieApplicationDataInteraction(owner);

        }

        /// <inheritdoc cref="IPamaxieDatabaseService.PamaxieApplicationData"/>
        public IPamaxieApplicationDataInteraction PamaxieApplicationData { get; }

        /// <inheritdoc cref="IPamaxieDatabaseService.PamaxieUserData"/>
        public IPamaxieUserDataInteraction PamaxieUserData { get; }

        /// <inheritdoc cref="IPamaxieDatabaseService.CheckDatabaseContext(IDatabaseContext)"/>
        public bool CheckDatabaseContext(IPamaxieDatabaseConfiguration connectionParams)
        {
            if (connectionParams == null)
            {
                throw new ArgumentNullException("The database configuration options must be reached in when calling this method");
            }

            using var conn = ConnectionMultiplexer.Connect(connectionParams.ToString());
            return conn.IsConnected;
        }

        /// <inheritdoc cref="IPamaxieDatabaseService.ExecuteCommand(IDatabaseContext, string)"/>
        public string ExecuteCommand(IPamaxieDatabaseConfiguration connectionParams, string command)
        {
            throw new NotSupportedException("This function is not yet supported with this driver");
        }
    }
}
