using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Isopoh.Cryptography.Argon2;
using Pamaxie.Data;
using Pamaxie.Database.Extensions.DataInteraction;
using Pamaxie.Database.Native.DataInteraction.BusinessLogicExtensions;
using Pamaxie.Database.Native.Sql;

namespace Pamaxie.Database.Native.DataInteraction;

public class PamProjectInteraction : PamSqlInteractionBase<Project>, IPamProjectInteraction
{
    public override IPamSqlObject Get(long uniqueKey)
    {
        if (uniqueKey <= 0)
        {
            throw new ArgumentException("Invalid unique Key for the Project", nameof(uniqueKey));
        }
        
        var item = base.Get(uniqueKey);
        
        if (item is not Project project)
        {
            return null;
        }

        return project.ToIPamProject();
    }

    public override bool Create(IPamSqlObject data)
    {
        using var context = new PgSqlContext();
        
        if (data is Project project)
        {
            context.ProjectUsers.Add(new ProjectUser(project.OwnerId, project.Id));
            context.SaveChangesAsync();
        }

        if (data is IPamProject pamProject)
        {
            pamProject.Id = Project.ProjectIdGenerator.CreateId();
            return base.Create(pamProject.ToDbProject());
        }

        return base.Create(data);
    }

    public override bool Update(IPamSqlObject data)
    {
        return base.Update(data);
    }

    public override bool UpdateOrCreate(IPamSqlObject data)
    {
        if (!base.Exists(data.Id))
        {
            return Create(data);
        }
        
        return base.UpdateOrCreate(data);
    }

    public IPamProject LoadOwner(IPamProject item)
    {
        if (item.Owner.Data.UserId == null && item.Owner.Data.UserId <= 0)
        {
            throw new ArgumentException(
                "The reached in object does not have the owner data included to load it properly. " +
                "Please reload the object from the database. Otherwise we cannot load the users Owner information.", nameof(item));
        }
        
        using var context = new PgSqlContext();

        var owner = PamaxieDatabaseService.UserSingleton.Get(item.Owner.Data.UserId);

        if (owner is IPamUser user)
        {
            item.Owner = new LazyObject<(IPamUser User, long UserId)>() {IsLoaded = true, Data = (user, user.Id)};

            return item;
        }

        throw new Exception("An invalid data type was given when trying to load the Owner for the project or the owner" +
                            "for the project could not be retrieved. This should normally not happen. Please try again at a later" +
                            "time. If the issue persists please contact our support or if you're running in a custom environment your Administrator.");
    }

    public IPamProject LoadLastModifiedUser(IPamProject item)
    {
        if (item.LastModifiedUser.Data.UserId == null || item.LastModifiedUser.Data.UserId <= 0)
        {
            throw new ArgumentException(
                "The reached in object does not have the last modified users data included to load it properly. " +
                "Please reload the object from the database. Otherwise we cannot load the user who last modified this project and their information.", nameof(item));
        }
        
        using var context = new PgSqlContext();

        var lastModifiedUser = PamaxieDatabaseService.UserSingleton.Get(item.LastModifiedUser.Data.UserId);

        if (lastModifiedUser is IPamUser user)
        {
            item.LastModifiedUser = new LazyObject<(IPamUser User, long UserId)>() {IsLoaded = true, Data = (user, user.Id)};

            return item;
        }

        throw new Exception("An invalid data type was given when trying to load the user who last modified the project" +
                            "This should normally not happen. Please try again at a later" +
                            "time. If the issue persists please contact our support or if you're running in a custom environment your Administrator.");
    }

    public IPamProject LoadApiTokens(IPamProject item)
    {
        if (item.Id == null || item.Id <= 0)
        {
            throw new ArgumentException(
                "The reached in object does not have the id included, which means it is not loaded properly. " +
                "Please reload the object from the database. Otherwise we are unable to load the APi tokens for it", nameof(item));
        }
        
        using var context = new PgSqlContext();
        var projects = context.ApiKeys.Where(x => x.ProjectId == item.Id);

        item.ApiTokens = new LazyList<(string Token, DateTime LastUsage)>() {IsLoaded = true};
        
        foreach (var project in projects)
        {
            item.ApiTokens.Add((project.CredentialHash, project.TTL.Value.AddMonths(-6)));
        }

        return item;
    }

    public IPamProject LoadUsers(IPamProject item)
    {
        if (item.Id == null || item.Id <= 0)
        {
            throw new ArgumentException(
                "The reached in object does not have the id included, which means it is not loaded properly. " +
                "Please reload the object from the database. Otherwise we are unable to load the projects users", nameof(item));
        }
        
        using var context = new PgSqlContext();
        var users = context.ProjectUsers.Where(x => x.ProjectId == item.Id);

        item.Users = new LazyList<(long UserId, ProjectPermissions Permission)>() {IsLoaded = true};
        
        foreach (var project in users)
        {
            item.Users.Add((project.UserId, project.Permissions));
        }

        return item;
    }

    public bool ValidateToken(long projectId, string token)
    {
        using var context = new PgSqlContext();
        var apiKeys = context.ApiKeys.Where(x => x.ProjectId == projectId);

        foreach (var apiKey in apiKeys)
        {
            if (Argon2.Verify(token, apiKey.CredentialHash))
            {
                return true;
            }
        }

        return false;
    }

