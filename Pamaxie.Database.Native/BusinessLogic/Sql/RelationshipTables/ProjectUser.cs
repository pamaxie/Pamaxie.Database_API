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
public class ProjectUser : IPamSqlObject
{
    private static IdGenerator ProjectIdGenerator = new IdGenerator(4);
    private DateTime? _ttl;

    public ProjectUser()
    {
        Id = ProjectIdGenerator.CreateId();
    }
    
    /// <inheritdoc cref="IPamSqlObject.Id"/>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public long Id { get; set; }
    
    /// <summary>
    /// User who this Project is referencing
    /// </summary>
    public User User { get; set; }

    /// <summary>
    /// Id of the project this user is referencing
    /// </summary>
    public Project Project { get; set; }
    
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