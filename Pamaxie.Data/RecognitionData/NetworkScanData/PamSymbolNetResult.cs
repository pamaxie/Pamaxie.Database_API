using System;

namespace Pamaxie.Data;

public sealed class PamSymbolNetResult
{
    public float HitlerLikelihood { get; set; }
    public float SwastikaLikelihood { get; set; }
    public float NoneLikelihood { get; set; }
    public string DetectedLabel { get; set; }
    public int NetworkVersion { get; set; }
    public string VersionCommonName { get; set; }
    public int ScanPPi { get; set; }
    public bool WasCompressed { get; set; }
    public DateTime ScanDate { get; set; }
}