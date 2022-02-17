using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IdGen;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic.CompilerServices;
using Pamaxie.Data;

namespace Pamaxie.Database.Native.Sql;

/// <summary>
/// Api keys that allow access to Pamaxie's Scanning API
/// </summary>
[Index(nameof(ProjectId), nameof(CredentialHash))]
public class ApiKey : IPamSqlObject
{
    internal static IdGenerator ApiKeyIdGenerator = new IdGenerator(4);
    private DateTime? _ttl;

    public ApiKey()
    {
        Id = ApiKeyIdGenerator.CreateId();
    }
    
    /// <inheritdoc cref="IPamSqlObject.Id"/>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public long Id { get; set; }

    /// <summary>
    /// Project that this API key is used with
    /// </summary>
    public long ProjectId { get; set; }
    
    /// <summary>
    /// Credential hash that is used to authenticated with the API
    /// </summary>
    public string CredentialHash { get; set; }

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