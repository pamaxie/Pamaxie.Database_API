using Pamaxie.Data;

namespace Pamaxie.Database.Native.Sql;

/// <summary>
/// Api keys that allow access to Pamaxie's Scanning API
/// </summary>
public class ApiKey
{
    /// <summary>
    /// Owner who owns this Api key (see <see cref="ApiKeyType"/> for which one it is)
    /// </summary>
    public long OwnerId { get; set; }
    
    /// <summary>
    /// Credential hash that is used to authenticated with the API
    /// </summary>
    public string CredentialHash { get; set; }
    
    /// <summary>
    /// Type of API key (if its User, Org or Project Owned)
    /// </summary>
    public ApiKeyType ApiKeyType { get; set; }
}