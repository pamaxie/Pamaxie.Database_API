using Pamaxie.Data;
using Pamaxie.Database.Extensions.ServerSide;

namespace Pamaxie.Database.Redis.DataInteraction;

public class PamOrgDataInteraction : PamDataInteractionBase, IPamOrgInteraction
{
    private PamaxieDatabaseService _owner;
    
    internal PamOrgDataInteraction(PamaxieDatabaseService owner) : base(owner)
    {
        this._owner = owner;
    }
    
    public IPamOrg LoadOwner(IPamOrg org)
    {
        throw new System.NotImplementedException();
    }

    public IPamOrg LoadProjects(IPamOrg org)
    {
        throw new System.NotImplementedException();
    }

    public IPamOrg LoadUsers(IPamOrg org)
    {
        throw new System.NotImplementedException();
    }

    public bool CheckUserPermissions(PamOrgPermissions permissions, string username)
    {
        throw new System.NotImplementedException();
    }
}