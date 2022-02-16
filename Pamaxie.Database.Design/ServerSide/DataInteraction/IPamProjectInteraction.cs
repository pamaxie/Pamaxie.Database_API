using System.Collections.Generic;
using Pamaxie.Data;

namespace Pamaxie.Database.Extensions.DataInteraction
{
    /// <summary>
    /// Interface that defines Application interactions
    /// </summary>
    public interface IPamProjectInteraction : IPamInteractionBase<IPamSqlObject, ulong>
    {
        /// <summary>
        /// Loads the <see cref="IPamProject.Owner"/> Property of the reached in <see cref="item"/>
        /// </summary>
        /// <param name="item">The <see cref="IPamProject"/> to load the owner from</param>
        /// <returns><see cref="item"/> with it's <see cref="IPamProject.Owner"/> Loaded</returns>
        public IPamProject LoadOwner(IPamProject item);

        /// <summary>
        /// Gets the projects for the specific user
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public List<IPamProject> GetProjects(IPamUser user);

        /// <summary>
        /// Checks if a login can be authenticated via their <see cref="token"/>
        /// </summary>
        /// <param name="token">The auth token fot the project</param>
        /// <returns><see cref="IPamProject"/> that could be authenticated with the token</returns>
        public IPamProject VerifyAuthentication(string token);

        /// <summary>
        /// Checks if a user has certain project permissions
        /// </summary>
        /// <param name="permissions"><see cref="PamProjectPermissions"/> flags enum which permissions should be validated</param>
        /// <param name="username">username of the user which permissions should be validated</param>
        /// <returns></returns>
        public bool HasPermission(ProjectPermissions permissions, string username);
    }
}