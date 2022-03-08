using System;
using System.Collections.Generic;

namespace Pamaxie.Data;

/// <summary>
/// Return data for the API for Projects
/// </summary>
public interface IPamProject : IPamSqlObject
{
    /// <summary>
    /// Friendly Name for the <see cref="IPamProject"/>
    /// </summary>
    public string Name { get; set; }
    
    /// <summary>
    /// Owner of this Project
    /// </summary>
    public LazyObject<(PamUser User, long UserId)> Owner { get; set; }
    
    /// <summary>
    /// Last user who modified this Project
    /// </summary>
    public LazyObject<(PamUser User, long UserId)> LastModifiedUser { get; set; }

    /// <summary>
    /// API / Access Token for the <see cref="IPamProject"/>
    /// </summary>
    public LazyList<(string Token, long TokenId)> ApiTokens { get; set; }

    /// <summary>
    /// <see cref="IPamUser"/> with their <see cref="ProjectPermissions"/> for this project
    /// </summary>
    public LazyList<(long UserId, ProjectPermissions Permission)> Users { get; set; }
    
    /// <summary>
    /// Flags that set certain properties for the <see cref="IPamProject"/>
    /// </summary>
    public ProjectFlags Flags { get; set; }
    
    /// <summary>
    /// When this Project was created
    /// </summary>
    public DateTime CreationDate { get; set; }
    
    /// <summary>
    /// When this Project was last modified
    /// </summary>
    public DateTime LastModifiedAt { get; set; }
}