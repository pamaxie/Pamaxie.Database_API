using System;
using System.Collections.Generic;

namespace Pamaxie.Data;

/// <inheritdoc cref="IPamProject"/>
public class PamProject : IPamProject
{
    /// <inheritdoc cref="IPamSqlObject.Id"/>
    public long Id { get; set; }

    /// <see cref="IPamProject.Name"/>
    public string Name { get; set; }
    
    /// <see cref="IPamProject.Owner"/>
    public LazyObject<(PamUser User, long UserId)> Owner { get; set; }

    /// <see cref="IPamProject.LastModifiedUser"/>
    public LazyObject<(PamUser User, long UserId)> LastModifiedUser { get; set; }

    /// <see cref="IPamProject.Users"/>
    public LazyList<(long UserId, ProjectPermissions Permission)> Users { get; set; }
    
    /// <see cref="IPamProject.ApiTokens"/>
    public LazyList<(string Token, long TokenId)> ApiTokens { get; set; }

    /// <see cref="IPamProject.Flags"/>
    public ProjectFlags Flags { get; set; }
    
    /// <see cref="IPamProject.CreationDate"/>
    public DateTime CreationDate { get; set; }
    
    /// <see cref="IPamProject.LastModified"/>
    public DateTime LastModifiedAt { get; set; }

    /// <inheritdoc cref="IPamSqlObject.TTL"/>
    public DateTime? TTL { get; set; }
}