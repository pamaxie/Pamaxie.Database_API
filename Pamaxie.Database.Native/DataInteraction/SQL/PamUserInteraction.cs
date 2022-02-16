using System;
using System.Linq;
using Pamaxie.Data;
using Pamaxie.Database.Native.Sql;

namespace Pamaxie.Database.Native.DataInteraction.BusinessLogicExtensions;

public class PamUserInteraction : PamSqlInteractionBase<User>, Extensions.DataInteraction.IPamUserInteraction
{
    /// <inheritdoc cref="Get"/>
    public override IPamSqlObject Get(long uniqueKey)
    {
        var item = base.Get(uniqueKey);
        
        if (item is not User user)
        {
            return null;
        }

        return user.ToIPamUser();
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
        var dbUser = context.Users.FirstOrDefault(x => x.Id == user.Id);
        var projects = dbUser.Projects.ToList();
        user.Projects = new LazyList<IPamProject>();

        foreach (var project in projects)
        {
            user.Projects.Add(new PamProject
            {
                Id = project.Project.Id,
                Name = project.Project.Name,
                Owner = new LazyObject<IPamUser, string>(){IsLoaded = false},
                LastModifiedUser = new LazyObject<IPamUser, string>(){IsLoaded = false},
                CreationDate = project.Project.CreationDate,
                LastModified = project.Project.LastModified,
                Users = new LazyList<(string UserName, ProjectPermissions Permission)>(){IsLoaded = false},
                ApiTokens = new LazyList<(string Token, DateTime LastUsage)>(){IsLoaded = false},
                Flags = project.Project.Flags,
                TTL = project.Project.TTL
            });
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
        var projectUser = project.Users.FirstOrDefault(x => x.Id == user.Id);

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
        var project = context.Projects.FirstOrDefault(x => x.Id == projectId);
        var projectUser = project.Users.FirstOrDefault(x => x.Id == user.Id);

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
        var twoFactorOptions = context.TwoFactorUsers.Where(x => x.User.Id == user.Id);
        
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
        var knownIps = context.KnownUserIps.Where(x => x.User.Id == user.Id);
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
        return context.KnownUserIps.Any(x => x.User.Id == user.Id && x.IpAddress == ipAddress);
    }

    /// <inheritdoc cref="GetUniqueKey(string)"/>
    public long GetUniqueKey(string username)
    {
        using var context = new PgSqlContext();
        return context.Users.FirstOrDefault(x => x.Username == username)?.Id ?? 0;
    }
}