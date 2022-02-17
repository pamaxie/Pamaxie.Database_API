using System;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Pamaxie.Data;
using Pamaxie.Database.Extensions.DataInteraction;

namespace Pamaxie.Database.Native.DataInteraction;

public class PamSqlInteractionBase<T> : IPamInteractionBase<IPamSqlObject, long>
{
    public virtual IPamSqlObject Get(long uniqueKey)
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
            context.Add(data);
            context.SaveChanges();
        }
        catch (PostgresException)
        {
            Debug.WriteLine("Caught exception while trying to create object in the Postgres database. Please" +
                            "ensure the database is available and the objects reached in item contained a defined Id");
            return false;
        }

        return true;
    }

    public virtual bool Update(IPamSqlObject data)
    {
        using var context = new PgSqlContext();

        try
        {
            context.Update(data);
            context.SaveChanges();
        }
        catch (PostgresException)
        {
            Debug.WriteLine("Caught exception while trying to update object in the Postgres database. Please" +
                            "ensure the database is available and the objects reached in item contained a defined Id");
            return false;
        }

        return true;
    }

    public virtual bool UpdateOrCreate(IPamSqlObject data)
    {
        return Create(data) || Update(data);
    }

    public bool Exists(long uniqueKey)
    {
        var item = Get(uniqueKey);
        return item == null;
    }

    public bool Delete(IPamSqlObject data)
    {
        using var context = new PgSqlContext();

        try
        {
            context.Remove(data);
            context.SaveChanges();
        }
        catch (PostgresException)
        {
            Debug.WriteLine("Caught exception while trying to remove object from the Postgres database. Please" +
                            "ensure the database is available and the objects reached in item contained a defined Id");
            return false;
        }

        return true;
    }
}