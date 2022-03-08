using System;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using Pamaxie.Data;
using Pamaxie.Database.Extensions.DataInteraction;
using Pamaxie.Database.Native.NoSql;
using StackExchange.Redis;

namespace Pamaxie.Database.Native.DataInteraction;

public class PamNoSqlInteractionBase : IPamInteractionBase<IPamNoSqlObject, string>
{
    private ConnectionMultiplexer RedisMultiplexer;

    protected PamNoSqlInteractionBase(PamaxieDatabaseService owner)
    {
        if (owner.DbConnectionHost1 is not ConnectionMultiplexer multiplexer)
        {
            throw new InvalidOperationException(
                "This API requires a connection multiplexer to be on the Primary DbConnectionHost");
        }

        RedisMultiplexer = multiplexer;
    }

    public virtual async Task<IPamNoSqlObject> GetAsync(string uniqueKey)
    {
        if (string.IsNullOrWhiteSpace(uniqueKey))
        {
            throw new ArgumentNullException(nameof(uniqueKey));
        }

        var db = CheckAndGetDb();
        var noSqlObj = await db.StringGetAsync(uniqueKey);
        if (!noSqlObj.HasValue)
        {
            return null;
        }
        
        //Update the TTL of the object to be half a year.
        await db.StringSetAsync(uniqueKey, noSqlObj.ToString(), DateTime.Now.AddMonths(6) - DateTime.Now);

        if (TryParseJson(noSqlObj, out ScanData scanData))
        {
            return scanData;
        }
        
        if(TryParseJson(noSqlObj, out ImageScanResult imageScanResult))
        {
            return imageScanResult;
        }
        
        if(TryParseJson(noSqlObj, out ScanMachine scanMachine))
        {
            return scanMachine;
        }
        
        
        var item = JsonConvert.DeserializeObject<PamNoSqlObject>(noSqlObj);
        return item;
    }

    public virtual async Task<(bool, string)> CreateAsync(IPamNoSqlObject data)
    {
        return (await this.UpdateAsync(data), data.Key);
    }

    public virtual async Task<bool> UpdateAsync(IPamNoSqlObject data)
    {
        if (data == null)
        {
            throw new ArgumentNullException(nameof(data));
        }

        if (string.IsNullOrEmpty(data.Key))
        {
            throw new ArgumentException("Please ensure that the reached in data contains a " +
                                        "key before trying to save it to the database", nameof(data));
        }

        if (data.TTL != null && data.TTL < DateTime.Now)
        {
            throw new ArgumentException("The reached in TTL is in the past. " +
                                        "Please ensure its in the future.", nameof(data.TTL));
        }

        var db = CheckAndGetDb();

        data.TTL = DateTime.Now.AddMonths(6);
        var jsonData = JsonConvert.SerializeObject(data);
        var timeDiff = data.TTL - DateTime.Now;
        
        return await db.StringSetAsync(data.Key, jsonData, timeDiff);
    }

    public async Task<bool> ExistsAsync(string uniqueKey)
    {
        if (string.IsNullOrEmpty(uniqueKey))
        {
            throw new ArgumentException("Please ensure that the reached in data contains a " +
                                        "key before trying to save it to the database", nameof(uniqueKey));
        }

        var db = CheckAndGetDb();
        var exists = await db.KeyExistsAsync(uniqueKey);
        return exists;
    }

    public async Task<bool> DeleteAsync(IPamNoSqlObject data)
    {
        if (data == null)
        {
            throw new ArgumentNullException(nameof(data));
        }

        if (string.IsNullOrEmpty(data.Key))
        {
            throw new ArgumentException("Please ensure that the reached in data contains a " +
                                        "key before trying to save it to the database", nameof(data));
        }

        var db = CheckAndGetDb();
        return await db.KeyDeleteAsync(data.Key);
    }

    private static bool TryParseJson<T>(string unparsedJson, out T result)
    {
        bool success = true;
        var settings = new JsonSerializerSettings
        {
            Error = (sender, args) => { success = false; args.ErrorContext.Handled = true; },
            MissingMemberHandling = MissingMemberHandling.Error
        };
        result = JsonConvert.DeserializeObject<T>(unparsedJson, settings);
        return success;
    }
    
    private IDatabase CheckAndGetDb()
    {
        if (!RedisMultiplexer.IsConnected)
        {
            throw new RedisException("Unable to connect to the database. " +
                                     "Please ensure its connected before trying to interact with it.");
        }

        return RedisMultiplexer.GetDatabase();
    }
}