using Pamaxie.Data;
using Pamaxie.Database.Design;

namespace Pamaxie.Database.Redis.DataInteraction;

/// <summary>
/// <inheritdoc cref="IPamInteractionBase{T}"/>
/// </summary>
public class PamDataInteractionBase : IPamInteractionBase<IPamDbObject>
{
    private readonly PamaxieDatabaseService _owner;
    
    internal PamDataInteractionBase(PamaxieDatabaseService owner)
    {
        _owner = owner;
    }
    
    /// <summary>
    /// <inheritdoc cref="IPamInteractionBase{T}.Get"/>
    /// </summary>
    public IPamDbObject Get(string uniqueKey)
    {
        throw new System.NotImplementedException();
    }

    /// <summary>
    /// <inheritdoc cref="IPamInteractionBase{T}.Create"/>
    /// </summary>
    public IPamDbObject Create(IPamDbObject data)
    {
        throw new System.NotImplementedException();
    }

    /// <summary>
    /// <inheritdoc cref="IPamInteractionBase{T}.TryCreate"/>
    /// </summary>
    public bool TryCreate(IPamDbObject data, out IPamDbObject createdItem)
    {
        throw new System.NotImplementedException();
    }
    
    /// <summary>
    /// <inheritdoc cref="IPamInteractionBase{T}.Update"/>
    /// </summary>
    public IPamDbObject Update(IPamDbObject data)
    {
        throw new System.NotImplementedException();
    }

    /// <summary>
    /// <inheritdoc cref="IPamInteractionBase{T}.TryUpdate"/>
    /// </summary>
    public bool TryUpdate(IPamDbObject data, out IPamDbObject updatedItem)
    {
        throw new System.NotImplementedException();
    }

    /// <summary>
    /// <inheritdoc cref="IPamInteractionBase{T}.UpdateOrCreate"/>
    /// </summary>
    public bool UpdateOrCreate(IPamDbObject data, out IPamDbObject updatedOrCreatedItem)
    {
        throw new System.NotImplementedException();
    }

    /// <summary>
    /// <inheritdoc cref="IPamInteractionBase{T}.Exists"/>
    /// </summary>
    public bool Exists(string uniqueKey)
    {
        throw new System.NotImplementedException();
    }

    /// <summary>
    /// <inheritdoc cref="IPamInteractionBase{T}.Delete"/>
    /// </summary>
    public bool Delete(IPamDbObject data)
    {
        throw new System.NotImplementedException();
    }
}