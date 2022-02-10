using System;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Pamaxie.Data;
using Pamaxie.Database.Extensions.DataInteraction;

namespace Pamaxie.Database.Native.DataInteraction;

public class PamSqlInteractionBase<T> : IPamInteractionBase<IPamSqlObject, ulong>
{
    public IPamSqlObject Get(ulong uniqueKey)
    {
        using var context = new PgSqlContext();
        var obj = context.Find(typeof(T), uniqueKey as object);
        return obj is not T ? null : (IPamSqlObject) obj;
    }

    public virtual bool Create(IPamSqlObject data)
    {
        using var context = new PgSqlContext();

        try
        {
            var obj = context.Add(data);
            context.SaveChanges();
        }
        catch (PostgresException)
        {
            Debug.WriteLine("Caught exception while trying to create object in Postgres database. Please" +
                            "ensure the database is available and the objects reached in item contained a defined Id");
            return false;
        }

        return true;
    }

    public bool TryCreate(IPamSqlObject data, out IPamSqlObject createdItem)
    {
        throw new NotImplementedException();
    }

    public bool Update(IPamSqlObject data)
    {
        throw new NotImplementedException();
    }

    public bool TryUpdate(IPamSqlObject data, out IPamSqlObject updatedItem)
    {
        throw new NotImplementedException();
    }

    public bool UpdateOrCreate(IPamSqlObject data, out IPamSqlObject updatedOrCreatedItem)
    {
        throw new NotImplementedException();
    }

    public bool Exists(ulong uniqueKey)
    {
        throw new NotImplementedException();
    }

    public bool Delete(IPamSqlObject data)
    {
        throw new NotImplementedException();
    }
}