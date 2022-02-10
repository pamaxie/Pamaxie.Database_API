using System.Threading.Tasks;
using Pamaxie.Database.Extensions.DataInteraction;

namespace Pamaxie.Database.Extensions
{
    public interface IPamaxieDatabaseService
    {
        public bool IsDbConnected { get; }
        
        /// <summary>
        /// First db connection host (for connecting with the first db type)
        /// </summary>
        public dynamic DbConnectionHost1 { get; }
        
        /// <summary>
        /// Second db connection host (for connecting a second db type)
        /// </summary>
        public dynamic DbConnectionHost2 { get; }
        
        /// <summary>
        /// Service for accessing Pamaxie's application data
        /// </summary>
        IPamProjectInteraction Projects { get; }

        /// <summary>
        /// Service for accessing Pamaxie's user data
        /// </summary>
        IPamUserInteraction Users { get; }

        /// <summary>
        /// Service for accessing scan results from Pamaxie's api
        /// </summary>
        IPamScanInteraction Scans { get; }
        
        /// <summary>
        /// Validates the connection to the database with the reached in database context
        /// </summary>
        /// <param name="connectionParams">The connection parameters for establishing a connection with the database</param>
        /// <returns><see cref="bool"/> if the connection with the database was successful</returns>
        bool ValidateConfiguration(IPamaxieDatabaseConfiguration connectionParams);

        /// <summary>
        /// Connects to the database to create a long lasting / living connection
        /// </summary>
        /// <param name="connectionParams">Connection parameters to the database</param>
        /// <returns></returns>
        void ConnectToDatabase(IPamaxieDatabaseConfiguration connectionParams = null);

        /// <summary>
        /// Async implementation of <see cref="ConnectToDatabase"/>
        /// </summary>
        /// <param name="connectionParams"></param>
        /// <returns></returns>
        Task ConnectToDatabaseAsync(IPamaxieDatabaseConfiguration connectionParams = null);

        /// <summary>
        /// Validates the Database Integrity
        /// </summary>
        void ValidateDatabase();
        
        /// <summary>
        /// Validates the Database Integrity Async
        /// </summary>
        /// <returns></returns>
        Task ValidateDatabaseAsync();
    }
}
