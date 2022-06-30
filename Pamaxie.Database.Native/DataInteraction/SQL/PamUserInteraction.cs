using System;
using System.Linq;
using System.Threading.Tasks;
using Isopoh.Cryptography.Argon2;
using Microsoft.EntityFrameworkCore;
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

        //Making sure the password hash isn't there.
        user.PasswordHash = string.Empty;
        return user.ToUserLogic();
    }
    
    public override async Task<(bool, long)> CreateAsync(IPamSqlObject data)
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
        await using var context = new PgSqlContext();
        
        if (data is IPamUser pamUser)
        {
            var user = pamUser.ToBusinessLogic();

            await VerifyUserBeforeUpdate(user);


            return await base.UpdateAsync(user);
        }
        
        await VerifyUserBeforeUpdate((User)data);
        return await base.UpdateAsync(data);
    }

    private async Task<User> VerifyUserBeforeUpdate(User user)
    {
        await using var context = new PgSqlContext();
        
        if (string.IsNullOrWhiteSpace(user.PasswordHash))
        {
            user.PasswordHash = context.Users.FirstOrDefault(x => x.Id == user.Id)?.PasswordHash;
        }
            
        if (string.IsNullOrWhiteSpace(user.Email))
        {
            user.Email = context.Users.FirstOrDefault(x => x.Id == user.Id)?.Email;
        }

        if (user.CreationDate == DateTime.MinValue)
        {
            user.CreationDate = context.Users.FirstOrDefault(x => x.Id == user.Id)?.CreationDate ?? DateTime.Now;
        }

        //Making sure flags are loaded if they haven't been changed by staff
        if (user.Flags == UserFlags.None)
        {
            user.Flags = context.Users.FirstOrDefault(x => x.Id == user.Id)?.Flags ?? UserFlags.None;
        }

        return user;
    }

    public override async Task<bool> DeleteAsync(IPamSqlObject data)
    {
        if (data is IPamUser user)
        {
            await using var context = new PgSqlContext();
            var projectUsers = context.ProjectUsers.Where(x => x.Id == user.Id);
            var ownedProjects = context.Projects.Where(x => x.OwnerId == user.Id);
            var knownIps = context.KnownUserIps.Where(x => x.UserId == user.Id);
            var emailConfirmations = context.EmailConfirmations.Where(x => x.UserId == user.Id);
            context.RemoveRange(projectUsers);
            context.RemoveRange(ownedProjects);
            context.RemoveRange(knownIps);
            context.RemoveRange(emailConfirmations);
            
            if (await base.DeleteAsync(user.ToBusinessLogic()))
            {
                //Only apply the changes if the user deletion was successful.
                await context.SaveChangesAsync();
                return true;
            }
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
    
    /// <inheritdoc cref="GetViaMailAsync(string)"/>
    public async Task<IPamUser> GetViaMailAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentNullException(nameof(email));
        }

        await using var context = new PgSqlContext();
        return context.Users.FirstOrDefault(x => x.Email == email)?.ToUserLogic();
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
    
    /// <inheritdoc cref="ValidatePassword"/>
    public async Task<bool> ValidatePassword(string password, long userId)
    {
        await using var context = new PgSqlContext();
        var user = await context.Users.FirstOrDefaultAsync(x => x.Id == userId);
        return user != null && Argon2.Verify(user.PasswordHash, password);
    }

    /// <inheritdoc cref="LoadFullyAsync"/>
    public async Task<IPamUser> LoadFullyAsync(IPamUser user)
    {
        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        if (user.Id < 1)
        {
            throw new ArgumentException("Invalid user id", nameof(user.Id));
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
            if (dbData is not PamProject projectData)
            {
                throw new Exception(
                    "Sadly we were not able to load the projects because of an internal server error. " +
                    "Please try again at a later time or contact your system administrator.");
            }

            user.Projects.Add((projectData, projectUser.Id));
        }

        return user;
    }
    
    /// <inheritdoc cref="IPamUserInteraction.GetProjectPermissionsAsync(string,Pamaxie.Data.IPamUser)"/>
    public async Task<ProjectPermissions> GetProjectPermissionsAsync(string projectName, long userId)
    {
        if (userId < 1)
        {
            throw new ArgumentException("Invalid User Id",nameof(userId));
        }
        
        if (string.IsNullOrWhiteSpace(projectName))
        {
            throw new ArgumentNullException(nameof(projectName));
        }

        await using var context = new PgSqlContext();
        var project = context.Projects.FirstOrDefault(x => x.Name == projectName);
        var projectUser = context.ProjectUsers.FirstOrDefault(x => x.ProjectId == project.Id && x.UserId == userId);
        return projectUser?.Permissions ?? ProjectPermissions.None;
    }
    
    /// <inheritdoc cref="IPamUserInteraction.GetProjectPermissionsAsync(long,Pamaxie.Data.IPamUser)"/>
    public async Task<ProjectPermissions> GetProjectPermissionsAsync(long projectId, long userId) 
    {
        if (userId < 0)
        {
            throw new ArgumentException("Invalid User Id",nameof(userId));
        }
        
        if (projectId == 0)
        {
            throw new ArgumentException("Invalid Project Id", nameof(projectId));
        }

        await using var context = new PgSqlContext();
        var projectUser = context.ProjectUsers.FirstOrDefault(x => x.ProjectId == projectId && x.UserId == userId);
        return projectUser?.Permissions ?? ProjectPermissions.None;
    }

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
    public async Task<bool> IsIpKnownAsync(long userId, string ipAddress)
    {
        await using var context = new PgSqlContext();
        return context.KnownUserIps.Any(x => x.UserId == userId && x.IpAddress == ipAddress);
    }

    /// <inheritdoc cref="GetUniqueKeyAsync"/>
    public async Task<long> GetUniqueKeyAsync(string username)
    {
        await using var context = new PgSqlContext();
        return context.Users.FirstOrDefault(x => x.Username == username)?.Id ?? 0;
    }

    /// <inheritdoc cref="SetConfirmationCodeAsync"/>
    public async Task<bool> SetConfirmationCodeAsync(long userId, string confirmationCode, bool is_refresh = false)
    {
        await using var context = new PgSqlContext();

        if (context.EmailConfirmations.Any(x => x.UserId == userId))
        {
            context.EmailConfirmations.Update(new EmailConfirmation()
                {UserId = userId, ConfirmationCode = confirmationCode});
        }
        else
        {
            if (is_refresh){
                return false;
            }

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
        
        context.EmailConfirmations.Remove(confirmation);
        await context.SaveChangesAsync();
        return (true, confirmation.UserId);
    }
}