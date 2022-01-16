using System;
using System.Collections.Generic;
using Pamaxie.Data;

namespace Pamaxie.Database.Redis;

/// <summary>
/// Defines how <see cref="IPamUser"/> data is stored in the db
/// </summary>
internal sealed class PamUserData : IPamDbObject
{
    /// <summary>
    /// <inheritdoc cref="IPamDbObject.Uid"/>
    /// </summary>
    public string Uid { get; set; }

    /// <summary>
    /// Email of the user
    /// </summary>
    public string Email { get; set; }

    /// <summary>
    /// Username
    /// </summary>
    public string UserName { get; set; }
    
    /// <summary>
    /// Last Name
    /// </summary>
    public string LastName { get; set; }
    
    /// <summary>
    /// First Name
    /// </summary>
    public string FirstName { get; set; }
    
    /// <summary>
    /// Hash of the password
    /// </summary>
    public string PasswordHash { get; set; }
    
    /// <summary>
    /// Two factor options that are active
    /// </summary>
    public List<(TwoFactorType Type, string Secret)> TwoFactorOptions { get; set; }
    
    /// <summary>
    /// Home country of the user
    /// </summary>
    public Countries HomeCountry { get; set; }
    
    /// <summary>
    /// Known locations / IP addresses of the user
    /// </summary>
    public List<string> KnownIps { get; set; }
    
    /// <summary>
    /// Unique IDs of the Organizations
    /// </summary>
    public List<string> OrgGuid { get; set; }
    
    /// <summary>
    /// Organizations the user is a part of
    /// </summary>
    public List<IPamOrg> Orgs { get; set; }
    
    /// <summary>
    /// Guids of the Projects
    /// </summary>
    public List<string> ProjectGuids { get; set; }
    
    /// <summary>
    /// Projects the user owns
    /// </summary>
    public List<IPamProject> Projects { get; set; }
    
    /// <summary>
    /// Flags that are currently active for the user
    /// </summary>
    public IPamUser.UserAttributes Flags { get; set; }
    
    /// <summary>
    /// Which Objects have been loaded already
    /// </summary>
    public LoadedObjectsEnum LoadedObjects { get; set; }
    
    /// <summary>
    /// <inheritdoc cref="IPamDbObject.TTL"/>
    /// </summary>
    public DateTime? TTL { get; set; }
    
    /// <summary>
    /// Flags Enum that carries which lists are currently loaded
    /// </summary>
    [Flags]
    public enum LoadedObjectsEnum
    {
        /// <summary>
        /// No items are currently loaded
        /// </summary>
        None = 0,
        
        /// <summary>
        /// <see cref="PamUserData.Orgs"/> is loaded
        /// </summary>
        Orgs = 1,
        
        /// <summary>
        /// <see cref="PamUserData.Projects"/> is loaded
        /// </summary>
        Projects = 2
    }


}