using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using IdGen;
using Microsoft.EntityFrameworkCore;
using Pamaxie.Data;

namespace Pamaxie.Database.Native.Sql;

/// <summary>
/// Stores Users data
/// </summary>
[Index(nameof(TTL), nameof(Username), nameof(Email))]
public class User : IPamSqlObject
{
    internal static IdGenerator UserIdGenerator = new IdGenerator(1);
    private DateTime? _ttl;
    private DateTime _creationDate;
    
    public User() : this(DateTime.Now) { }

    public User(DateTime creationDate)
    {
        CreationDate = creationDate;
        Id = UserIdGenerator.CreateId();
        
        //Temporary I'm a bit lazy rn to fix this. Won't go like this public.
        ProfilePictureUrl = "null";
    }

    /// <summary>
    /// <inheritdoc cref="IPamSqlObject.Id"/>
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public long Id { get; set; }

    /// <summary>
    /// Username of the user
    /// </summary>
    [Required]
    public string Username { get; set; }
    
    /// <summary>
    /// Url where the profile picture of the user resides
    /// </summary>
    public string ProfilePictureUrl { get; set; }

    /// <summary>
    /// Email of the user (please validate its verified before allowing a login)
    /// </summary>
    [Required]
    [DataType(DataType.EmailAddress)]
    public string Email { get; set; }

    /// <summary>
    /// First name of the user
    /// </summary>
    [Required]
    public string FirstName { get; set; }

    /// <summary>
    /// Last name of the user
    /// </summary>
    [Required]
    public string LastName { get; set; }

    /// <summary>
    /// Hash of the users password
    /// </summary>
    [Required]
    [DataType(DataType.Password)]
    public string PasswordHash { get; set; }

    /// <summary>
    /// FLags of the user (stores information like if email is verified or 2fa is active)
    /// </summary>
    [Required]
    public UserFlags Flags { get; set; }

    /// <summary>
    /// When was this users account created.
    /// </summary>
    [Required]
    public DateTime CreationDate
    {
        get => _creationDate;
        set => _creationDate = value.ToUniversalTime();
    }

    /// <summary>
    /// <inheritdoc cref="TTL"/>
    /// </summary>
    public DateTime? TTL
    {
        get => _ttl;
        set
        {
            if (value.HasValue)
            {
                _ttl = value.Value.ToUniversalTime();
                return;
            }

            _ttl = null;
        } 
    }
}