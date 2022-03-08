using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IdGen;
using Microsoft.EntityFrameworkCore;
using Pamaxie.Data;

namespace Pamaxie.Database.Native.Sql;

/// <summary>
/// Stores relationships between a User and a Project including their Permissions for the project
/// </summary>
[Index(nameof(UserId), nameof(ProjectId))]
public class ProjectUser : IPamSqlObject
{
    internal static IdGenerator ProjectIdGenerator = new IdGenerator(4);
    private DateTime? _ttl;

    internal ProjectUser() : this(0, 0) { }

    public ProjectUser(long userId, long projectId)
    {
        Id = ProjectIdGenerator.CreateId();
        UserId = userId;
        ProjectId = projectId;
    }
    
    /// <inheritdoc cref="IPamSqlObject.Id"/>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public long Id { get; set; }
    
    /// <summary>
    /// User who this Project is referencing
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    /// Id of the project this user is referencing
    /// </summary>
    public long ProjectId { get; set; }
    
    /// <summary>
    /// Permissions for the user
    /// </summary>
    public ProjectPermissions Permissions { get; set; }

    /// <inheritdoc cref="IPamSqlObject.TTL"/>
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