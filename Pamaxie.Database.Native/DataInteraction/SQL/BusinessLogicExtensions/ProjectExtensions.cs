using System;
using System.Threading.Tasks;
using Pamaxie.Data;
using Pamaxie.Database.Native.Sql;

namespace Pamaxie.Database.Native.DataInteraction.BusinessLogicExtensions;

public static class ProjectExtensions
{
    public static IPamProject ToUserLogic(this Project project)
    {
        var pamProject = new PamProject()
        {
            Id = project.Id,
            Name = project.Name,
            CreationDate = project.CreationDate,
            LastModifiedAt = project.LastModified,
            LastModifiedUser = new LazyObject<(IPamUser User, long UserId)>(),
            Owner = new LazyObject<(IPamUser User, long UserId)>() {Data = (null, UserId: project.OwnerId), IsLoaded = false},
            ApiTokens = new LazyList<(string Token, DateTime LastUsed)>(){IsLoaded = false},
            Users = new LazyList<(long UserId, ProjectPermissions Permission)>(),
            Flags = project.Flags,
            TTL = project.TTL
        };

        return pamProject;
    }
    
    public static Project ToBusinessLogic(this IPamProject pamProject)
    {
        var project = new Project
        {
            Name = pamProject.Name,
            Flags = ProjectFlags.None,
            //Forbid from changing creation date
            //CreationDate = pamProject.CreationDate,
            LastModified = pamProject.LastModifiedAt,
            LastModifiedUserId = pamProject.LastModifiedUser?.Data.UserId ?? 0,
            OwnerId = pamProject.Owner?.Data.UserId ?? 0,
            TTL = pamProject.TTL
        };

        project.Id = pamProject.Id;
        
        return project;
    }

    /// <summary>
    /// Loads the user from the database
    /// </summary>
    /// <param name="project"></param>
    /// <returns></returns>
    public static async Task<Project> LoadDbProjectAsync(this IPamProject project)
    {
        PamSqlInteractionBase<Project> sqlInteractionBase = new();
        var pamObj = await sqlInteractionBase.GetAsync(project.Id);

        if (pamObj is not Project dbProject)
        {
            return null;
        }

        return dbProject;
    }
}