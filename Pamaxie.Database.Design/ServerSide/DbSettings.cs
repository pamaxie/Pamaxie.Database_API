﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pamaxie.Database.Extensions
{
    /// <summary>
    /// This is the public object that stores the database settings for each database
    /// </summary>
    public class DbSettings
    {
        /// <summary>
        /// Unique GUID to identify the database
        /// </summary>
        public Guid DatabaseDriverGuid { get; set; }

        /// <summary>
        /// Settings that should be used with the database (like a connection string)
        /// </summary>
        public string Db1Settings { get; set; }
        
        /// <summary>
        /// Settings for the second database
        /// </summary>
        public string Db2Settings { get; set; }
    }
}
