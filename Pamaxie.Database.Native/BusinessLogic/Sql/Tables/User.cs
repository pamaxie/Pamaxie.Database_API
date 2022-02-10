using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using IdGen;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Pamaxie.Data;

namespace Pamaxie.Database.Native.Sql;

/// <summary>
/// Stores Users data
/// </summary>
[Index(nameof(TTL))]
public class User : IPamSqlObject
{
    private static IdGenerator UserIdGenerator = new IdGenerator(1);
    
    public User() : this(DateTime.Now) { }

    public User(DateTime creationDate)
    {
        CreationDate = creationDate;
        Id = (ulong) UserIdGenerator.CreateId();
    }

    [NotMapped]
    private DateTime? _ttl;

    /// <summary>
    /// <inheritdoc cref="IPamSqlObject.Id"/>
    /// </summary>
    [Key]
    public ulong Id { get; set; }

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
    [NotNull]
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
    public DateTime? TTL
    {
        get => _ttl;
        set
        {
            if (value == null)
            {
                _ttl = null;
                return;
            }

            _ttl = value.Value.ToUniversalTime();
        } 
    }

    /// <summary>
    /// IPs that are known for this user
    /// </summary>
    public List<KnownUserIp> KnownIps { get; set; }

    /// <summary>
    /// Projects that this user is part of
    /// </summary>
    public List<ProjectUser> Projects { get; set; }

    /// <summary>
    /// Two factor authentications for users
    /// </summary>
    public List<TwoFactorUser> TwoFactorAuths { get; set; }
}