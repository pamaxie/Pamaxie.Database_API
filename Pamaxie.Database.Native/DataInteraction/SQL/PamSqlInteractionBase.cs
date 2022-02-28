using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Pamaxie.Data;
using Pamaxie.Database.Extensions.DataInteraction;

namespace Pamaxie.Database.Native.DataInteraction;

public class PamSqlInteractionBase<T> : IPamInteractionBase<IPamSqlObject, long>
{
    public virtual async Task<IPamSqlObject> GetAsync(long uniqueKey)
    {
        await using var context = new PgSqlContext();
        var obj = await context.FindAsync(typeof(T), uniqueKey as object);
        return obj is not T ? null : (IPamSqlObject) obj;
    }

    public virtual async Task<bool> CreateAsync(IPamSqlObject data)
    {
        await using var context = new PgSqlContext();

        try
        {
            await context.AddAsync(data);
            await context.SaveChangesAsync();
        }
        catch (PostgresException)
        {
            Debug.WriteLine("Caught exception while trying to create object in the Postgres database. Please" +
                            "ensure the database is available and the objects reached in item contained a defined Id");
            return false;
        }

        return true;
    }

    public virtual async Task<bool> UpdateAsync(IPamSqlObject data)
    {
        await using var context = new PgSqlContext();

        try
        {
            context.Update(data);
            await context.SaveChangesAsync();
        }
        catch (PostgresException)
        {
            Debug.WriteLine("Caught exception while trying to update object in the Postgres database. Please" +
                            "ensure the database is available and the objects reached in item contained a defined Id");
            return false;
        }

        return true;
    }

    public virtual async Task<bool> UpdateOrCreateAsync(IPamSqlObject data)
    {
        return await CreateAsync(data) || await UpdateAsync(data);
    }

    public async Task<bool> ExistsAsync(long uniqueKey)
    {
        var item = await GetAsync(uniqueKey);
        return item == null;
    }

    public async Task<bool> DeleteAsync(IPamSqlObject data)
    {
        await using var context = new PgSqlContext();

        try
        {
            context.Remove(data);
            await context.SaveChangesAsync();
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