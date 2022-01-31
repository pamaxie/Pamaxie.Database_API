using Pamaxie.Data;

namespace Pamaxie.Database.Extensions.DataInteraction;

public interface IPamOrgInteraction : IPamInteractionBase<IPamSqlObject>
{
    /// <summary>
    /// Loads see <see cref="IPamOrg.Owner"/> property completely
    /// </summary>
    /// <param name="org"><see cref="IPamOrg"/> of which the owner should be loaded</param>
    /// <returns><see cref="IPamOrg"/> with the <see cref="IPamOrg.Owner"/> property loaded</returns>
    public IPamOrg LoadOwner(IPamOrg org);

    /// <summary>
    /// Loads the <see cref="IPamOrg.Projects"/>
    /// </summary>
    /// <param name="org"><see cref="IPamOrg"/> of which <see cref="IPamProject"/>s should be loaded</param>
    /// <returns><see cref="IPamOrg"/> with the <see cref="IPamOrg.Projects"/> property loaded</returns>
    public IPamOrg LoadProjects(IPamOrg org);

    /// <summary>
    /// Loads the <see cref="IPamOrg.Users"/>
    /// </summary>
    /// <param name="org"><see cref="IPamOrg"/> of which <see cref="IPamUser"/>s should be loaded</param>
    /// <returns><see cref="IPamOrg"/> with the <see cref="IPamOrg.Users"/> property loaded</returns>
    public IPamOrg LoadUsers(IPamOrg org);

    /// <summary>
    /// Validates if a <see cref="IPamUser"/> has certain <see cref="PamOrgPermissions"/>
    /// </summary>
    /// <param name="permissions"><see cref="PamOrgPermissions"/> that should be validated</param>
    /// <param name="username"><see cref="username"/> of the user</param>
    /// <returns><see cref="bool"/> determining if a user has the permissions</returns>
    public bool CheckUserPermissions(PamOrgPermissions permissions, string username);
}