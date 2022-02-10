using Pamaxie.Data;
using Pamaxie.Database.Native.Sql;

namespace Pamaxie.Database.Native.DataInteraction.BusinessLogicExtensions;

public class PamUserInteraction : PamSqlInteractionBase<User>, Extensions.DataInteraction.IPamUserInteraction
{
    public IPamUser LoadProjects(IPamUser user)
    {
        throw new System.NotImplementedException();
    }

    public PamProjectPermissions GetProjectPermissions(string projectName)
    {
        throw new System.NotImplementedException();
    }

    public PamOrgPermissions GetOrgPermissions(string domainName)
    {
        throw new System.NotImplementedException();
    }

    public string GetUniqueKey(string username)
    {
        throw new System.NotImplementedException();
    }
}