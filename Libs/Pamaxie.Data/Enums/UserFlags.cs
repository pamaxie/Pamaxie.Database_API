using System;

namespace Pamaxie.Data;

[Flags]
public enum UserFlags : long
{
    /// <summary>
    /// No user flags are set
    /// </summary>
    None = 0,

    /// <summary>
    /// User account is locked and cannot be accessed or used
    /// </summary>
    Locked = 1,
    
    /// <summary>
    /// Is the user account confirmed?
    /// </summary>
    ConfirmedAccount = 2,
    
    /// <summary>
    /// Is the user account secured via 2Fa
    /// </summary>
    //TwoFactorSecured = 4,
    
    /// <summary>
    /// Type of user
    /// </summary>
    PamaxieStaff = 8,
    
    /// <summary>
    /// Does the user have access to closed access features
    /// </summary>
    HasClosedAccess = 16
}