using System;
using Pamaxie.Data;

namespace Pamaxie.Database.Redis;

/// <summary>
/// Defines how <see cref="IPamScanResult"/> are stored in the db
/// </summary>
public class PamScanData : IPamDbObject
{
    /// <summary>
    /// <see cref="IPamDbObject.Uid"/>
    /// </summary>
    public string Uid { get; set; }
    
    /// <summary>
    /// Was this item scanned by our system or an external worker node
    /// </summary>
    public bool SystemScan { get; set; }
    
    /// <summary>
    /// Guid of the person who scanned this
    /// </summary>
    public string ScannerGuid { get; set; }
    
    /// <summary>
    /// When was this item scanned
    /// </summary>
    public DateTime ScanTime { get; set; }
    
    /// <summary>
    /// Image Data that has been recognized
    /// </summary>
    public IPamImageData ImageData { get; set; }
    
    /// <summary>
    /// Content type of the recognition (image, file, text, ect)
    /// this is mostly for later use (will be left empty for now)
    /// </summary>
    public string ContentType { get; set; }

    /// <summary>
    /// <inheritdoc cref="IPamDbObject.TTL"/>
    /// </summary>
    public DateTime? TTL { get; set; }
}