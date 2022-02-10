using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IdGen;
using Microsoft.EntityFrameworkCore;
using Pamaxie.Data;

namespace Pamaxie.Database.Native.Sql;

/// <summary>
/// Api keys that allow access to Pamaxie's Scanning API
/// </summary>
public class ApiKey : IPamSqlObject
{
    private static IdGenerator ApiKeyIdGenerator = new IdGenerator(4);
    
    public ApiKey()
    {
        Id = (ulong) ApiKeyIdGenerator.CreateId();
    }
    
    /// <inheritdoc cref="IPamSqlObject.Id"/>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public ulong Id { get; set; }
    
    /// <summary>
    /// Project that this API key is used with
    /// </summary>
    public Project Project { get; set; }
    
    /// <summary>
    /// Credential hash that is used to authenticated with the API
    /// </summary>
    public string CredentialHash { get; set; }

    /// <inheritdoc cref="IPamSqlObject.TTL"/>
    public DateTime? TTL { get; set; }
}