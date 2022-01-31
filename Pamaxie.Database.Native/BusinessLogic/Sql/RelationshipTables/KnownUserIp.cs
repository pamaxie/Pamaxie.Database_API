using System.ComponentModel.DataAnnotations;

namespace Pamaxie.Database.Native.Sql;

/// <summary>
/// Stores known Ips users connected from
/// </summary>
public class KnownUserIp
{
    /// <summary>
    /// User Id who logged in with this IP previously
    /// </summary>
    public long UserId { get; set; }
    
    /// <summary>
    /// IP address of new login
    /// </summary>
    public string IpAddress { get; set; }
}