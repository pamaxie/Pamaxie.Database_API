using Pamaxie.Database.Extensions.ServerSide;

namespace Pamaxie.Database.Redis.DataInteraction;

public class PamScanDataInteraction : PamDataInteractionBase, IPamScanInteraction
{
    private readonly PamaxieDatabaseService _owner;
    
    internal PamScanDataInteraction(PamaxieDatabaseService owner) : base(owner)
    {
        this._owner = owner;
    }
}