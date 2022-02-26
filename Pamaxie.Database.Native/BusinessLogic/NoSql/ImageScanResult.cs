using System;
using System.Security.Cryptography;
using IdGen;
using Pamaxie.Data;

namespace Pamaxie.Database.Native.NoSql;

/// <summary>
/// Stores Image Scan Data (everything that is an image more or less
/// </summary>
public class ImageScanResult : IPamNoSqlObject
{
    public static RandomNumberGenerator RngGenerator =  System.Security.Cryptography.RandomNumberGenerator.Create();
    
    public ImageScanResult(string ownerKey)
    {
        byte[] tokenBuffer = new byte[64];
        RngGenerator.GetNonZeroBytes(tokenBuffer);
        Key = Convert.ToBase64String(tokenBuffer);
        OwnerKey = ownerKey;
    }
    
    public string OwnerKey { get; set; }
    public string Key { get; set; }
    public string DetectedShieldLabel { get; set; }
    public float DetectedShieldLabelLikelihood { get; set; }
    public string DetectedShieldLabelAnnotation { get; set; }
    public string DetectedShieldLabelAnnotationLikelihood { get; set; }
    public string DetectedSymbolLabel { get; set; }
    public string DetectedSymbolLikelihood { get; set; }
    public PamShieldScanResult AllShieldData { get; set; }
    public PamShieldAnnotationScanResult AllShieldAnnotationData { get; set; }
    public PamSymbolNetResult AllSymbolData { get; set; }
    public DateTime? TTL { get; set; }
}