using System;

namespace Pamaxie.Database.Design
{
    /// <summary>
    /// Service responsible for handling interaction with the database or database api. This automatically detects
    /// the connection context
    /// </summary>
    public interface IPamaxieDatabaseDriver
    {
        /// <summary>
        /// Friendly name for the database driver
        /// </summary>
        string DatabaseTypeName { get; }

        /// <summary>
        /// Unique Guid to differentiate between Databases
        /// </summary>
        Guid DatabaseTypeGuid { get; }

        /// <summary>
        /// Database Context for connecting with the database
        /// </summary>
        IPamaxieDatabaseConfiguration Configuration { get; }

        /// <summary>
        /// Service for connecting with the database
        /// </summary>
        IPamaxieDatabaseService Service { get; }
    }
}