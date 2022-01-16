using System.Collections.Generic;
using Pamaxie.Data;

namespace Pamaxie.Database.Design
{
    /// <summary>
    /// Interface that defines User interactions
    /// </summary>
    public interface IPamUserInteraction : IPamInteractionBase<IPamDbObject>
    {
        /// <summary>
        /// Loads the <see cref="IPamUser.Projects"/> property
        /// </summary>
        /// <returns><see cref="IPamUser"/> with their <see cref="IPamUser.Projects"/> property loaded</returns>
        public IPamUser LoadProjects(IPamUser user);

        /// <summary>
        /// Loads the <see cref="IPamUser.Orgs"/> property
        /// </summary>
        /// <param name="user">User to load the property from</param>
        /// <returns><see cref="IPamProject"/> with their <see cref="IPamUser.Orgs"/> property loaded</returns>
        public IPamUser LoadOrgs(IPamUser user);

        /// <summary>
        /// Gets the users <see cref="PamProjectPermissions"/> for the project with the reached in <see cref="projectName"/>
        /// </summary>
        /// <param name="projectName">Name of the project</param>
        /// <returns><see cref="PamProjectPermissions"/> of the user</returns>
        public PamProjectPermissions GetProjectPermissions(string projectName);

        /// <summary>
        /// Gets the users <see cref="PamOrgPermissions"/> for the <see cref="IPamOrg"/> with the reached in <see cref="domainName"/>
        /// </summary>
        /// <param name="domainName"><see cref="IPamOrg.DomainName"/> for the org</param>
        /// <returns><see cref="PamOrgPermissions"/> of the user</returns>
        public PamOrgPermissions GetOrgPermissions(string domainName);

        /// <summary>
        /// Gets the actual unique key of the <see cref="IPamUser"/> via the users username
        /// </summary>
        /// <param name="username"><see cref="IPamUser"/>'s username to get the keys from</param>
        /// <returns>unique key</returns>
        public string GetUniqueKey(string username);
    }
}