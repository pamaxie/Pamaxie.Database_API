using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Pamaxie.Data;

namespace Pamaxie.Database.Extensions.DataInteraction
{
    /// <summary>
    /// Interface that defines Application interactions
    /// </summary>
    public interface IPamProjectInteraction : IPamInteractionBase<IPamSqlObject, long>
    {
        /// <summary>
        /// Loads the <see cref="IPamProject.Owner"/> Property of the reached in <see cref="item"/>
        /// </summary>
        /// <param name="item">The <see cref="IPamProject"/> to load the owner from</param>
        /// <returns><see cref="item"/> with it's <see cref="IPamProject.Owner"/> Loaded</returns>
        public Task<IPamProject> LoadOwnerAsync(IPamProject item);
            
        /// <summary>
        /// Loads the user who last modified the project
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public Task<IPamProject> LoadLastModifiedUserAsync(IPamProject item);
        
        /// <summary>
        /// Loads the Api Tokens for the Project
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public Task<IPamProject> LoadApiTokensAsync(IPamProject item);
        
        /// <summary>
        /// Loads the Users for the project
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public Task<IPamProject> LoadUsersAsync(IPamProject item);

        /// <summary>
        /// Checks if a login can be authenticated via their <see cref="token"/>
        /// </summary>
        /// <param name="projectId">Id of the project that should be verified</param>
        /// <param name="token">The auth token fot the project</param>
        /// <returns><see cref="IPamProject"/> that could be authenticated with the token</returns>
        public Task<bool> ValidateTokenAsync(string token);

        /// <summary>
        /// Checks if a user has certain project permissions
        /// </summary>
        /// <param name="permissions"><see>
        ///         <cref>PamProjectPermissions</cref>
        ///     </see>
        ///     flags enum which permissions should be validated</param>
        /// <param name="username">username of the user which permissions should be validated</param>
        /// <param name="project">project that the permissions should be validated against</param>
        /// <returns></returns>
        public Task<bool> HasPermissionAsync(IPamProject project, ProjectPermissions permissions, string username);

        /// <summary>
        /// Checks if a user has certain project permissions
        /// </summary>
        /// <param name="permissions"><see>
        ///         <cref>PamProjectPermissions</cref>
        ///     </see>
        ///     flags enum which permissions should be validated</param>
        /// <param name="userId">Id of the user which permissions should be validated</param>
        /// <param name="projectId">project id that the permissions should be validated against</param>
        /// <returns></returns>
        public Task<bool> HasPermissionAsync(long projectId, ProjectPermissions permissions, long userId);

        /// <summary>
        /// Checks if a user has certain project permissions
        /// </summary>
        /// <param name="permissions"><see>
        ///         <cref>PamProjectPermissions</cref>
        ///     </see>
        ///     flags enum which permissions should be validated</param>
        /// <param name="userId">Id of the user which permissions should be validated</param>
        /// <param name="project">project that the permissions should be validated against</param>
        /// <returns></returns>
        public Task<bool> HasPermissionAsync(IPamProject project, ProjectPermissions permissions, long userId);

        /// <summary>
        /// Checks if a user has certain project permissions
        /// </summary>
        /// <param name="permissions"><see>
        ///         <cref>PamProjectPermissions</cref>
        ///     </see>
        ///     flags enum which permissions should be validated</param>
        /// <param name="username">username of the user which permissions should be validated</param>
        /// <param name="projectId">project id that the permissions should be validated against</param>
        /// <returns></returns>
        public Task<bool> HasPermissionAsync(long projectId, ProjectPermissions permissions, string username);

        /// <summary>
        /// Removes a Api Token to a project
        /// </summary>
        /// <param name="projectId">Id of the project to create a token for</param>
        /// <returns>Api token that was auto generated for the user (login credential)</returns>
        public Task<string> CreateTokenAsync(long projectId, DateTime expiryTime);

        /// <summary>
        /// Adds a Api Token to a project
        /// </summary>
        ///<param name="projectId">Id of the project to remove the token from</param>
        /// <param name="tokenId">Id of the token to remove</param>
        /// <returns></returns>
        public Task<bool> RemoveTokenAsync(long projectId, long tokenId);

        /// <summary>
        /// Sets a users permission for a project
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="userId"></param>
        /// <param name="newPermissions"></param>
        /// <returns></returns>
        public Task<bool> SetPermissionsAsync(long projectId, long userId, ProjectPermissions newPermissions);

        /// <summary>
        /// Adds a user to a project
        /// </summary>
        /// <param name="projectId">Id of the project to add the user to</param>
        /// <param name="userId">User to add</param>
        /// <param name="permissions">permissions the user has in the project</param>
        /// <returns></returns>
        public Task<bool> AddUserAsync(long projectId, long userId, ProjectPermissions permissions);

        ///  <summary>
        ///  Removes a user to a project
        ///  </summary>
        /// <param name="projectId">Id of the project the user is removed from</param>
        ///  <param name="userId">Id of the user that should be removed from the project</param>
        ///  <returns></returns>
        public Task<bool> RemoveUserAsync(long projectId, long userId);
    }
}