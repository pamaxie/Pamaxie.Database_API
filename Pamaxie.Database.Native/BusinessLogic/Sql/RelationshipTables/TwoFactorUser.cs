using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IdGen;
using Microsoft.EntityFrameworkCore;
using Pamaxie.Data;

namespace Pamaxie.Database.Native.Sql;

/// <summary>
/// Stores data for the users who have two factor authentication enabled
/// </summary>
public class TwoFactorUser : IPamSqlObject
{
    internal static IdGenerator TwoFactorIdGenerator = new IdGenerator(3);

    public TwoFactorUser()
    {
        Id = (ulong)TwoFactorIdGenerator.CreateId();
    }
    
    /// <inheritdoc cref="IPamSqlObject.Id"/>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public ulong Id { get; set; }
    
    /// <summary>
    /// User where 2fa is active
    /// </summary>
    public User User { get; set; }
    
    /// <summary>
    /// Two factor type
    /// </summary>
    public TwoFactorType Type { get; set; }
    
    /// <summary>
    /// Public key of the two factor authentication
    /// </summary>
    public string PublicKey { get; set; }
    
    /// <inheritdoc cref="IPamSqlObject.TTL"/>
    public DateTime? TTL { get; set; }
}