using Pamaxie.Database.Design;
using StackExchange.Redis;
using System;
using Pamaxie.Database.Extensions.ServerSide;
using Pamaxie.Database.Redis.DataInteraction;

namespace Pamaxie.Database.Redis
{
    internal sealed class PamaxieDatabaseService : IPamaxieDatabaseService
    {
        PamaxieDatabaseDriver owner;

        internal PamaxieDatabaseService(PamaxieDatabaseDriver owner)
        {
            this.owner = owner;

            Projects = new PamProjectDataInteraction(this);
            Orgs = new PamOrgDataInteraction(this);
            Users = new PamUserDataInteraction(this);
            Scans = new PamScanDataInteraction(this);
        }

        /// <inheritdoc cref="IPamaxieDatabaseService.Projects"/>
        public IPamProjectInteraction Projects { get; }

        /// <inheritdoc cref="IPamaxieDatabaseService.Users"/>
        public IPamUserInteraction Users { get; }
        
        /// <inheritdoc cref="IPamaxieDatabaseService.Orgs"/>
        public IPamOrgInteraction Orgs { get; }
        
        /// <inheritdoc cref="IPamaxieDatabaseService.Scans"/>
        public IPamScanInteraction Scans { get; }

        /// <inheritdoc cref="IPamaxieDatabaseService.CheckDatabaseContext"/>
        public bool CheckDatabaseContext(IPamaxieDatabaseConfiguration connectionParams)
        {
            if (connectionParams == null) throw new ArgumentNullException(nameof(connectionParams));
            
            using var conn = ConnectionMultiplexer.Connect(connectionParams.ToString());
            return conn.IsConnected;
        }

        /// <inheritdoc cref="IPamaxieDatabaseService.ExecuteCommand"/>
        public string ExecuteCommand(IPamaxieDatabaseConfiguration connectionParams, string command)
        {
            throw new NotSupportedException("This function is not yet supported with this driver");
        }
    }
}
