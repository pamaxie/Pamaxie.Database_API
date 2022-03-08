using System.Threading.Tasks;
using Pamaxie.Data;
using Pamaxie.Database.Extensions.DataInteraction;
using Pamaxie.Database.Native.DataInteraction.BusinessLogicExtensions;
using Pamaxie.Database.Native.NoSql;

namespace Pamaxie.Database.Native.DataInteraction;

public class PamScanInteractionBase : PamNoSqlInteractionBase, IPamScanInteraction
{
    public PamScanInteractionBase(PamaxieDatabaseService owner) : base(owner) { }
    
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
        if (data is PamScanData<ImageScanResult> scanData)
        {
            var scan = scanData.ToBusinessLogic(out var userDataScanResult);
            
            if (userDataScanResult is not ImageScanResult imageScanResult)
            {
                return await base.CreateAsync(data);
            }

            var creationResult = await base.UpdateAsync(data) && await base.UpdateAsync(imageScanResult);
            return (creationResult, scan.Key);
        }
        else
        {
            return await base.CreateAsync(data);
        }
        
    }

    public override async Task<bool> UpdateAsync(IPamNoSqlObject data)
    {
        if (data is PamScanData<ImageScanResult> scanData)
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
}