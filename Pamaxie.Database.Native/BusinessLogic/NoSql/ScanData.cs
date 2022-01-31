using System;
using Pamaxie.Data;

namespace Pamaxie.Database.Native.NoSql;

/// <summary>
/// Stores Image Scan Data (everything that is an image more or less
/// </summary>
public class ImageScanResult : IPamNoSqlObject
{
    public string Key { get; set; }
    public string DataType { get; set; }
    public string DataExtension { get; set; }
    public string ScanMachineGuid { get; set; }
    public bool UserScan { get; set; }
    public string ScanResultKey { get; set; }
    public DateTime ExpiryDate { get; set; }
    public int Id { get; set; }
    public DateTime? TTL { get; set; }
}