using System;
using System.ComponentModel.DataAnnotations;
using Pamaxie.Data;

namespace Pamaxie.Database.Native.Sql;

/// <summary>
/// Stores Projects Data
/// </summary>
public class PamProjectData : IPamSqlObject
{
    /// <summary>
    /// <inheritdoc cref="IPamSqlObject.Id"/>
    /// </summary>
    [Key]
    public int Id { get; set; }
    
    /// <summary>
    /// Name of the project
    /// </summary>
    public string Name { get; set; }
    
    /// <summary>
    /// Id of the Owner of this project (see <see cref="Flags"/> for owner type)
    /// </summary>
    public long OwnerId { get; set; }
    
    /// <summary>
    /// Flags of this Project
    /// </summary>
    public ProjectFlags Flags { get; set; }
    
    /// <summary>
    /// When this project was created
    /// </summary>
    public DateTime CreationDate { get; set; }
    
    /// <summary>
    /// When this Project was last edited / modified
    /// </summary>
    public DateTime LastModified { get; set; }
    
    /// <summary>
    /// Unique id of the user who edited this project
    /// </summary>
    public long LastModifiedUserId { get; set; }
    
    /// <summary>
    /// <inheritdoc cref="IPamSqlObject.TTL"/>
    /// </summary>
    public DateTime? TTL { get; set; }
}