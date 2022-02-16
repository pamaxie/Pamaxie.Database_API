using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net;
using IdGen;
using Microsoft.EntityFrameworkCore;
using Pamaxie.Data;

namespace Pamaxie.Database.Native.Sql;

/// <summary>
/// Stores known Ips users connected from
/// </summary>
public class KnownUserIp : IPamSqlObject
{
    private static IdGenerator KnownIpsIdsGenerator = new IdGenerator(4);
    private DateTime? _ttl;

    public KnownUserIp()
    {
        Id = KnownIpsIdsGenerator.CreateId();
    }
    
    /// <inheritdoc cref="IPamSqlObject.Id"/>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public long Id { get; set; }
    
    /// <summary>
    /// User who logged in with this IP previously
    /// </summary>
    public User User { get; set; }
    
    /// <summary>
    /// IP address of new login
    /// </summary>
    public string IpAddress { get; set; }

    /// <inheritdoc cref="IPamSqlObject.TTL"/>
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