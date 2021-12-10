namespace Pamaxie.Database.Design
{
    public interface IPamaxieDatabaseService
    {
        /// <summary>
        /// Service for accessing pamaxies application data
        /// </summary>
        IPamaxieApplicationDataInteraction PamaxieApplicationData { get; }

        /// <summary>
        /// Service for accessing pamaxies user data
        /// </summary>
        IPamaxieUserDataInteraction PamaxieUserData { get; }

        /// <summary>
        /// Validates the connection to the database with the reached in database context
        /// </summary>
        /// <param name="connectionParams">The connection parameters for establishing a connection with the database</param>
        /// <returns><see cref="bool"/> if the connection with the database was successful</returns>
        bool CheckDatabaseContext(IPamaxieDatabaseConfiguration connectionParams);

        /// <summary>
        /// Can be used for executing a certain command on a database level, if there is no predefined function for it. Please use sparingly since it basically negates the entire
        /// purpose of this api (mostly should be used for database testing).
        /// </summary>
        /// <param name="connectionParams">Connection Parameters for establishing a connection with the database</param>
        /// <param name="command">The command as a string that should be executed</param>
        /// <returns></returns>
        string ExecuteCommand(IPamaxieDatabaseConfiguration connectionParams, string command);
    }
}
