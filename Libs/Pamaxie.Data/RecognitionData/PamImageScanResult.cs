using System;

namespace Pamaxie.Data;

public class PamImageScanResult : IPamNoSqlObject
{
    public string Key { get; set; }
    public string DetectedShieldLabel { get; set; }
    public float DetectedShieldLabelLikelihood { get; set; }
    public string DetectedShieldLabelAnnotation { get; set; }
    public float DetectedShieldLabelAnnotationLikelihood { get; set; }
    public string DetectedSymbolLabel { get; set; }
    public float DetectedSymbolLikelihood { get; set; }
    public PamShieldScanResult AllShieldData {get; set;}
    public PamShieldAnnotationScanResult AllShieldAnnotationData { get; set; }
    public PamSymbolNetResult AllSymbolData { get; set; }
    public DateTime? TTL { get; set; }
}