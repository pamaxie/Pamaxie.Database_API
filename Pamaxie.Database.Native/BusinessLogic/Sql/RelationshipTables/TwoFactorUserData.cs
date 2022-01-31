using Pamaxie.Data;

namespace Pamaxie.Database.Native.Sql;

/// <summary>
/// Stores data for the users who have two factor authentication enabled
/// </summary>
public class TwoFactorUserData
{
    /// <summary>
    /// Id of the user where 2fa is active
    /// </summary>
    public long UserId { get; set; }
    
    /// <summary>
    /// Two factor type
    /// </summary>
    public TwoFactorType Type { get; set; }
    
    /// <summary>
    /// Public key of the two factor authentication
    /// </summary>
    public string PublicKey { get; set; }
}