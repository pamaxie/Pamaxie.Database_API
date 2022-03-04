using System;
using Pamaxie.Data;
using Pamaxie.Database.Extensions.DataInteraction;
using Pamaxie.Database.Native.NoSql;

namespace Pamaxie.Database.Native.DataInteraction.BusinessLogicExtensions;

public static class ScanResultExtensions
{
    public static ScanData ToBusinessLogic<T> (this PamScanData<T> userData, out object userDataScanResult) where T : IPamNoSqlObject
    {
        if (userData.ScanResult is PamImageScanResult imageScanResult)
        {
            var item = imageScanResult.ToBusinessLogic(userData.Key);
            userDataScanResult = item;

            var scanData = new ScanData(userData.Key, item.Key)
            {
                DataType = userData.DataType,
                DataExtension = userData.DataExtension,
                IsUserScan = userData.IsUserScan,
                ScanMachineGuid = userData.ScanMachineGuid,
                TTL = userData.TTL
            };

            return scanData;
        }

        userDataScanResult = default(T);
        return null;
    }

    public static PamScanData<ImageScanResult> ToImageScanUserLogic (this ScanData businessData, ImageScanResult scanResult)
    {
        return new PamScanData<ImageScanResult>()
        {
            ScanResult = scanResult,
            Key = businessData.Key,
            DataType = businessData.DataType,
            DataExtension = businessData.DataExtension,
            IsUserScan = businessData.IsUserScan,
            ScanMachineGuid = businessData.ScanMachineGuid,
            TTL = businessData.TTL
        };
    }
}