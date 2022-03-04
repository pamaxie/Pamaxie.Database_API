using System.ComponentModel.DataAnnotations;

namespace Pamaxie.Database.Native.Sql;

public class EmailConfirmation
{
    /// <summary>
    /// Users Id
    /// </summary>
    [Key]
    public long UserId { get; set; }
    
    /// <summary>
    /// ConfirmationCode
    /// </summary>
    public string ConfirmationCode { get; set; }
}