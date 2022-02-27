using System;
using System.Diagnostics;
using System.Net.Http.Json;
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

    public IPamNoSqlObject Get(string uniqueKey)
    {
        if (string.IsNullOrWhiteSpace(uniqueKey))
        {
            throw new ArgumentNullException(nameof(uniqueKey));
        }

        var db = CheckAndGetDb();
        var noSqlObj = db.StringGet(uniqueKey);

        return string.IsNullOrWhiteSpace(noSqlObj) ? null : JsonConvert.DeserializeObject<IPamNoSqlObject>(noSqlObj);
    }

    public bool Create(IPamNoSqlObject data)
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
            return db.StringSet(data.Key, jsonData);
        }

        var timeDiff = data.TTL - DateTime.Now;
        return db.StringSet(data.Key, jsonData, timeDiff);
    }

    public bool Update(IPamNoSqlObject data)
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

        if (!db.KeyExists(data.Key))
        {
            return false;
        }

        var jsonData = JsonConvert.SerializeObject(data);

        if (data.TTL == null)
        {
            return db.StringSet(data.Key, jsonData);
        }

        var timeDiff = data.TTL - DateTime.Now;
        return db.StringSet(data.Key, jsonData, timeDiff);
    }

    public bool UpdateOrCreate(IPamNoSqlObject data)
    {
        return Update(data) || Create(data);
    }

    public bool Exists(string uniqueKey)
    {
        if (string.IsNullOrEmpty(uniqueKey))
        {
            throw new ArgumentException("Please ensure that the reached in data contains a " +
                                        "key before trying to save it to the database", nameof(uniqueKey));
        }

        var db = CheckAndGetDb();

        return db.KeyExists(uniqueKey);
    }

    public bool Delete(IPamNoSqlObject data)
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
        return db.KeyDelete(data.Key);
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