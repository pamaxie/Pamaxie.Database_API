using System;
using System.Security.Cryptography;
using Pamaxie.Data;
using Pamaxie.Database.Native.NoSql;

namespace Pamaxie.Database.Native.DataInteraction.BusinessLogicExtensions;

public static class ImageScanResultExtensions
{
    public static ImageScanResult ToBusinessLogic (this PamImageScanResult userData, string ownerKey)
    {
        if (string.IsNullOrEmpty(ownerKey))
        {
            throw new ArgumentNullException(nameof(ownerKey));
        }

        return new ImageScanResult(ownerKey)
        {
            DetectedShieldLabel = userData.DetectedShieldLabel,
            DetectedShieldLabelLikelihood = userData.DetectedShieldLabelLikelihood,
            DetectedShieldLabelAnnotation = userData.DetectedShieldLabelAnnotation,
            DetectedShieldLabelAnnotationLikelihood = userData.DetectedShieldLabelAnnotationLikelihood,
            DetectedSymbolLabel = userData.DetectedSymbolLabel,
            DetectedSymbolLikelihood = userData.DetectedSymbolLikelihood,
            AllShieldData = userData.AllShieldData,
            AllShieldAnnotationData = userData.AllShieldAnnotationData,
            AllSymbolData = userData.AllSymbolData,
            TTL = userData.TTL
        };
    }

    public static PamImageScanResult ToImageScanUserLogic (this ImageScanResult businessData)
    {
        return new PamImageScanResult()
        {
            Key = businessData.Key,
            DetectedShieldLabel = businessData.DetectedShieldLabel,
            DetectedShieldLabelLikelihood = businessData.DetectedShieldLabelLikelihood,
            DetectedShieldLabelAnnotation = businessData.DetectedShieldLabelAnnotation,
            DetectedShieldLabelAnnotationLikelihood = businessData.DetectedShieldLabelAnnotationLikelihood,
            DetectedSymbolLabel = businessData.DetectedSymbolLabel,
            DetectedSymbolLikelihood = businessData.DetectedSymbolLikelihood,
            AllShieldData = businessData.AllShieldData,
            AllShieldAnnotationData = businessData.AllShieldAnnotationData,
            AllSymbolData = businessData.AllSymbolData
        };
    }
}