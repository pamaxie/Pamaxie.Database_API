using System;
using Pamaxie.Data;

namespace Pamaxie.Database.Native.Sql;

/// <summary>
/// Stores Users data
/// </summary>
public class UserData : IPamSqlObject
{
    /// <summary>
    /// <inheritdoc cref="IPamSqlObject.Id"/>
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Username of the user
    /// </summary>
    public string Username { get; set; }
    
    /// <summary>
    /// Email of the user (please validate its verified before allowing a login)
    /// </summary>
    public string Email { get; set; }
    
    /// <summary>
    /// First name of the user
    /// </summary>
    public string FirstName { get; set; }

    /// <summary>
    /// Last name of the user
    /// </summary>
    public string LastName { get; set; }
    
    /// <summary>
    /// Hash of the users password
    /// </summary>
    public string PasswordHash { get; set; }
    
    /// <summary>
    /// FLags of the user (stores information like if email is verified or 2fa is active)
    /// </summary>
    public UserFlags Flags { get; set; }
    
    /// <summary>
    /// When was this users account created.
    /// </summary>
    public DateTime CreationDate { get; set; }
    
    /// <summary>
    /// <inheritdoc cref="TTL"/>
    /// </summary>
    public DateTime? TTL { get; set; }
}