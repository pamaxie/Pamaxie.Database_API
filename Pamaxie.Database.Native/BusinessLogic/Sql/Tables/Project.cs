using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IdGen;
using Microsoft.EntityFrameworkCore;
using Pamaxie.Data;

namespace Pamaxie.Database.Native.Sql;

/// <summary>
/// Stores Projects Data
/// </summary>
[Index(nameof(TTL))]
public class Project : IPamSqlObject
{
    private static IdGenerator ProjectIdGenerator = new IdGenerator(2);
    private DateTime _creationDate;
    private DateTime _lastModified;
    private DateTime? _ttl;

    public Project()
    {
        Id = ProjectIdGenerator.CreateId();
    }
        
    /// <summary>
    /// <inheritdoc cref="IPamSqlObject.Id"/>
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public long Id { get; set; }
    
    /// <summary>
    /// Name of the project
    /// </summary>
    public string Name { get; set; }
    
    /// <summary>
    /// Id of the Owner of this project
    /// </summary>
    public ulong OwnerId { get; set; }
    
    /// <summary>
    /// Flags of this Project
    /// </summary>
    public ProjectFlags Flags { get; set; }

    /// <summary>
    /// When this project was created
    /// </summary>
    public DateTime CreationDate
    {
        get => _creationDate;
        set => _creationDate = value.ToUniversalTime();
    }

    /// <summary>
    /// When this Project was last edited / modified
    /// </summary>
    public DateTime LastModified
    {
        get => _lastModified;
        set => _lastModified = value.ToUniversalTime();
    }

    /// <summary>
    /// Unique id of the user who edited this project
    /// </summary>
    public ulong LastModifiedUserId { get; set; }

    /// <summary>
    /// <inheritdoc cref="IPamSqlObject.TTL"/>
    /// </summary>
    public DateTime? TTL
    {
        get => _ttl;
        set
        {
            if (value.HasValue)
            {
                _ttl = value.Value.ToUniversalTime();
                return;
            }
            
            _ttl = null;
        }
    }

    /// <summary>
    /// Users that are part of this project
    /// </summary>
    public List<ProjectUser> Users { get; set; }
    
    /// <summary>
    /// Api keys for this project
    /// </summary>
    public List<ApiKey> ApiKeys { get; set; }
}