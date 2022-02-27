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
        private string _secret;

        public JwtTokenConfig()
        {
            
        }
        
        public JwtTokenConfig(string secret)
        {
            SymmetricSecurityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secret));
        }

        /// <summary>
        /// Time to live for Keys
        /// </summary>
        public int ExpiresInMinutes { get; set; }

        /// <summary>
        /// Private secret that is used to generate Keys
        /// </summary>
        public string Secret
        {
            get => _secret;
            set
            {
                if (_secret != null && _secret == value)
                {
                    return;
                }
                    
                _secret = value;
                SymmetricSecurityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(value));
            } 
        }

        /// <summary>
        /// Bytes from the Authentications <see cref="Secret"/>
        /// </summary>
        public SecurityKey SymmetricSecurityKey { get; private set; }
    }
}
