using Pamaxie.Data;
using Pamaxie.Database.Extensions.DataInteraction;

namespace Pamaxie.Database.Native.DataInteraction;

public class PamNoSqlInteractionBase : IPamInteractionBase<IPamNoSqlObject, string>
{
    public IPamNoSqlObject Get(string uniqueKey)
    {
        throw new System.NotImplementedException();
    }

    public bool Create(IPamNoSqlObject data)
    {
        throw new System.NotImplementedException();
    }

    public bool TryCreate(IPamNoSqlObject data, out IPamNoSqlObject createdItem)
    {
        throw new System.NotImplementedException();
    }

    public bool Update(IPamNoSqlObject data)
    {
        throw new System.NotImplementedException();
    }

    public bool UpdateOrCreate(IPamNoSqlObject data)
    {
        throw new System.NotImplementedException();
    }

    public bool Exists(string uniqueKey)
    {
        throw new System.NotImplementedException();
    }

    public bool Delete(IPamNoSqlObject data)
    {
        throw new System.NotImplementedException();
    }
}