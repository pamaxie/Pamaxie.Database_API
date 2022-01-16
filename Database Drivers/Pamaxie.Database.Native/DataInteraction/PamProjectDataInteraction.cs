using Pamaxie.Data;
using Pamaxie.Database.Design;

namespace Pamaxie.Database.Redis.DataInteraction;

public class PamProjectDataInteraction : PamDataInteractionBase, IPamProjectInteraction
{
    private readonly PamaxieDatabaseService _owner;
    
    internal PamProjectDataInteraction(PamaxieDatabaseService owner) : base(owner)
    {
        this._owner = owner;
    }

    public IPamProject LoadOwner(IPamProject item)
    {
        throw new System.NotImplementedException();
    }

    public IPamProject VerifyAuthentication(string token)
    {
        throw new System.NotImplementedException();
    }

    public bool HasPermission(PamProjectPermissions permissions, string username)
    {
        throw new System.NotImplementedException();
    }
}