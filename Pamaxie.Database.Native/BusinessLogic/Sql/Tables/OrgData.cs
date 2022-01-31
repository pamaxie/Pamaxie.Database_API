using System;
using System.ComponentModel.DataAnnotations;
using Pamaxie.Data;

namespace Pamaxie.Database.Native.Sql;

/// <summary>
/// Stores Organizations data
/// </summary>
public class PamOrgData : IPamSqlObject
{
    /// <summary>
    /// <inheritdoc cref="IPamSqlObjects.Id"/>
    /// </summary>
    [Key]
    public int Id { get; set; }
    
    /// <summary>
    /// Organizations User friendly name
    /// </summary>
    public string OrgName { get; set; }

    /// <summary>
    /// User Id of the owner of this org
    /// </summary>
    public long OwnerId { get; set; }
    
    /// <summary>
    /// Domain name of this organization (please validate via flags if it is verified)
    /// </summary>
    public string DomainName { get; set; }
    
    /// <summary>
    /// Country this organization resides in
    /// </summary>
    public Country Country { get; set; }
    
    /// <summary>
    /// Flags set for this Organization
    /// </summary>
    public OrgFlags Flags { get; set; }
    
    /// <summary>
    /// When was this Organization created
    /// </summary>
    public DateTime CreationDate { get; set; }

    /// <summary>
    /// <inheritdoc cref="IPamSqlObject.TTL"/>
    /// </summary>
    public DateTime? TTL { get; set; }
}