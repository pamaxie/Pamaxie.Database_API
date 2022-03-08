using System.Threading.Tasks;
using Newtonsoft.Json;
using Pamaxie.Data;
using Pamaxie.Database.Extensions.DataInteraction;
using Pamaxie.Database.Native.DataInteraction.BusinessLogicExtensions;
using Pamaxie.Database.Native.NoSql;

namespace Pamaxie.Database.Native.DataInteraction;

public class PamScanInteraction : PamNoSqlInteractionBase, IPamScanInteraction
{
    public PamScanInteraction(PamaxieDatabaseService owner) : base(owner) { }
    
    public override async Task<IPamNoSqlObject> GetAsync(string uniqueKey)
    {
        var item =  await base.GetAsync(uniqueKey);

        if (item is not ScanData scanBase)
        {
            return item;
        }

        if (!string.IsNullOrEmpty(scanBase.ScanResultKey))
        {
            var scanResult = await base.GetAsync(scanBase.ScanResultKey);

            if (scanResult is ImageScanResult result)
            {
                return scanBase.ToImageScanUserLogic(result);
            }
        }
        

        //Unknown scan result so data isn't really interesting for us
        return item;
    }

    public override async Task<(bool, string)> CreateAsync(IPamNoSqlObject data)
    {
        if (data is PamScanData<PamImageScanResult> scanData)
        {
            var scan = scanData.ToBusinessLogic(out var userDataScanResult);
            
            if (userDataScanResult is not ImageScanResult imageScanResult)
            {
                return await base.CreateAsync(data);
            }

            var creationResult = await base.UpdateAsync(scan) && await base.UpdateAsync(imageScanResult);
            return (creationResult, scan.Key);
        }
        else
        {
            return (false, null);
        }
        
    }

    public override async Task<bool> UpdateAsync(IPamNoSqlObject data)
    {
        if (data is PamScanData<PamImageScanResult> scanData)
        {
            var scan = scanData.ToBusinessLogic(out var userDataScanResult);
            if (userDataScanResult is not ImageScanResult imageScanResult)
            {
                return await base.UpdateAsync(data);
            }

            return await base.UpdateAsync(data) && await base.UpdateAsync(imageScanResult);
        }
        else
        {
            return await base.UpdateAsync(data);
        }
    }

    public async Task<string> GetSerializedData(string key)
    {
        var item =  await base.GetAsync(key);

        if (item is not ScanData scanBase)
        {
            return JsonConvert.SerializeObject(item);
        }

        if (!string.IsNullOrEmpty(scanBase.ScanResultKey))
        {
            var scanResult = await base.GetAsync(scanBase.ScanResultKey);

            if (scanResult is ImageScanResult result)
            {
                var userData = scanBase.ToImageScanUserLogic(result);
                return JsonConvert.SerializeObject(userData);
            }
        }

        //Unknown scan result so data isn't really interesting for us
        return JsonConvert.SerializeObject(item);
    }

    public async Task<bool> DeleteAsync(string key)
    {
        var data = await base.GetAsync(key);
        if (data is ScanData scanResult)
        {
            var subDataDeletion = await base.DeleteAsync(new PamNoSqlObject() {Key = scanResult.ScanResultKey});

            if (!subDataDeletion)
            {
                return false;
            }

            return await base.DeleteAsync(new PamNoSqlObject() {Key = key});
        }
        return await base.DeleteAsync(new PamNoSqlObject() {Key = key});
    }
}