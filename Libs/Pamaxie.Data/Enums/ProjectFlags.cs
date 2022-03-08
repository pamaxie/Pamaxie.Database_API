using System;

namespace Pamaxie.Data;

[Flags]
public enum ProjectFlags : long
{
    /// <summary>
    /// No flags are set
    /// </summary>
    None = 0,
    
    /// <summary>
    /// Project is locked from being used / accessed or modified
    /// </summary>
    Locked = 1,
    
    /// <summary>
    /// Project is shared with multiple users
    /// </summary>
    Shared = 2
}