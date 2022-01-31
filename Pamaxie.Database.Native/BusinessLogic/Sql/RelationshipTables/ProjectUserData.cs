using Pamaxie.Data;

namespace Pamaxie.Database.Native.Sql;

/// <summary>
/// Stores relationships between an Organization or User with a project
/// </summary>
public class ProjectUserData
{
    /// <summary>
    /// Id of the user who this Project is referencing
    /// </summary>
    public long OwnerId { get; set; }
    
    /// <summary>
    /// Id of the project this user is referencing
    /// </summary>
    public long ProjectId { get; set; }
    
    /// <summary>
    /// Type of relationship this displays
    /// </summary>
    public ProjectReferenceType OwnerType { get; set; }
}