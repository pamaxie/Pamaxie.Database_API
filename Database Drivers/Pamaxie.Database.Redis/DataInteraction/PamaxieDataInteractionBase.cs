using Newtonsoft.Json;
using Pamaxie.Data;
using Pamaxie.Database.Design;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pamaxie.Database.Redis.DataInteraction
{
    internal class PamaxieDataInteractionBase<T>  : IPamaxieDataInteractionBase<T> where T : IDatabaseObject
    {
        /// <summary>
        /// Used for accessing the Redis database
        /// </summary>
        private PamaxieDatabaseDriver _owner;

        /// <summary>
        /// Called upon creating the database service
        /// </summary>
        /// <param name="config"></param>
        public PamaxieDataInteractionBase(PamaxieDatabaseDriver config)
        {
            _owner = config;
        }

        /// <inheritdoc cref="IPamaxieDataInteractionBase{T}.Get(string)"/>
        public T Get(string uniqueKey)
        {
            if (_owner == null)
            {
                throw new NullReferenceException($"The required property {nameof(_owner)} was null. Please ensure it is set before calling this class. This should usually never happen.");
            }

            if (_owner.Configuration == null || string.IsNullOrWhiteSpace(_owner.Configuration.ToString()))
            {
                throw new InvalidOperationException("This method cannot be called before the configuration has been initialized and properly configured. Please configure the database settings first.");
            }

            if (string.IsNullOrWhiteSpace(uniqueKey))
            {
                throw new ArgumentNullException(nameof(uniqueKey));
            }

            using var conn = ConnectionMultiplexer.Connect(_owner.Configuration.ToString());
            IDatabase db = conn.GetDatabase();
            RedisValue rawData = db.StringGet(uniqueKey);
            return string.IsNullOrWhiteSpace(rawData) ? default : JsonConvert.DeserializeObject<T>(rawData);
        }

        /// <inheritdoc cref="IPamaxieDataInteractionBase{T}.Create(T)"/>
        public T Create(T data)
        {
            if (_owner == null)
            {
                throw new NullReferenceException($"The required property {nameof(_owner)} was null. Please ensure it is set before calling this class. This should usually never happen.");
            }

            if (_owner.Configuration == null || string.IsNullOrWhiteSpace(_owner.Configuration.ToString()))
            {
                throw new InvalidOperationException("This method cannot be called before the configuration has been initialized and properly configured. Please configure the database settings first.");
            }

            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            using var conn = ConnectionMultiplexer.Connect(_owner.Configuration.ToString());
            IDatabase db = conn.GetDatabase();

            if (string.IsNullOrWhiteSpace(data.UniqueKey))
            {
                data.UniqueKey = Guid.NewGuid().ToString();
                Debug.Assert(!db.KeyExists(data.UniqueKey));
            }
            else if (db.KeyExists(data.UniqueKey))
            {
                throw new ArgumentException("The key you tried to create already exists inside of our database");
            }

            //Usually this shouldn't be done but we require this because of Redis being a non rational database
            if (data is IPamaxieUser user)
            {
                if (db.KeyExists(user.UserName))
                {
                    throw new ArgumentException("The user you tried to create already exists in our database");
                }

                db.StringSet(user.UserName, user.UniqueKey);
            }

            string parsedData = JsonConvert.SerializeObject(data);
            db.StringSet(data.UniqueKey, parsedData);



            return data;
        }

        /// <inheritdoc cref="IPamaxieDataInteractionBase{T}.TryCreate(T, out T)"/>
        public bool TryCreate(T data, out T createdItem)
        {
   
            if (_owner == null)
            {
                throw new NullReferenceException($"The required property {nameof(_owner)} was null. Please ensure it is set before calling this class. This should usually never happen.");
            }

            if (_owner.Configuration == null || string.IsNullOrWhiteSpace(_owner.Configuration.ToString()))
            {
                throw new InvalidOperationException("This method cannot be called before the configuration has been initialized and properly configured. Please configure the database settings first.");
            }

            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            using var conn = ConnectionMultiplexer.Connect(_owner.Configuration.ToString());
            IDatabase db = conn.GetDatabase();
            createdItem = default;

            if (string.IsNullOrWhiteSpace(data.UniqueKey))
            {
                data.UniqueKey = Guid.NewGuid().ToString();
                Debug.Assert(!db.KeyExists(data.UniqueKey));
            }
            else if (db.KeyExists(data.UniqueKey))
            {
                return false;
            }

            string parseData = JsonConvert.SerializeObject(data);

            //Usually this shouldn't be done but we require this because of Redis being a non rational database
            if (data is IPamaxieUser user)
            {
                if (db.KeyExists(user.UserName))
                {
                    return false;
                }

                db.StringSet(user.UserName, user.UniqueKey);
            }

            if (db.StringSet(data.UniqueKey, parseData))
            {
                createdItem = data;
            }

            return true;
        }

        /// <inheritdoc cref="IPamaxieDataInteractionBase{T}.Update(T)"/>
        public T Update(T data)
        {
            if (_owner == null)
            {
                throw new NullReferenceException($"The required property {nameof(_owner)} was null. Please ensure it is set before calling this class. This should usually never happen.");
            }

            if (_owner.Configuration == null || string.IsNullOrWhiteSpace(_owner.Configuration.ToString()))
            {
                throw new InvalidOperationException("This method cannot be called before the configuration has been initialized and properly configured. Please configure the database settings first.");
            }

            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (string.IsNullOrWhiteSpace(data.UniqueKey))
            {
                throw new ArgumentNullException(nameof(data.UniqueKey));
            }

            using var conn = ConnectionMultiplexer.Connect(_owner.Configuration.ToString());
            IDatabase db = conn.GetDatabase();

            //Usually this shouldn't be done but we require this because of Redis being a non rational database
            if (data is IPamaxieUser user)
            {
                if (!db.KeyExists(user.UserName))
                {
                    throw new ArgumentException("The user you entered does not exist in our database yet. Please ensure the username is correct.");
                }

                db.StringSet(user.UserName, user.UniqueKey);
            }

            if (!db.KeyExists(data.UniqueKey))
            {
                throw new ArgumentException("The key you entered does not exist in our database yet");
            }

            string parsedData = JsonConvert.SerializeObject(data);
            db.StringSet(data.UniqueKey, parsedData);
            return data;
        }

        /// <inheritdoc cref="IPamaxieDataInteractionBase{T}.TryUpdate(T, out T)"/>
        public bool TryUpdate(T data, out T updatedItem)
        {
            if (_owner == null)
            {
                throw new NullReferenceException($"The required property {nameof(_owner)} was null. Please ensure it is set before calling this class. This should usually never happen.");
            }

            if (_owner.Configuration == null || string.IsNullOrWhiteSpace(_owner.Configuration.ToString()))
            {
                throw new InvalidOperationException("This method cannot be called before the configuration has been initialized and properly configured. Please configure the database settings first.");
            }

            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (string.IsNullOrWhiteSpace(data.UniqueKey))
            {
                throw new ArgumentNullException(nameof(data.UniqueKey));
            }

            using var conn = ConnectionMultiplexer.Connect(_owner.Configuration.ToString());
            IDatabase db = conn.GetDatabase();
            updatedItem = default;

            if (!db.KeyExists(data.UniqueKey))
            {
                return false;
            }

            //Usually this shouldn't be done but we require this because of Redis being a non rational database
            if (data is IPamaxieUser user)
            {
                if (!db.KeyExists(user.UserName))
                {
                    return false;
                }

                db.StringSet(user.UserName, user.UniqueKey);
            }

            string parsedData = JsonConvert.SerializeObject(data);

            if (!db.StringSet(data.UniqueKey, parsedData))
            {
                return false;
            }

            updatedItem = data;
            return true;
        }

        /// <inheritdoc cref="IPamaxieDataInteractionBase{T}.UpdateOrCreate(T, out T)"/>
        public bool UpdateOrCreate(T data, out T updatedOrCreatedItem)
        {
            if (_owner == null)
            {
                throw new NullReferenceException($"The required property {nameof(_owner)} was null. Please ensure it is set before calling this class. This should usually never happen.");
            }

            if (_owner.Configuration == null || string.IsNullOrWhiteSpace(_owner.Configuration.ToString()))
            {
                throw new InvalidOperationException("This method cannot be called before the configuration has been initialized and properly configured. Please configure the database settings first.");
            }

            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (string.IsNullOrWhiteSpace(data.UniqueKey))
            {
                throw new ArgumentNullException(nameof(data.UniqueKey));
            }

            using var conn = ConnectionMultiplexer.Connect(_owner.Configuration.ToString());
            IDatabase db = conn.GetDatabase();
            updatedOrCreatedItem = default;
            bool createdNew = false;

            if (string.IsNullOrWhiteSpace(data.UniqueKey))
            {
                data.UniqueKey = Guid.NewGuid().ToString();
                Debug.Assert(!db.KeyExists(data.UniqueKey));
                createdNew = true;
            }
            else if (db.KeyExists(data.UniqueKey))
            {
                createdNew = false;
            }

            string parsedData = JsonConvert.SerializeObject(data);

            if (!db.StringSet(data.UniqueKey, parsedData))
            {
                throw new RedisServerException("Problems with creating or updating the value");
            }

            updatedOrCreatedItem = data;
            return createdNew;
        }

        /// <inheritdoc cref="IPamaxieDataInteractionBase{T}.Delete(T)"/>C
        public bool Delete(T data)
        {
            if (_owner == null)
            {
                throw new NullReferenceException($"The required property {nameof(_owner)} was null. Please ensure it is set before calling this class. This should usually never happen.");
            }

            if (_owner.Configuration == null || string.IsNullOrWhiteSpace(_owner.Configuration.ToString()))
            {
                throw new InvalidOperationException("This method cannot be called before the configuration has been initialized and properly configured. Please configure the database settings first.");
            }

            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (string.IsNullOrWhiteSpace(data.UniqueKey))
            {
                throw new ArgumentNullException(nameof(data.UniqueKey));
            }

            using var conn = ConnectionMultiplexer.Connect(_owner.Configuration.ToString());
            IDatabase db = conn.GetDatabase();

            if (!db.KeyExists(data.UniqueKey))
            {
                throw new ArgumentException("The key of the data you entered could not be found in our database.");
            }

            return db.KeyDelete(data.UniqueKey);
        }

        /// <inheritdoc cref="IPamaxieDataInteractionBase{T}.Exists(string)"/>
        public bool Exists(string uniqueKey)
        {
            if (_owner == null)
            {
                throw new NullReferenceException($"The required property {nameof(_owner)} was null. Please ensure it is set before calling this class. This should usually never happen.");
            }

            if (_owner.Configuration == null || string.IsNullOrWhiteSpace(_owner.Configuration.ToString()))
            {
                throw new InvalidOperationException("This method cannot be called before the configuration has been initialized and properly configured. Please configure the database settings first.");
            }

            if (string.IsNullOrWhiteSpace(uniqueKey))
            {
                throw new ArgumentNullException(nameof(uniqueKey));
            }

            using var conn = ConnectionMultiplexer.Connect(_owner.Configuration.ToString());
            IDatabase db = conn.GetDatabase();
            return db.KeyExists(uniqueKey);
        }
    }
}
