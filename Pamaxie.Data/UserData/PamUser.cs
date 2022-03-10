using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Pamaxie.Data;

/// <inheritdoc cref="IPamUser"/>
public class PamUser : IPamUser
{
    /// <inheritdoc cref="IPamSqlObject.Id"/>
    public long Id { get; set; }
    
    /// <inheritdoc cref="IPamUser.Email"/>
    public string Email { get; set; }
    
    /// <inheritdoc cref="IPamUser.UserName"/>
    public string UserName { get; set; }
    
    /// <inheritdoc cref="IPamUser.LastName"/>
    public string LastName { get; set; }
    
    /// <inheritdoc cref="IPamUser.FirstName"/>
    public string FirstName { get; set; }
    
    /// <inheritdoc cref="IPamUser.PasswordHash"/>
    public string PasswordHash { get; set; }
    
    /// <inheritdoc cref="IPamUser.TwoFactorOptions"/>
    //public LazyList<(TwoFactorType Type, long TwoFactorId)> TwoFactorOptions { get; set; }

    /// <inheritdoc cref="IPamUser.KnownIps"/>
    public LazyList<string> KnownIps { get; set; }

    /// <inheritdoc cref="IPamUser.Projects"/>
    public LazyList<(IPamProject Project, long ProjectId)> Projects { get; set; }

    /// <inheritdoc cref="IPamUser.Flags"/>
    public UserFlags Flags { get; set; }
    
    /// <inheritdoc cref="IPamUser.CreationDate"/>
    public DateTime CreationDate { get; set; }

    /// <inheritdoc cref="TTL"/>
    public DateTime? TTL { get; set; }
}