using System;
using System.Diagnostics;
using System.Linq;
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

    public virtual async Task<(bool, long)> CreateAsync(IPamSqlObject data)
    {
        await using var context = new PgSqlContext();

        long userId = 0;
        try
        {
            await context.AddAsync(data);
            await context.SaveChangesAsync();
            userId = data.Id;
        }
        catch (PostgresException)
        {
            Debug.WriteLine("Caught exception while trying to create object in the Postgres database. Please" +
                            "ensure the database is available and the objects reached in item contained a defined Id");
            return (false, 0);
        }

        return (true, userId);
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

    public async Task<bool> ExistsAsync(long uniqueKey)
    {
        var item = await GetAsync(uniqueKey);
        return item != null;
    }

    public virtual async Task<bool> DeleteAsync(IPamSqlObject data)
    {
        await using var context = new PgSqlContext();

        try
        {
            var projectUsers = context.ProjectUsers.Where(x => x.ProjectId == data.Id);
            context.RemoveRange(projectUsers);
            context.Remove(data);
            await context.SaveChangesAsync();
            return true;
        }
        catch (PostgresException)
        {
            Debug.WriteLine("Caught exception while trying to remove object from the Postgres database. Please" +
                            "ensure the database is available and the objects reached in item contained a defined Id");
            return false;
        }
    }

    public async Task<bool> DeleteAsync(long projectId)
    {
        await using var context = new PgSqlContext();
        
        try
        {
            var projectUsers = context.ProjectUsers.Where(x => x.ProjectId == projectId);
            context.RemoveRange(projectUsers);
            
            var project = await context.Projects.FirstOrDefaultAsync(x => x.Id == projectId);

            if (project == null)
            {
                return false;
            }
            
            context.Remove(project);
            await context.SaveChangesAsync();
            return true;
        }
        catch (PostgresException)
        {
            Debug.WriteLine("Caught exception while trying to remove object from the Postgres database. Please" +
                            "ensure the database is available and the objects reached in item contained a defined Id");
            return false;
        }
    }
}