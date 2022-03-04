using System;
using System.Security.Cryptography;
using IdGen;
using Pamaxie.Data;

namespace Pamaxie.Database.Native.NoSql;

/// <summary>
/// Stores Image Scan Data (everything that is an image more or less
/// </summary>
public class ScanData : IPamNoSqlObject
{
    public ScanData(string key, string scanResultKey)
    {
        Key = key;
        ScanResultKey = scanResultKey;
    }
    
    public string Key { get; set; }
    public string DataType { get; set; }
    public string DataExtension { get; set; }
    public string ScanMachineGuid { get; set; }
    public bool IsUserScan { get; set; }
    public string ScanResultKey { get; private set; }
    public DateTime? TTL { get; set; }
}