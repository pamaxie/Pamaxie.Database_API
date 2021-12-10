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
        /// Name for the type of database that is used
        /// </summary>
        string DatabaseTypeName { get; }

        /// <summary>
        /// Unique Identifier for distincting between types of the same name
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