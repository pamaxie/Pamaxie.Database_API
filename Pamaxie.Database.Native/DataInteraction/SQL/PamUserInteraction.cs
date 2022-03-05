using System;
using System.Linq;
using System.Threading.Tasks;
using Pamaxie.Data;
using Pamaxie.Database.Extensions.DataInteraction;
using Pamaxie.Database.Native.DataInteraction.BusinessLogicExtensions;
using Pamaxie.Database.Native.Sql;

namespace Pamaxie.Database.Native.DataInteraction;

public class PamUserInteraction : PamSqlInteractionBase<User>, IPamUserInteraction
{
    /// <inheritdoc cref="GetAsync(string)"/>
    public override async Task<IPamSqlObject> GetAsync(long uniqueKey)
    {
        if (uniqueKey <= 0)
        {
            throw new ArgumentException("Invalid unique Key for the User", nameof(uniqueKey));
        }
        
        var item = await base.GetAsync(uniqueKey);
        
        if (item is not User user)
        {
            return null;
        }
        
        
        return user.ToUserLogic();
    }
    
    public override async Task<bool> CreateAsync(IPamSqlObject data)
    {
        if (data is IPamUser pamUser)
        {
            pamUser.Id = User.UserIdGenerator.CreateId();
            return await base.CreateAsync(pamUser.ToBusinessLogic());
        }
        
        return await base.CreateAsync(data);
    }

    public override async Task<bool> UpdateAsync(IPamSqlObject data)
    {
        if (data is IPamUser pamUser)
        {
            return await base.UpdateAsync(pamUser.ToBusinessLogic());
        }
        
        return await base.UpdateAsync(data);
    }

    public override async Task<bool> DeleteAsync(IPamSqlObject data)
    {
        if (data is IPamUser user)
        {
            return await base.DeleteAsync(user.ToBusinessLogic());
        }
        
        return await base.DeleteAsync(data);
    }

    /// <inheritdoc cref="GetAsync(string)"/>
    public async Task<IPamUser> GetAsync(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            throw new ArgumentNullException(nameof(username));
        }

