using System;

namespace Pamaxie.Data;

public enum ProjectPermissions
{
    /// <summary>
    /// No Permissions for this user (User is not part for this project or no permissions have been granted to them)
    /// </summary>
    None = 0,
    
    /// <summary>
    /// Read the projects current properties
    /// </summary>
    Read = 1,
    
    /// <summary>
    /// Write project properties
    /// </summary>
    Write = 2,
    
    /// <summary>
    /// Mange Users, read / write and do anything really in the Project
    /// </summary>
    Administrator = 128
}