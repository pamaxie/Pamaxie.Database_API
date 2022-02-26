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
    public static RandomNumberGenerator RngGenerator =  System.Security.Cryptography.RandomNumberGenerator.Create();
    
    public ScanData(string scanResultKey)
    {
        byte[] tokenBuffer = new byte[64];
        RngGenerator.GetNonZeroBytes(tokenBuffer);
        Key = Convert.ToBase64String(tokenBuffer);
        ScanResultKey = Key;
    }
    
    public string Key { get; set; }
    public string DataType { get; set; }
    public string DataExtension { get; set; }
    public string ScanMachineGuid { get; set; }
    public bool IsUserScan { get; set; }
    public string ScanResultKey { get; set; }
    public DateTime? TTL { get; set; }
}