using System;

namespace Pamaxie.Data;

public class PamShieldAnnotationScanResult
{
    public float VirtualModelLikelihood { get; set; }
    public float DrawingLikelihood { get; set; }
    public float AnimationLikelihood { get; set; }
    public float RealLikelihood { get; set; }
    public string DetectedLabel { get; set; }
    public int NetworkVersion { get; set; }
    public string VersionCommonName { get; set; }
    public int ScanPPi { get; set; }
    public bool WasCompressed { get; set; }
    public DateTime ScanDate { get; set; }
}