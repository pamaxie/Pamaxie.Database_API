using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Isopoh.Cryptography.Argon2;
using Microsoft.EntityFrameworkCore;
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

        return project.ToUserLogic();
    }

    public  override async Task<(bool, long)> CreateAsync(IPamSqlObject data)
    {
        if (data == null)
        {
            throw new ArgumentException(nameof(data));
        }
        
        await using var context = new PgSqlContext();

        if (data is PamProject pamProject)
        {
            pamProject.Id = Project.ProjectIdGenerator.CreateId();

            var businessObject = pamProject.ToBusinessLogic();
            businessObject.CreationDate = DateTime.Now;
            return await base.CreateAsync(businessObject);
        }

        return await base.CreateAsync(data);
    }
    
    public override Task<bool> DeleteAsync(IPamSqlObject data)
    {
        if (data == null)
        {
            throw new ArgumentNullException(nameof(data));
        }
        
        if (data is IPamProject project)
        {
            return base.DeleteAsync(project.ToBusinessLogic());
        }
        
        return base.DeleteAsync(data);
    }

    public override async Task<bool> UpdateAsync(IPamSqlObject data)
    {
        if (data == null)
        {
            throw new ArgumentNullException(nameof(data));
        }
        
        if (data is IPamProject project)
        {
            await using var context = new PgSqlContext();
            var toBeCreatedProject = project.ToBusinessLogic();
            toBeCreatedProject.CreationDate = context.Projects.FirstOrDefault(x => x.Id == data.Id)?.CreationDate ?? DateTime.Now;

            //Making sure flags are loaded if they haven't been changed by staff
            if (toBeCreatedProject.Flags == ProjectFlags.None)
            {
                toBeCreatedProject.Flags = context.Projects.FirstOrDefault(x => x.Id == data.Id)?.Flags ?? ProjectFlags.None;
            }
            
            return await base.UpdateAsync(toBeCreatedProject);
        }
        return await base.UpdateAsync(data);
    }

    public async Task<IPamProject> LoadOwnerAsync(IPamProject item)
    {
        if (item == null)
        {
            throw new ArgumentException(nameof(item));
        }
        
        if (item.Id <= 0)
        {
            throw new ArgumentException(
                "The reached in object does not have a valid Id set. " +
                "Please reload the object from the database. Otherwise we cannot load the users Owner information.", nameof(item));
        }

        await using var context = new PgSqlContext();

        var project = await context.Projects.FirstOrDefaultAsync(x => x.Id == item.Id);

        if (project == null)
        {
            throw new ArgumentException("The reached in project cannot be found in our database.");
        }
        var owner = await PamaxieDatabaseService.UserSingleton.GetAsync(project.OwnerId);

        if (owner is PamUser user)
        {
            item.Owner = new LazyObject<(PamUser User, long UserId)>() {IsLoaded = true, Data = (user, user.Id)};

            return item;
        }

        throw new Exception("An invalid data type was given when trying to load the Owner for the project or the owner" +
                            "for the project could not be retrieved. This should normally not happen. Please try again at a later" +
                            "time. If the issue persists please contact our support or if you're running in a custom environment your Administrator.");
    }

    public async Task<IPamProject> LoadLastModifiedUserAsync(IPamProject item)
    {
        if (item == null)
        {
            throw new ArgumentException(nameof(item));
        }
        
        if (item.Id <= 0)
        {
            throw new ArgumentException(
                "The reached in object does not have a valid item id. " +
                "Please reload the object from the database. Otherwise we cannot load the user who last modified this project and their information.", nameof(item));
        }

        await using var context = new PgSqlContext();
        var project = await context.Projects.FirstOrDefaultAsync(x => x.Id == item.Id);
        
        if (project == null)
        {
            throw new ArgumentException("The specified project Id does not exist");
        }

        var lastModifiedUser = await PamaxieDatabaseService.UserSingleton.GetAsync(project.LastModifiedUserId);

        if (lastModifiedUser is PamUser user)
        {
            item.LastModifiedUser = new LazyObject<(PamUser User, long UserId)>() {IsLoaded = true, Data = (user, user.Id)};

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

        item.ApiTokens = new LazyList<(string Token, long TokenId)>() {IsLoaded = true};
        
        foreach (var project in projects)
        {
            item.ApiTokens.Add((project.CredentialHash, project.Id));
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

    public async Task<IPamProject> LoadFullyAsync(IPamProject item)
    {
        item = await LoadOwnerAsync(item);
        item = await LoadLastModifiedUserAsync(item);
        item = await LoadApiTokensAsync(item);
        item = await LoadUsersAsync(item);
        
        return item;
    }
    
    public async Task<bool> IsTokenActiveAsync(long tokenId)
    {
        if (tokenId <= 0)
        {
            throw new ArgumentException(
                "The reached in token Id is invalid", nameof(tokenId));
        }
        
        await using var context = new PgSqlContext();
        return await context.ApiKeys.AnyAsync(x => x.Id == tokenId);
    }

    public async Task<bool> IsPamProject(long projectId)
    {
        await using var context = new PgSqlContext();
        var project = await context.Projects.FirstOrDefaultAsync(x => x.Id == projectId);
        
        if (project == null)
        {
            throw new ArgumentException("This project does not exist in our database.");
        }

        return await context.Users.AnyAsync(x => x.Id == project.OwnerId && x.Flags.HasFlag(UserFlags.PamaxieStaff));
    }

    public async Task<(bool, long, long)> ValidateTokenAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new ArgumentNullException(nameof(token));
        }
        
        await using var context = new PgSqlContext();
        
        if (!token.StartsWith("PamToken/-//"))
        {
            return (false, 0, 0);
        }

        var tokenParts = token.Split("/-//");
        
        if (tokenParts.Length < 3)
        {
            return (false, 0, 0);
        }
        
        if (!long.TryParse(tokenParts[1], out long projectId))
        {
            return (false, 0, 0);
        }

        var project = await context.Projects.FirstOrDefaultAsync(x => x.Id == projectId);

        if (project == null || project.Flags.HasFlag(ProjectFlags.Locked))
        {
            return (false, 0, 0);
        }
        
        var apiKeys = context.ApiKeys.Where(x => x.ProjectId == projectId);

        foreach (var apiKey in apiKeys)
        {
            if (Argon2.Verify(apiKey.CredentialHash, token))
            {
                return (true, projectId, apiKey.Id);
            }
        }

        return (false, 0, 0);
    }

    public async Task<bool> HasPermissionAsync(long projectId,long userId, ProjectPermissions permissions)
    {
        await using var context = new PgSqlContext();   
        
        var project = await context.Projects.FirstOrDefaultAsync(x => x.Id == projectId);

        if (project == null)
        {
            return false;
        }

        if (project.OwnerId == userId)
        {
            return true;
        }

        var projectUsers = context.ProjectUsers.Where(x => x.ProjectId == projectId);
        var user = await projectUsers.FirstOrDefaultAsync(x => x.UserId == userId);

        if (user != null && user.Permissions >= permissions)
        {
            return true;
        }

        return false;
    }

    public async Task<string> CreateTokenAsync(long projectId, DateTime expiryTime)
    {
        if (projectId <= 0)
        {
            throw new ArgumentException("Invalid parameter. Project Id must be above zero.", nameof(projectId));
        }
        
        if(!await ExistsAsync(projectId))
        {
            return null;
        }

        await using var context = new PgSqlContext();
        
        var apiKey = new ApiKey
        {
            ProjectId = projectId,
            TTL = expiryTime == DateTime.MaxValue ? expiryTime : null
        };
        
        var privateToken = apiKey.CreateToken();
        context.ApiKeys.Add(apiKey);
        await context.SaveChangesAsync();
        return privateToken;
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

    public async Task<bool> HasUserAsync(long projectId, long userId)
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
        return await context.ProjectUsers.AnyAsync(x => x.UserId == userId && x.ProjectId == projectId);
    }

    public async Task<bool> IsOwnerAsync(long projectId, long userId)
    {
        await using var context = new PgSqlContext();
        return await context.Projects.AnyAsync(x => x.Id == projectId && x.OwnerId == userId);
    }
    
}