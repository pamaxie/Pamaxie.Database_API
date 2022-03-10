using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;

namespace Pamaxie.Authentication
{
    public class JwtTokenConfig
    {
        /// <summary>
        /// Time to live for Keys
        /// </summary>
        public int ExpiresInMinutes { get; set; }
        
        /// <summary>
        /// Lifespan for the long lived token
        /// </summary>
        public int LongLivedExpiresInDays { get; set; }

        /// <summary>
        /// Private secret that is used to generate Keys
        /// </summary>
        public string Secret { get; set; }
    }
}
