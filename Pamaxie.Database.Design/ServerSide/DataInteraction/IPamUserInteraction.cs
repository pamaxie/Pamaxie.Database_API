using System.Collections.Generic;
using System.Threading.Tasks;
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
        public Task<IPamUser> GetAsync(string username);
        
        /// <summary>
        /// Gets a user via their username
        /// </summary>
        /// <param name="email">email of the user</param>
        /// <returns></returns>
        public Task<IPamUser> GetViaMailAsync(string email);

        /// <summary>
        /// Does the username exist in our database
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public Task<bool> ExistsUsernameAsync(string username);
        
        /// <summary>
        /// Does the email exist in our database
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public Task<bool> ExistsEmailAsync(string email);
        
        /// <summary>
        /// Loads the user completely
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public Task<IPamUser> LoadFullyAsync(IPamUser user);
        
        /// <summary>
        /// Loads the <see cref="IPamUser.Projects"/> property
        /// </summary>
        /// <returns><see cref="IPamUser"/> with their <see cref="IPamUser.Projects"/> property loaded</returns>
        public Task<IPamUser> LoadProjectsAsync(IPamUser user);

        /// <summary>
        /// Gets the <see cref="user"/> <see>
        ///     <cref>PamProjectPermissions</cref>
        /// </see>
        /// for the project with the reached in <see cref="projectName"/>
        /// </summary>
        /// <param name="projectName">Name of the project</param>
        /// <param name="userIdUser to get the permissions of</param>
        /// <returns><see>
        ///         <cref>PamProjectPermissions</cref>
        ///     </see>
        ///     of the user</returns>
        public Task<ProjectPermissions> GetProjectPermissionsAsync(string projectName, long userId);

        /// <summary>
        /// Gets the <see cref="user"/> <see>
        ///     <cref>PamProjectPermissions</cref>
        /// </see>
        /// for the project with the reached in <see cref="projectId"/>
        /// </summary>
        /// <param name="projectId">Id of the Project</param>
        /// <param name="userIdUser to get the permissions of</param>
        /// <returns><see>
        ///         <cref>PamProjectPermissions</cref>
        ///     </see>
        ///     of the user</returns>
        public Task<ProjectPermissions> GetProjectPermissionsAsync(long projectId, long userId);

        // /// <summary>
        // /// Loads the <see>
        // ///     <cref>users</cref>
        // /// </see>
        // /// Two Factor options
        // /// </summary>
        // /// <param name="user">User to get the Two factor options from</param>
        // public Task<IPamUser> LoadTwoFactorOptionsAsync(IPamUser user);

        /// <summary>
        /// Loads the <see>
        ///     <cref>users</cref>
        /// </see>
        /// User to load the known IPs of
        /// </summary>
        /// <param name="user">User to get the Two factor options from</param>
        public Task<IPamUser> LoadKnownIpsAsync(IPamUser user);

        /// <summary>
        /// Checks if the Ip Address for the user is known
        /// </summary>
        /// <param name="userId">UserId to check the IP address for</param>
        /// <param name="ipAddress">Ip address to check for</param>
        public Task<bool> IsIpKnownAsync(long userId, string ipAddress);
        

        /// <summary>
        /// Gets the actual unique key of the <see cref="IPamUser"/> via the users username
        /// </summary>
        /// <param name="username"><see cref="IPamUser"/>'s username to get the keys from</param>
        /// <returns>unique key</returns>
        public Task<long> GetUniqueKeyAsync(string username);

        /// <summary>
        /// Sets a confirmation code of a user for validating their email
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="confirmationCode"></param>
        /// <returns></returns>
        public Task<bool> SetConfirmationCodeAsync(long userId, string confirmationCode);

        /// <summary>
        /// Validates if a confirmation code is correct.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="confirmationCode"></param>
        /// <returns></returns>
        public Task<(bool ConfirmationSuccessful, long UserId)> ValidateConfirmationCodeAsync(string confirmationCode);
    }
}