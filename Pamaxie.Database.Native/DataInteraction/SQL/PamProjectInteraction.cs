using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Isopoh.Cryptography.Argon2;
using Pamaxie.Data;
using Pamaxie.Database.Extensions.DataInteraction;
using Pamaxie.Database.Native.DataInteraction.BusinessLogicExtensions;
using Pamaxie.Database.Native.Sql;

namespace Pamaxie.Database.Native.DataInteraction;

public class PamProjectInteraction : PamSqlInteractionBase<Project>, IPamProjectInteraction
{
    public override async Task<IPamSqlObject> GetAsync(long uniqueKey)
    {
        if (uniqueKey <= 0)
        {
            throw new ArgumentException("Invalid unique Key for the Project", nameof(uniqueKey));
        }
        
        var item = await base.GetAsync(uniqueKey);
        
        if (item is not Project project)
        {
            return null;
        }

        return project.ToIPamProject();
    }

    public  override async Task<bool> CreateAsync(IPamSqlObject data)
    {
        await using var context = new PgSqlContext();
        
        if (data is Project project)
        {
            context.ProjectUsers.Add(new ProjectUser(project.OwnerId, project.Id));
            await context.SaveChangesAsync();
        }

        if (data is IPamProject pamProject)
        {
            pamProject.Id = Project.ProjectIdGenerator.CreateId();
            return await base.CreateAsync(pamProject.ToDbProject());
        }

        return await base.CreateAsync(data);
    }

    public override async Task<bool> UpdateOrCreateAsync(IPamSqlObject data)
    {
        if (!await ExistsAsync(data.Id))
        {
            return await CreateAsync(data);
        }
        
        return await base.UpdateOrCreateAsync(data);
    }

    public async Task<IPamProject> LoadOwnerAsync(IPamProject item)
    {
        if (item.Owner == null || item.Owner.Data.UserId <= 0)
        {
            throw new ArgumentException(
                "The reached in object does not have the owner data included to load it properly. " +
                "Please reload the object from the database. Otherwise we cannot load the users Owner information.", nameof(item));
        }

        await using var context = new PgSqlContext();

        var owner = await PamaxieDatabaseService.UserSingleton.GetAsync(item.Owner.Data.UserId);

        if (owner is IPamUser user)
        {
            item.Owner = new LazyObject<(IPamUser User, long UserId)>() {IsLoaded = true, Data = (user, user.Id)};

            return item;
        }

        throw new Exception("An invalid data type was given when trying to load the Owner for the project or the owner" +
                            "for the project could not be retrieved. This should normally not happen. Please try again at a later" +
                            "time. If the issue persists please contact our support or if you're running in a custom environment your Administrator.");
    }

    public async Task<IPamProject> LoadLastModifiedUserAsync(IPamProject item)
    {
        if (item.LastModifiedUser == null || item.LastModifiedUser.Data.UserId <= 0)
        {
            throw new ArgumentException(
                "The reached in object does not have the last modified users data included to load it properly. " +
                "Please reload the object from the database. Otherwise we cannot load the user who last modified this project and their information.", nameof(item));
        }

        await using var context = new PgSqlContext();

        var lastModifiedUser = await PamaxieDatabaseService.UserSingleton.GetAsync(item.LastModifiedUser.Data.UserId);

        if (lastModifiedUser is IPamUser user)
        {
            item.LastModifiedUser = new LazyObject<(IPamUser User, long UserId)>() {IsLoaded = true, Data = (user, user.Id)};

            return item;
        }

        throw new Exception("An invalid data type was given when trying to load the user who last modified the project" +
                            "This should normally not happen. Please try again at a later" +
                            "time. If the issue persists please contact our support or if you're running in a custom environment your Administrator.");
    }

    public async Task<IPamProject> LoadApiTokensAsync(IPamProject item)
    {
        if (item == null || item.Id <= 0)
        {
            throw new ArgumentException(
                "The reached in object does not have the id included, which means it is not loaded properly. " +
                "Please reload the object from the database. Otherwise we are unable to load the APi tokens for it", nameof(item));
        }

        await using var context = new PgSqlContext();
        var projects = context.ApiKeys.Where(x => x.ProjectId == item.Id);

        item.ApiTokens = new LazyList<(string Token, DateTime LastUsage)>() {IsLoaded = true};
        
        foreach (var project in projects)
        {
            if (project.TTL != null)
            {
                item.ApiTokens.Add((project.CredentialHash, project.TTL.Value.AddMonths(-6)));
            }
        }

        return item;
    }

    public async Task<IPamProject> LoadUsersAsync(IPamProject item)
    {
        if (item == null || item.Id <= 0)
        {
            throw new ArgumentException(
                "The reached in object does not have the id included, which means it is not loaded properly. " +
                "Please reload the object from the database. Otherwise we are unable to load the projects users", nameof(item));
        }

        await using var context = new PgSqlContext();
        var users = context.ProjectUsers.Where(x => x.ProjectId == item.Id);

        item.Users = new LazyList<(long UserId, ProjectPermissions Permission)>() {IsLoaded = true};
        
        foreach (var project in users)
        {
            item.Users.Add((project.UserId, project.Permissions));
        }

        return item;
    }

