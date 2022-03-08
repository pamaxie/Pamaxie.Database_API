using System;

namespace Pamaxie.Data;

public class PamScanData<T> : IPamNoSqlObject
{
    public string Key { get; set; }
    public string DataType { get; set; }
    public string DataExtension { get; set; }
    public string ScanMachineGuid { get; set; }
    public bool IsUserScan { get; set; }
    public T ScanResult { get; set; }
    public DateTime? TTL { get; set; }
}