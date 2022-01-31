using System.ComponentModel.DataAnnotations;

namespace Pamaxie.Database.Native.Sql;

/// <summary>
/// Stores relationship between Users and Organizations
/// </summary>
public class OrgUserData
{
    /// <summary>
    /// Id of the user who is part of this organization
    /// </summary>
    public long UserId { get; set; }
    
    /// <summary>
    /// Id of the organization this user is part of
    /// </summary>
    public long OrgId { get; set; }
}