        await using var context = new PgSqlContext();
        return context.Users.FirstOrDefault(x => x.Username == username)?.ToUserLogic();
    }

    ///<inheritdoc cref="ExistsUsernameAsync"/>
    public async Task<bool> ExistsUsernameAsync(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            throw new ArgumentNullException(nameof(username));
        }

        await using var context = new PgSqlContext();
        return context.Users.Any(x => x.Username == username);
    }

    ///<inheritdoc cref="ExistsUsernameAsync"/>
    public async Task<bool> ExistsEmailAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentNullException(nameof(email));
        }

        await using var context = new PgSqlContext();
        return context.Users.Any(x => x.Email == email);
    }

    /// <inheritdoc cref="LoadFullyAsync"/>
    public async Task<IPamUser> LoadFullyAsync(IPamUser user)
    {
        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }
        
        await LoadProjectsAsync(user);
        await LoadKnownIpsAsync(user);
        //await LoadTwoFactorOptionsAsync(user);

        return user;
    }

    /// <inheritdoc cref="LoadProjectsAsync"/>
    public async Task<IPamUser> LoadProjectsAsync(IPamUser user)
    {
        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        await using var context = new PgSqlContext();
        var projectUsers = context.ProjectUsers.Where(x => x.UserId == user.Id);
        user.Projects = new LazyList<(IPamProject Project, long ProjectId)>();

        foreach (var projectUser in projectUsers)
        {
            var dbData = await PamaxieDatabaseService.ProjectSingleton.GetAsync(projectUser.ProjectId);
            if (dbData is not Project projectData)
            {
                throw new Exception(
                    "Sadly we were not able to load the projects because of an internal server error. " +
                    "Please try again at a later time or contact your system administrator.");
            }

            var newProject = new PamProject
            {
                Id = projectData.Id,
                Name = projectData.Name,
                Owner = new LazyObject<(IPamUser User, long UserId)>() { Data = ( null, projectData.OwnerId ), IsLoaded = false },
                LastModifiedUser = new LazyObject<(IPamUser User, long UserId)>() { Data = ( null, projectData.LastModifiedUserId ), IsLoaded = false},
                CreationDate = projectData.CreationDate,
                LastModifiedAt = projectData.LastModified,
                Users = new LazyList<(long UserId, ProjectPermissions Permission)>() { IsLoaded = false },
                ApiTokens = new LazyList<(string Token, DateTime LastUsage)>(){IsLoaded = false},
                Flags = projectData.Flags,
                TTL = projectData.TTL
            };
            
            user.Projects.Add((newProject, projectUser.Id));
        }

        return user;
    }
    
    /// <inheritdoc cref="IPamUserInteraction.GetProjectPermissionsAsync(string,Pamaxie.Data.IPamUser)"/>
    public async Task<ProjectPermissions> GetProjectPermissionsAsync(string projectName, IPamUser user)
    {
        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }
        
        if (string.IsNullOrWhiteSpace(projectName))
        {
            throw new ArgumentNullException(nameof(projectName));
        }

        await using var context = new PgSqlContext();
        var project = context.Projects.FirstOrDefault(x => x.Name == projectName);
        var projectUser = context.ProjectUsers.FirstOrDefault(x => x.ProjectId == project.Id && x.UserId == user.Id);
        return projectUser?.Permissions ?? ProjectPermissions.None;
    }
    
    /// <inheritdoc cref="IPamUserInteraction.GetProjectPermissionsAsync(long,Pamaxie.Data.IPamUser)"/>
    public async Task<ProjectPermissions> GetProjectPermissionsAsync(long projectId, IPamUser user) 
    {
        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }
        
        if (projectId == 0)
        {
            throw new ArgumentException("Invalid Project Id", nameof(projectId));
        }

        await using var context = new PgSqlContext();
        var projectUser = context.ProjectUsers.FirstOrDefault(x => x.ProjectId == projectId && x.UserId == user.Id);
        return projectUser?.Permissions ?? ProjectPermissions.None;
    }
    
    /*/// <inheritdoc cref="LoadTwoFactorOptionsAsync"/>
    public async Task<IPamUser> LoadTwoFactorOptionsAsync(IPamUser user)
    {   
        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        await using var context = new PgSqlContext();
        var twoFactorOptions = context.TwoFactorUsers.Where(x => x.UserId == user.Id);
        
        foreach (var twoFactorOption in twoFactorOptions)
        {
            user.TwoFactorOptions.Add((twoFactorOption.Type));
        }

        return user;
    }*/
    
    /// <inheritdoc cref="LoadKnownIpsAsync"/>
    public async Task<IPamUser> LoadKnownIpsAsync(IPamUser user)
    {
        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        await using var context = new PgSqlContext();
        var knownIps = context.KnownUserIps.Where(x => x.UserId == user.Id);
        foreach (var knownIp in knownIps)
        {
            user.KnownIps.Add(knownIp.IpAddress);
        }

        return user;
    }
    
    /// <inheritdoc cref="IsIpKnownAsync"/>
    public async Task<bool> IsIpKnownAsync(IPamUser user, string ipAddress)
    {
        await using var context = new PgSqlContext();
        return context.KnownUserIps.Any(x => x.UserId == user.Id && x.IpAddress == ipAddress);
    }

    /// <inheritdoc cref="GetUniqueKeyAsync"/>
    public async Task<long> GetUniqueKeyAsync(string username)
    {
        await using var context = new PgSqlContext();
        return context.Users.FirstOrDefault(x => x.Username == username)?.Id ?? 0;
    }

    /// <inheritdoc cref="SetConfirmationCodeAsync"/>
    public async Task<bool> SetConfirmationCodeAsync(long userId, string confirmationCode)
    {
        await using var context = new PgSqlContext();

        if (context.EmailConfirmations.Any(x => x.UserId == userId))
        {
            context.EmailConfirmations.Update(new EmailConfirmation()
                {UserId = userId, ConfirmationCode = confirmationCode});
        }
        else
        {
            await context.EmailConfirmations.AddAsync(new EmailConfirmation()
                {UserId = userId, ConfirmationCode = confirmationCode});
        }

        await context.SaveChangesAsync();

        return true;
    }

    /// <inheritdoc cref="ValidateConfirmationCodeAsync"/>
    public async Task<(bool ConfirmationSuccessful, long UserId)> ValidateConfirmationCodeAsync(string confirmationCode)
    {
        await using var context = new PgSqlContext();
        if (!context.EmailConfirmations.Any(x => x.ConfirmationCode == confirmationCode))
        {
            return (false, 0);
        }

        var confirmation = context.EmailConfirmations.FirstOrDefault(x => x.ConfirmationCode == confirmationCode);

        if (confirmation == null)
        {
            return (false, 0);
        }
        
        //context.EmailConfirmations.Remove(confirmation);
        //await context.SaveChangesAsync();
        return (true, confirmation.UserId);
    }
}