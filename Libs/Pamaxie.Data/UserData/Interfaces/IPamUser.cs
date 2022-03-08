using System;
using System.Collections.Generic;

namespace Pamaxie.Data;

/// <summary>
/// Return data for the API for Users
/// </summary>
public interface IPamUser : IPamSqlObject
{
    /// <summary>
    /// Email the user users to get recovery emails and that was used for signup
    /// </summary>
    public string Email { get; set; }
    
    /// <summary>
    /// username the user uses to log into our service
    /// </summary>
    public string UserName { get; set; }
    
    /// <summary>
    /// Last name of the user
    /// </summary>
    public string LastName { get; set; }
    
    /// <summary>
    /// First name of the user
    /// </summary>
    public string FirstName { get; set; }
    
    /// <summary>
    /// Hash of the Password that the <see cref="IPamUser"/> uses
    /// </summary>
    public string PasswordHash { get; set; }

    /// <summary>
    /// Secret for two Factor Authentication. If not set and <see cref="UserFlags.TwoFactorSecured"/> is set
    /// it means that 2Fa is done via e-mail.
    /// </summary>
    //public LazyList<(TwoFactorType Type, long TwoFactorId)> TwoFactorOptions { get; set; }

    /// <summary>
    /// List of known Addresses the <see cref="IPamUser"/> connected from
    /// </summary>
    public LazyList<string> KnownIps { get; set; }

    /// <summary>
    /// Projects that the <see cref="IPamUser"/> owns
    /// </summary>
    public LazyList<(IPamProject Project, long ProjectId)> Projects { get; set; }

    /// <summary>
    /// When the user was created
    /// </summary>
    public DateTime CreationDate { get; set; }
    
    /// <summary>
    /// Flags that set certain properties for the <see cref="IPamUser"/>
    /// </summary>
    public UserFlags Flags { get; set; }
}