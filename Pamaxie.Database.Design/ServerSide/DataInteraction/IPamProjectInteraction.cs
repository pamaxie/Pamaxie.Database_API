using System.Collections.Generic;
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
        public IPamProject LoadOwner(IPamProject item);
            
        /// <summary>
        /// Loads the user who last modified the project
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public IPamProject LoadLastModifiedUser(IPamProject item);
        
        /// <summary>
        /// Loads the Api Tokens for the Project
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public IPamProject LoadApiTokens(IPamProject item);
        
        /// <summary>
        /// Loads the Users for the project
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public IPamProject LoadUsers(IPamProject item);

        /// <summary>
        /// Checks if a login can be authenticated via their <see cref="token"/>
        /// </summary>
        /// <param name="projectId">Id of the project that should be verified</param>
        /// <param name="token">The auth token fot the project</param>
        /// <returns><see cref="IPamProject"/> that could be authenticated with the token</returns>
        public bool ValidateToken(long projectId, string token);

        /// <summary>
        /// Checks if a user has certain project permissions
        /// </summary>
        /// <param name="permissions"><see cref="PamProjectPermissions"/> flags enum which permissions should be validated</param>
        /// <param name="username">username of the user which permissions should be validated</param>
        /// <param name="project">project that the permissions should be validated against</param>
        /// <returns></returns>
        public bool HasPermission(IPamProject project, ProjectPermissions permissions, string username);
        
        /// <summary>
        /// Checks if a user has certain project permissions
        /// </summary>
        /// <param name="permissions"><see cref="PamProjectPermissions"/> flags enum which permissions should be validated</param>
        /// <param name="userId">Id of the user which permissions should be validated</param>
        /// <param name="projectId">project id that the permissions should be validated against</param>
        /// <returns></returns>
        public bool HasPermission(long projectId, ProjectPermissions permissions, long userId);
        
        /// <summary>
        /// Checks if a user has certain project permissions
        /// </summary>
        /// <param name="permissions"><see cref="PamProjectPermissions"/> flags enum which permissions should be validated</param>
        /// <param name="userId">Id of the user which permissions should be validated</param>
        /// <param name="project">project that the permissions should be validated against</param>
        /// <returns></returns>
        public bool HasPermission(IPamProject project, ProjectPermissions permissions, long userId);
        
        /// <summary>
        /// Checks if a user has certain project permissions
        /// </summary>
        /// <param name="permissions"><see cref="PamProjectPermissions"/> flags enum which permissions should be validated</param>
        /// <param name="username">username of the user which permissions should be validated</param>
        /// <param name="projectId">project id that the permissions should be validated against</param>
        /// <returns></returns>
        public bool HasPermission(long projectId, ProjectPermissions permissions, string username);

        /// <summary>
        /// Removes a Api Token to a project
        /// </summary>
        /// <param name="userId">User to add</param>
        /// <returns>Api token that was auto generated for the user (login credential)</returns>
        public string CreateToken(long projectId);
        
        /// <summary>
        /// Adds a Api Token to a project
        /// </summary>
        /// <param name="userId">User to add</param>
        /// <returns></returns>
        public bool RemoveToken(long projectId, long tokenId);

        /// <summary>
        /// Sets a users permission for a project
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="userId"></param>
        /// <param name="newPermissions"></param>
        /// <returns></returns>
        public bool SetPermissions(long projectId, long userId, ProjectPermissions newPermissions);
        
        /// <summary>
        /// Adds a user to a project
        /// </summary>
        /// <param name="userId">User to add</param>
        /// <returns></returns>
        public bool AddUser(long projectId, long userId, ProjectPermissions permissions);
        
        /// <summary>
        /// Removes a user to a project
        /// </summary>
        /// <param name="user">User to remove</param>
        /// <returns></returns>
        public bool RemoveUser(long projectId, long userId);
    }
}