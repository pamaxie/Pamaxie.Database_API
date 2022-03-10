using System;

namespace Pamaxie.Database.Extensions
{
    /// <summary>
    /// This defines how the database config should be handled. <see cref="GenerateConfig"/> requires some special attention, so the json that comes out of it can be parsed by our services.
    /// For now please look at our services how we structure our JObjects if you want to build your own DatabaseConfig. We will provide help in the documentation once it is written. (https://wiki.pamaxie.com)
    /// </summary>
    public interface IPamaxieDatabaseConfiguration
    {
        /// <summary>
        /// Configuration of the primary database
        /// </summary>
        public string Db1Config { get; }
        
        /// <summary>
        /// Configuration of the secondary database (optional db that can be used to separate data on a second database type)
        /// Mostly used for us to store Redis configurations vs PgSql configs
        /// </summary>
        public string Db2Config { get; }
        
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
