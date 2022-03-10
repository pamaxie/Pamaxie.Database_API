using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using IdGen;
using Isopoh.Cryptography.Argon2;
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
    private static readonly IdGenerator ApiKeyIdGenerator = new IdGenerator(4);
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
    
    
    internal string CreateToken()
    {
        if (ProjectId == 0 || Id == 0)
        {
            throw new InvalidOperationException("Cannot create a token before the project and items Id are assigned");
        }
        
        var sha512Provider = SHA512.Create();
        var hash = sha512Provider.ComputeHash(RandomNumberGenerator.GetBytes(512));
        var tokenCredential =  Convert.ToBase64String(hash);
        var secretToken = $"PamToken/-//{ProjectId}/-//{Id}/-//{tokenCredential}";

        CredentialHash = Argon2.Hash(secretToken);
        return secretToken;
    }
}