    public async Task<bool> ValidateTokenAsync(long projectId, string token)
    {
        await using var context = new PgSqlContext();
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

    public async Task<bool> HasPermissionAsync(IPamProject project, ProjectPermissions permissions, string username)
    {
        return await HasPermissionAsync(project.Id, 0, username, permissions);
    }

    public async Task<bool> HasPermissionAsync(long projectId, ProjectPermissions permissions, long userId)
    {
        return await HasPermissionAsync(projectId, userId, string.Empty, permissions);
    }

    public async Task<bool> HasPermissionAsync(IPamProject project, ProjectPermissions permissions, long userId)
    {
        return await HasPermissionAsync(project.Id, userId, string.Empty, permissions);
    }

    public async Task<bool> HasPermissionAsync(long projectId, ProjectPermissions permissions, string username)
    {
        return await HasPermissionAsync(projectId, 0, username, permissions);
    }

    public async Task<string> CreateTokenAsync(long projectId)
    {
        if (projectId <= 0)
        {
            throw new ArgumentException("Invalid parameter. Project Id must be above zero.", nameof(projectId));
        }

        await using var context = new PgSqlContext();
        var pw = CreatePassword();


        context.ApiKeys.Add(new ApiKey
        {
            ProjectId = projectId,
            CredentialHash = null,
            
            //Expiration timeout if not used.
            TTL = DateTime.Now.AddMonths(6)
        });

        await context.SaveChangesAsync();

        return pw;
    }

    public async Task<bool> RemoveTokenAsync(long projectId, long apiToken)
    {
        if (projectId <= 0)
        {
            throw new ArgumentException("Invalid parameter. Project Id must be above zero.", nameof(projectId));
        }
        
        if (apiToken <= 0)
        {
            throw new ArgumentException("Invalid parameter. Api Token Id must be above zero.", nameof(apiToken));
        }

        await using var context = new PgSqlContext();

        if (!context.Projects.Any(x => x.Id == projectId))
        {
            return false;
        }

        var key = context.ApiKeys.FirstOrDefault(x => x.Id == apiToken);

        if (key == null)
        {
            return false;
        }
        
        context.Remove(key);
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> AddUserAsync(long projectId, long userId, ProjectPermissions permissions)
    {
        if (projectId <= 0)
        {
            throw new ArgumentException("Invalid parameter. Project Id must be above zero.", nameof(projectId));
        }
        
        if (userId <= 0)
        {
            throw new ArgumentException("Invalid parameter. User Id must be above zero.", nameof(userId));
        }

        await using var context = new PgSqlContext();
        
        if (!context.Users.Any(x => x.Id == userId))
        {
            return false;
        }

        if (!context.Projects.Any(x => x.Id == projectId))
        {
            return false;
        }

        context.ProjectUsers.Add(new ProjectUser(userId, projectId){Permissions = permissions});
        await context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> SetPermissionsAsync(long projectId, long userId, ProjectPermissions newPermissions)
    {
        if (projectId <= 0)
        {
            throw new ArgumentException("Invalid parameter. Project Id must be above zero.", nameof(projectId));
        }
        
        if (userId <= 0)
        {
            throw new ArgumentException("Invalid parameter. User Id must be above zero.", nameof(userId));
        }

        await using var context = new PgSqlContext();
        
        if (!context.Projects.Any(x => x.Id == projectId))
        {
            return false;
        }
        
        if (!context.Users.Any(x => x.Id == userId))
        {
            return false;
        }

        var projectUser = context.ProjectUsers.FirstOrDefault(x => x.UserId == userId && x.ProjectId == projectId);
        
        if (projectUser != null)
        {
            projectUser.Permissions = newPermissions;
        }
        
        await context.SaveChangesAsync();
        
        return true;
    }

    public async Task<bool> RemoveUserAsync(long projectId, long userId)
    {
        if (projectId <= 0)
        {
            throw new ArgumentException("Invalid parameter. Project Id must be above zero.", nameof(projectId));
        }
        
        if (userId <= 0)
        {
            throw new ArgumentException("Invalid parameter. User Id must be above zero.", nameof(userId));
        }

        await using var context = new PgSqlContext();
        
        if (!context.Projects.Any(x => x.Id == projectId))
        {
            return false;
        }
        
        if (!context.Users.Any(x => x.Id == userId))
        {
            return false;
        }
        
        var projectUser = context.ProjectUsers.FirstOrDefault(x => x.UserId == userId && x.ProjectId == projectId);
        
        if (projectUser != null)
        {
            context.ProjectUsers.Remove(projectUser);
        }
        
        await context.SaveChangesAsync();
        
        return true;
    }

    #region Helper Stuff

    private async Task<bool> HasPermissionAsync(long projectId, long userId, string username, ProjectPermissions permissions)
    {
        await using var context = new PgSqlContext(); 
        var dbProjects = context.ProjectUsers.Where(x => x.ProjectId == projectId);
        ProjectUser user;

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

        if (user != null && user.Permissions >= permissions)
        {
            return true;
        }

        return false;
    }


    private static readonly RandomNumberGenerator RngGenerator =  RandomNumberGenerator.Create();
    
    private static string CreatePassword()
    {
        var tokenBuffer = new byte[64];
        RngGenerator.GetNonZeroBytes(tokenBuffer);
        return Convert.ToBase64String(tokenBuffer);
    }

    #endregion
}