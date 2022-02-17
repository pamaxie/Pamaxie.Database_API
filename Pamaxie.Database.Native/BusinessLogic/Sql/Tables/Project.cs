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
[Index(nameof(TTL), nameof(OwnerId), nameof(Name))]
public class Project : IPamSqlObject
{
    internal static IdGenerator ProjectIdGenerator = new IdGenerator(2);
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
    [Required]
    public string Name { get; set; }
    
    /// <summary>
    /// Url where the picture of a project resides
    /// </summary>
    public string ProjectPicture { get; set; }
    
    /// <summary>
    /// Id of the Owner of this project
    /// </summary>
    [Required]
    public long OwnerId { get; set; }
    
    /// <summary>
    /// Flags of this Project
    /// </summary>
    [Required]
    public ProjectFlags Flags { get; set; }

    /// <summary>
    /// When this project was created
    /// </summary>
    [Required]
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
    public long LastModifiedUserId { get; set; }
 
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
}