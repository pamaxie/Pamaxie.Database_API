using System;
using System.Threading.Tasks;
using Pamaxie.Data;
using Pamaxie.Database.Native.Sql;

namespace Pamaxie.Database.Native.DataInteraction.BusinessLogicExtensions;

public static class ProjectExtensions
{
    public static IPamProject ToIPamProject(this Project user)
    {
        var pamUser = new PamProject()
        {
            Id = user.Id,
            Name = user.Name,
            CreationDate = user.CreationDate,
            LastModified = user.LastModified,
            LastModifiedUser = new LazyObject<(IPamUser User, long UserId)>(),
            Owner = new LazyObject<(IPamUser User, long UserId)>() {Data = (null, UserId: user.OwnerId), IsLoaded = false},
            ApiTokens = new LazyList<(string Token, DateTime LastUsed)>(){IsLoaded = false},
            Users = new LazyList<(long UserId, ProjectPermissions Permission)>(),
            Flags = user.Flags,
            TTL = user.TTL
        };

        return pamUser;
    }
    
    public static Project ToDbProject(this IPamProject project)
    {
        var pamUser = new Project
        {
            Name = project.Name,
            Flags = ProjectFlags.None,
            CreationDate = project.CreationDate,
            LastModified = project.LastModified,
            LastModifiedUserId = project.LastModifiedUser?.Data.UserId ?? 0,
            OwnerId = project.Owner?.Data.UserId ?? 0,
            TTL = project.TTL
        };
        
        return pamUser;
    }

    /// <summary>
    /// Loads the user from the database
    /// </summary>
    /// <param name="project"></param>
    /// <returns></returns>
    public static async Task<Project> LoadDbProjectAsync(this IPamProject project)
    {
        PamSqlInteractionBase<Project> sqlInteractionBase = new();
        var userObj = await sqlInteractionBase.GetAsync(project.Id);

        if (userObj is not Project dbUser)
        {
            return null;
        }

        return dbUser;
    }
}