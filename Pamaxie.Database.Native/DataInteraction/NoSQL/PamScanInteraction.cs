using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CoenM.ImageHash;
using Newtonsoft.Json;
using Pamaxie.Data;
using Pamaxie.Database.Extensions.DataInteraction;
using Pamaxie.Database.Native.DataInteraction.BusinessLogicExtensions;
using Pamaxie.Database.Native.NoSql;
using StackExchange.Redis;

namespace Pamaxie.Database.Native.DataInteraction;

public class PamScanInteraction : PamNoSqlInteractionBase, IPamScanInteraction
{

    private ConnectionMultiplexer RedisMultiplexer;
    private List<string> Databases;

    public PamScanInteraction(PamaxieDatabaseService owner) : base(owner) { 
        if (owner.DbConnectionHost1 is not ConnectionMultiplexer multiplexer)
        {
            throw new InvalidOperationException(
                "This API requires a connection multiplexer to be on the Primary DbConnectionHost");
        }

        RedisMultiplexer = multiplexer;

        //Splitting at the ; character then going to the first index to find the server names
        var splitConfig = RedisMultiplexer.Configuration.Split(';');
        if (splitConfig.Length > 0){
            var serverNames = splitConfig[0].Split(',');
            if (serverNames.Length > 0){
                Databases = new List<string>(serverNames);
            }
        }
    }
    
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

    public async Task<IPamNoSqlObject> GetWithHammingDistance(string key, double hammingDistance)
    {
        CancellationTokenSource source = new CancellationTokenSource();
        CancellationToken token = source.Token;
        (string ItemHash, double Similarity) bestOverallMatch = new (null, 0);

        await Parallel.ForEachAsync(Databases, async (database, token) =>
        {
            bool isImageHash = ulong.TryParse(key, out ulong imageKeyHash);
            var db = RedisMultiplexer.GetServer(database).KeysAsync();
            (string ItemHash, double Similarity) bestMatch = new (null, 0);

            await foreach(var item in db){
                bool isDbItemImageKey = ulong.TryParse(item, out ulong imageHashDb);

                //Search for best / closest image hash
                if(isImageHash){
                    //Ignore items that are not an image hash if the original key is one
                    if(!isDbItemImageKey){
                        continue;
                    }

                    double similarity = CompareHash.Similarity(imageKeyHash, imageHashDb);

                    if (similarity > bestMatch.Similarity){
                        bestMatch = new (item, similarity);
                    }
                }
            }

            bestOverallMatch = bestMatch;
        });

        if (bestOverallMatch.ItemHash == null || bestOverallMatch.Similarity < (hammingDistance * 100)){
            return null;
        }

        return await GetAsync(bestOverallMatch.ItemHash);
    }
}