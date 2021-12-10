using Newtonsoft.Json;
using Pamaxie.Data;
using Pamaxie.Database.Design;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pamaxie.Database.Redis.DataInteraction
{
    /// <inheritdoc cref="IPamaxieUserDataInteraction"/>
    internal class PamaxieUserDataInteraction : PamaxieDataInteractionBase<PamaxieUser>, IPamaxieUserDataInteraction
    {
        /// <summary>
        /// Used for accessing the Redis database
        /// </summary>
        private PamaxieDatabaseDriver _owner;

        /// <summary>
        /// Passes through the database driver required for interaction with the database
        /// </summary>
        /// <param name="owner"></param>
        public PamaxieUserDataInteraction(PamaxieDatabaseDriver owner) : base(owner)
        {
            _owner = owner;
        }

        /// <inheritdoc cref="IPamaxieUserDataInteraction.GetAllApplications(PamaxieUser)"/>
        public IEnumerable<PamaxieApplication> GetAllApplications(PamaxieUser value)
        {
            if (_owner == null)
            {
                throw new NullReferenceException($"The required property {nameof(_owner)} was null. Please ensure it is set before calling this class. This should usually never happen.");
            }

            if (_owner.Configuration == null || string.IsNullOrWhiteSpace(_owner.Configuration.ToString()))
            {
                throw new InvalidOperationException("This method cannot be called before the configuration has been initialized and properly configured. Please configure the database settings first.");
            }

            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (string.IsNullOrWhiteSpace(value.UniqueKey))
            {
                throw new ArgumentNullException(nameof(value.UniqueKey));
            }

            using var conn = ConnectionMultiplexer.Connect(_owner.Configuration.ToString());
            IDatabase db = conn.GetDatabase();

            if (!db.KeyExists(value.UniqueKey))
            {
                throw new ArgumentException("A data entry with the specified value could not be found.");
            }

            IEnumerable<string> applicationKeys = value.ApplicationKeys;
            if (applicationKeys == null)
            {
                return null;
            }

            List<PamaxieApplication> applications = new List<PamaxieApplication>();

            foreach (string applicationKey in applicationKeys)
            {
                RedisValue rawData = db.StringGet(applicationKey);

                if (!string.IsNullOrWhiteSpace(rawData))
                {
                    PamaxieApplication application = JsonConvert.DeserializeObject<PamaxieApplication>(rawData);

                    if (application != null)
                    {
                        if (application.OwnerKey == value.UniqueKey)
                        {
                            applications.Add(application);
                        }
                    }
                }
            }

            return applications.AsEnumerable();
        }

        /// <inheritdoc cref="IPamaxieUserDataInteraction.VerifyEmail(PamaxieUser)"/>
        public bool VerifyEmail(PamaxieUser value)
        {
            if (_owner == null)
            {
                throw new NullReferenceException($"The required property {nameof(_owner)} was null. Please ensure it is set before calling this class. This should usually never happen.");
            }

            if (_owner.Configuration == null || string.IsNullOrWhiteSpace(_owner.Configuration.ToString()))
            {
                throw new InvalidOperationException("This method cannot be called before the configuration has been initialized and properly configured. Please configure the database settings first.");
            }

            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (string.IsNullOrWhiteSpace(value.UniqueKey))
            {
                throw new ArgumentNullException(nameof(value.UniqueKey));
            }

            using var conn = ConnectionMultiplexer.Connect(_owner.Configuration.ToString());
            IDatabase db = conn.GetDatabase();

            if (!db.KeyExists(value.UniqueKey))
            {
                return false;
            }

            value.EmailVerified = true;

            string data = JsonConvert.SerializeObject(value);
            db.StringSet(value.UniqueKey, data);
            return true;
        }
    }
}
