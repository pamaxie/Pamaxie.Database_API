using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Pamaxie.Data;
using Pamaxie.Database.Native.Sql;

namespace Pamaxie.Database.Native.DataInteraction.BusinessLogicExtensions;

public class PamUserInteraction : PamSqlInteractionBase<User>, Extensions.DataInteraction.IPamUserInteraction
{
    /// <inheritdoc cref="Get"/>
    public override IPamSqlObject Get(long uniqueKey)
    {
        if (uniqueKey <= 0)
        {
            throw new ArgumentException("Invalid unique Key for the User", nameof(uniqueKey));
        }
        
        var item = base.Get(uniqueKey);
        
        if (item is not User user)
        {
            return null;
        }
        
        
        return user.ToIPamUser();
    }
    
    public override bool Create(IPamSqlObject data)
    {
        if (data is IPamUser pamUser)
        {
            pamUser.Id = User.UserIdGenerator.CreateId();
            return base.Create(pamUser.ToDbUser());
        }
        
        return base.Create(data);
    }

    public override bool Update(IPamSqlObject data)
    {
        if (data is IPamUser pamUser)
        {
            return base.Update(pamUser.ToDbUser());
        }
        
        return base.Update(data);
    }

    public override bool UpdateOrCreate(IPamSqlObject data)
    {
        return base.UpdateOrCreate(data);
    }
    
    /// <inheritdoc cref="Get"/>
    public IPamUser Get(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            throw new ArgumentNullException(nameof(username));
        }
        
        using var context = new PgSqlContext();
        return context.Users.FirstOrDefault(x => x.Username == username)?.ToIPamUser();
    }

    ///<inheritdoc cref="ExistsUsername"/>
    public bool ExistsUsername(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            throw new ArgumentNullException(nameof(username));
        }
        
        using var context = new PgSqlContext();
        return context.Users.Any(x => x.Username == username);
    }

    ///<inheritdoc cref="ExistsUsername"/>
    public bool ExistsEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentNullException(nameof(email));
        }
        
        using var context = new PgSqlContext();
        return context.Users.Any(x => x.Email == email);
    }

    /// <inheritdoc cref="LoadFully"/>
    public IPamUser LoadFully(IPamUser user)
    {
        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }
        
        LoadProjects(user);
        LoadKnownIps(user);
        LoadTwoFactorOptions(user);

        return user;
    }

    /// <inheritdoc cref="LoadProjects"/>
    public IPamUser LoadProjects(IPamUser user)
    {
        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        using var context = new PgSqlContext();
        var projects = context.ProjectUsers.Where(x => x.UserId == user.Id);
        user.Projects = new LazyList<(IPamProject Project, long ProjectId)>();

        foreach (var project in projects)
        {
            var dbData = PamaxieDatabaseService.ProjectSingleton.Get(project.ProjectId);
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
                LastModified = projectData.LastModified,
                Users = new LazyList<(long UserId, ProjectPermissions Permission)>() { IsLoaded = false },
                ApiTokens = new LazyList<(string Token, DateTime LastUsage)>(){IsLoaded = false},
                Flags = projectData.Flags,
                TTL = projectData.TTL
            };
            
            user.Projects.Add((newProject, project.Id));
        }

        return user;
    }
    
    /// <inheritdoc cref="GetProjectPermissions"/>
    public ProjectPermissions GetProjectPermissions(string projectName, IPamUser user)
    {
        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }
        
        if (string.IsNullOrWhiteSpace(projectName))
        {
            throw new ArgumentNullException(nameof(projectName));
        }
        
        using var context = new PgSqlContext();
        var project = context.Projects.FirstOrDefault(x => x.Name == projectName);
        var projectUser = context.ProjectUsers.FirstOrDefault(x => x.ProjectId == project.Id && x.UserId == user.Id);
        return projectUser?.Permissions ?? ProjectPermissions.None;
    }
    
    /// <inheritdoc cref="GetProjectPermissions"/>
    public ProjectPermissions GetProjectPermissions(long projectId, IPamUser user) 
    {
        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }
        
        if (projectId == 0)
        {
            throw new ArgumentException("Invalid Project Id", nameof(projectId));
        }
        
        using var context = new PgSqlContext();
        var projectUser = context.ProjectUsers.FirstOrDefault(x => x.ProjectId == projectId && x.UserId == user.Id);
        return projectUser?.Permissions ?? ProjectPermissions.None;
    }
    
    /// <inheritdoc cref="LoadTwoFactorOptions"/>
    public IPamUser LoadTwoFactorOptions(IPamUser user)
    {   
        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }
        
        using var context = new PgSqlContext();
        var twoFactorOptions = context.TwoFactorUsers.Where(x => x.UserId == user.Id);
        
        foreach (var twoFactorOption in twoFactorOptions)
        {
            user.TwoFactorOptions.Add((twoFactorOption.Type, twoFactorOption.PublicKey));
        }

        return user;
    }
    
    /// <inheritdoc cref="LoadKnownIps"/>
    public IPamUser LoadKnownIps(IPamUser user)
    {
        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }
        
        using var context = new PgSqlContext();
        var knownIps = context.KnownUserIps.Where(x => x.UserId == user.Id);
        foreach (var knownIp in knownIps)
        {
            user.KnownIps.Add(knownIp.IpAddress);
        }

        return user;
    }
    
    /// <inheritdoc cref="IsIpKnown"/>
    public bool IsIpKnown(IPamUser user, string ipAddress)
    {
        using var context = new PgSqlContext();
        return context.KnownUserIps.Any(x => x.UserId == user.Id && x.IpAddress == ipAddress);
    }

    /// <inheritdoc cref="GetUniqueKey(string)"/>
    public long GetUniqueKey(string username)
    {
        using var context = new PgSqlContext();
        return context.Users.FirstOrDefault(x => x.Username == username)?.Id ?? 0;
    }
}