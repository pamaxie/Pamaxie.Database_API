using Pamaxie.Database.Design;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pamaxie.Database.Redis
{
    /// <summary>
    /// Database driver we use for Pamaxie. Please contact us if you want to setup your own
    /// instances and require your own DB driver. We do not currently support that scenario.
    /// <inheritdoc/>
    /// </summary>
    public sealed class PamaxieDatabaseDriver : IPamaxieDatabaseDriver
    {
        public PamaxieDatabaseDriver()
        {
            this.DatabaseTypeGuid = new Guid("283f10c3-5022-4cda-a5a7-e491a1e84b32");
            Service = new PamaxieDatabaseService(this);
            Configuration = new PamaxieDatabaseConfig(this);
        }

        /// <inheritdoc cref="IPamaxieDatabaseDriver.DatabaseTypeName"/>
        public string DatabaseTypeName => "Native";

        /// <inheritdoc cref="IPamaxieDatabaseDriver.DatabaseTypeGuid"/>
        public Guid DatabaseTypeGuid { get; }

        /// <inheritdoc cref="IPamaxieDatabaseDriver.Configuration"/>
        public IPamaxieDatabaseConfiguration Configuration { get; }

        /// <inheritdoc <see cref="IPamaxieDatabaseDriver.Service"/>/>
        public IPamaxieDatabaseService Service { get; }
    }
}
