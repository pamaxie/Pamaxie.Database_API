using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pamaxie.Database.Design
{
    /// <summary>
    /// This defines how the database config should be handled. <see cref="GenerateConfig"/> requires some special attention, so the json that comes out of it can be parsed by our services.
    /// For now please look at our services how we structure our JObjects if you want to build your own DatabaseConfig. We will provide help in the documentation once it is written. (https://wiki.pamaxie.com)
    /// </summary>
    public interface IPamaxieDatabaseConfiguration
    {
        /// <summary>
        /// Guid of the database driver. This shouldn't be changed once set
        /// </summary>
        public Guid DatabaseDriverGuid { get; }

        /// <summary>
        /// Generates a configuration for the user (preferably with the user being able to set certain settings)
        /// </summary>
        /// <returns></returns>
        public string GenerateConfig();

        /// <summary>
        /// Loads a configuration for the user from a string.
        /// </summary>
        /// <param name="config"></param>
        void LoadConfig(string config);
    }
}
