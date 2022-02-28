using System;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Pamaxie.Data;
using Pamaxie.Database.Extensions.DataInteraction;
using StackExchange.Redis;

namespace Pamaxie.Database.Native.DataInteraction;

public class PamNoSqlInteractionBase : IPamInteractionBase<IPamNoSqlObject, string>
{
    private ConnectionMultiplexer RedisMultiplexer;

    public PamNoSqlInteractionBase(PamaxieDatabaseService owner)
    {
        if (owner.DbConnectionHost1 is not ConnectionMultiplexer multiplexer)
        {
            throw new InvalidOperationException(
                "This API requires a connection multiplexer to be on the Primary DbConnectionHost");
        }

        RedisMultiplexer = multiplexer;
    }

    public async Task<IPamNoSqlObject> GetAsync(string uniqueKey)
    {
        if (string.IsNullOrWhiteSpace(uniqueKey))
        {
            throw new ArgumentNullException(nameof(uniqueKey));
        }

        var db = CheckAndGetDb();
        var noSqlObj = await db.StringGetAsync(uniqueKey);

        return string.IsNullOrWhiteSpace(noSqlObj) ? null : JsonConvert.DeserializeObject<IPamNoSqlObject>(noSqlObj);
    }

    public async Task<bool> CreateAsync(IPamNoSqlObject data)
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
        var jsonData = JsonConvert.SerializeObject(data);

        if (data.TTL == null)
        {
            return await db.StringSetAsync(data.Key, jsonData);
        }

        var timeDiff = data.TTL - DateTime.Now;
        return await db.StringSetAsync(data.Key, jsonData, timeDiff);
    }

    public async Task<bool> UpdateAsync(IPamNoSqlObject data)
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

        if (!await db.KeyExistsAsync(data.Key))
        {
            return false;
        }

        var jsonData = JsonConvert.SerializeObject(data);

        if (data.TTL == null)
        {
            return await db.StringSetAsync(data.Key, jsonData);
        }

        var timeDiff = data.TTL - DateTime.Now;
        return await db.StringSetAsync(data.Key, jsonData, timeDiff);
    }

    public async Task<bool> UpdateOrCreateAsync(IPamNoSqlObject data)
    {
        return await UpdateAsync(data) || await CreateAsync(data);
    }

    public async Task<bool> ExistsAsync(string uniqueKey)
    {
        if (string.IsNullOrEmpty(uniqueKey))
        {
            throw new ArgumentException("Please ensure that the reached in data contains a " +
                                        "key before trying to save it to the database", nameof(uniqueKey));
        }

        var db = CheckAndGetDb();

        return await db.KeyExistsAsync(uniqueKey);
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