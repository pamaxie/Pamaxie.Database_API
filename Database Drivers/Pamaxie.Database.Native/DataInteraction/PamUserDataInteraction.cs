using Pamaxie.Data;
using Pamaxie.Database.Design;

namespace Pamaxie.Database.Redis.DataInteraction;

public class PamUserDataInteraction : PamDataInteractionBase, IPamUserInteraction
{
    private readonly PamaxieDatabaseService _owner;
    
    internal PamUserDataInteraction(PamaxieDatabaseService owner) : base(owner)
    {
        this._owner = owner;
    }

    public IPamUser LoadProjects(IPamUser user)
    {
        throw new System.NotImplementedException();
    }

    public IPamUser LoadOrgs(IPamUser user)
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