    public bool HasPermission(IPamProject project, ProjectPermissions permissions, string username)
    {
        return HasPermission(project.Id, 0, username, permissions);
    }

    public bool HasPermission(long projectId, ProjectPermissions permissions, long userId)
    {
        return HasPermission(projectId, userId, string.Empty, permissions);
    }

    public bool HasPermission(IPamProject project, ProjectPermissions permissions, long userId)
    {
        return HasPermission(project.Id, userId, string.Empty, permissions);
    }

    public bool HasPermission(long projectId, ProjectPermissions permissions, string username)
    {
        return HasPermission(projectId, 0, username, permissions);
    }

    public string CreateToken(long projectId)
    {
        if (projectId <= 0)
        {
            throw new ArgumentException("Invalid parameter. Project Id must be above zero.", nameof(projectId));
        }
        
        using var context = new PgSqlContext();
        var pw = CreatePassword();


        context.ApiKeys.Add(new ApiKey
        {
            ProjectId = projectId,
            CredentialHash = null,
            
            ///Expiration timeout if not used.
            TTL = DateTime.Now.AddMonths(6)
        });

        context.SaveChanges();

        return pw;
    }

    public bool RemoveToken(long projectId, long apiToken)
    {
        if (projectId <= 0)
        {
            throw new ArgumentException("Invalid parameter. Project Id must be above zero.", nameof(projectId));
        }
        
        if (apiToken <= 0)
        {
            throw new ArgumentException("Invalid parameter. Api Token Id must be above zero.", nameof(apiToken));
        }

        using var context = new PgSqlContext();

        if (!context.Projects.Any(x => x.Id == projectId))
        {
            return false;
        }

        var key = context.ApiKeys.FirstOrDefault(x => x.Id == apiToken && projectId == projectId);

        if (key == null)
        {
            return false;
        }
        
        context.Remove<ApiKey>(key);
        context.SaveChanges();
        return true;
    }

    public bool AddUser(long projectId, long userId, ProjectPermissions permissions)
    {
        if (projectId <= 0)
        {
            throw new ArgumentException("Invalid parameter. Project Id must be above zero.", nameof(projectId));
        }
        
        if (userId <= 0)
        {
            throw new ArgumentException("Invalid parameter. User Id must be above zero.", nameof(userId));
        }
        
        using var context = new PgSqlContext();
        
        if (!context.Users.Any(x => x.Id == userId))
        {
            return false;
        }

        if (!context.Projects.Any(x => x.Id == projectId))
        {
            return false;
        }

        context.ProjectUsers.Add(new ProjectUser(userId, projectId){Permissions = permissions});
        context.SaveChanges();

        return true;
    }

    public bool SetPermissions(long projectId, long userId, ProjectPermissions newPermissions)
    {
        if (projectId <= 0)
        {
            throw new ArgumentException("Invalid parameter. Project Id must be above zero.", nameof(projectId));
        }
        
        if (userId <= 0)
        {
            throw new ArgumentException("Invalid parameter. User Id must be above zero.", nameof(userId));
        }
        
        using var context = new PgSqlContext();
        
        if (!context.Projects.Any(x => x.Id == projectId))
        {
            return false;
        }
        
        if (!context.Users.Any(x => x.Id == userId))
        {
            return false;
        }

        var projectUser = context.ProjectUsers.FirstOrDefault(x => x.UserId == userId && x.ProjectId == projectId);
        projectUser.Permissions = newPermissions;
        context.SaveChanges();
        
        return true;
    }

    public bool RemoveUser(long projectId, long userId)
    {
        if (projectId <= 0)
        {
            throw new ArgumentException("Invalid parameter. Project Id must be above zero.", nameof(projectId));
        }
        
        if (userId <= 0)
        {
            throw new ArgumentException("Invalid parameter. User Id must be above zero.", nameof(userId));
        }
        
        using var context = new PgSqlContext();
        
        if (!context.Projects.Any(x => x.Id == projectId))
        {
            return false;
        }
        
        if (!context.Users.Any(x => x.Id == userId))
        {
            return false;
        }
        
        var projectUser = context.ProjectUsers.FirstOrDefault(x => x.UserId == userId && x.ProjectId == projectId);
        context.ProjectUsers.Remove(projectUser);
        context.SaveChanges();
        
        return true;
    }

    #region Helper Stuff

    private bool HasPermission(long projectId, long userId, string username, ProjectPermissions permissions)
    {
        using var context = new PgSqlContext(); 
        var dbProjects = context.ProjectUsers.Where(x => x.ProjectId == projectId);
        var user = new ProjectUser();

        if (string.IsNullOrWhiteSpace(username))
        {
            user = dbProjects.FirstOrDefault(x => x.UserId == userId);
        }
        else if (userId > 0)
        {
            var dbUser = context.Users.FirstOrDefault(x => x.Username == username);
            user = dbProjects.FirstOrDefault(x => x.UserId == dbUser.Id);
        }
        else {
            throw new InvalidOperationException("Please reach in either a username or a user id.");
        }

        if (user.Permissions >= permissions)
        {
            return true;
        }

        return false;
    }


    public static RandomNumberGenerator RngGenerator =  System.Security.Cryptography.RandomNumberGenerator.Create();
    
    private static string CreatePassword()
    {
        byte[] tokenBuffer = new byte[64];
        RngGenerator.GetNonZeroBytes(tokenBuffer);
        return Convert.ToBase64String(tokenBuffer);
    }

    #endregion
}