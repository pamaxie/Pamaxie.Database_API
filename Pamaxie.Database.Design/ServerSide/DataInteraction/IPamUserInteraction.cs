using System.Collections.Generic;
using Pamaxie.Data;

namespace Pamaxie.Database.Extensions.DataInteraction
{
    /// <summary>
    /// Interface that defines User interactions
    /// </summary>
    public interface IPamUserInteraction : IPamInteractionBase<IPamSqlObject, long>
    {
        /// <summary>
        /// Gets a user via their username
        /// </summary>
        /// <param name="username">username of the user</param>
        /// <returns></returns>
        public IPamUser Get(string username);

        /// <summary>
        /// Does the username exist in our database
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public bool ExistsUsername(string username);
        
        /// <summary>
        /// Does the email exist in our database
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public bool ExistsEmail(string email);
        
        /// <summary>
        /// Loads the user completely
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public IPamUser LoadFully(IPamUser user);
        
        /// <summary>
        /// Loads the <see cref="IPamUser.Projects"/> property
        /// </summary>
        /// <returns><see cref="IPamUser"/> with their <see cref="IPamUser.Projects"/> property loaded</returns>
        public IPamUser LoadProjects(IPamUser user);

        /// <summary>
        /// Gets the <see cref="user"/> <see>
        ///     <cref>PamProjectPermissions</cref>
        /// </see>
        /// for the project with the reached in <see cref="projectName"/>
        /// </summary>
        /// <param name="projectName">Name of the project</param>
        /// <param name="user">User to get the permissions of</param>
        /// <returns><see>
        ///         <cref>PamProjectPermissions</cref>
        ///     </see>
        ///     of the user</returns>
        public ProjectPermissions GetProjectPermissions(string projectName, IPamUser user);

        /// <summary>
        /// Gets the <see cref="user"/> <see>
        ///     <cref>PamProjectPermissions</cref>
        /// </see>
        /// for the project with the reached in <see cref="projectId"/>
        /// </summary>
        /// <param name="projectId">Id of the Project</param>
        /// <param name="user">User to get the permissions of</param>
        /// <returns><see>
        ///         <cref>PamProjectPermissions</cref>
        ///     </see>
        ///     of the user</returns>
        public ProjectPermissions GetProjectPermissions(long projectId, IPamUser user);

        /// <summary>
        /// Loads the <see>
        ///     <cref>users</cref>
        /// </see>
        /// Two Factor options
        /// </summary>
        /// <param name="user">User to get the Two factor options from</param>
        public IPamUser LoadTwoFactorOptions(IPamUser user);

        /// <summary>
        /// Loads the <see>
        ///     <cref>users</cref>
        /// </see>
        /// User to load the known IPs of
        /// </summary>
        /// <param name="user">User to get the Two factor options from</param>
        public IPamUser LoadKnownIps(IPamUser user);

        /// <summary>
        /// Checks if the Ip Address for the user is known
        /// </summary>
        /// <param name="user">User to check the IP address for</param>
        /// <param name="ipAddress">Ip address to check for</param>
        public bool IsIpKnown(IPamUser user, string ipAddress);
        

        /// <summary>
        /// Gets the actual unique key of the <see cref="IPamUser"/> via the users username
        /// </summary>
        /// <param name="username"><see cref="IPamUser"/>'s username to get the keys from</param>
        /// <returns>unique key</returns>
        public long GetUniqueKey(string username);
    }
}