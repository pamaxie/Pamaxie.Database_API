using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace Pamaxie.Authentication
{
    /// <summary>
    /// Authentication token generator
    /// </summary>
    public sealed class JwtTokenGenerator
    {
        private readonly IConfiguration _configuration;

        public JwtTokenGenerator(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Creates a token object through JWT encoding
        /// </summary>
        /// <param name="userId">User id of the user that will be contained in the token</param>
        /// <returns>A authentication token object</returns>
        public JwtToken CreateToken(string userId, string authTokenSettings = null)
        {
            //TODO: something is wrong with how this token is generated here. We need to fix this for gathering user information about a token to refresh it for example

           //Authentication successful so generate JWT Token
            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();

            byte[] key = null;
            DateTime? expires = null;
            if (string.IsNullOrWhiteSpace(authTokenSettings))
            {
                IConfigurationSection section = _configuration.GetSection("AuthData");
                key = Encoding.ASCII.GetBytes(section.GetValue<string>("Secret"));
                expires = DateTime.UtcNow.AddMinutes(section.GetValue<int>("ExpiresInMinutes"));
            }
            else
            {
                //TODO: this is only a temporary fix until we figured out how to use our own configuration via dependency injection.
                var settings = JsonConvert.DeserializeObject<JwtTokenConfig>(authTokenSettings);
                key = Encoding.ASCII.GetBytes(settings.Secret);
                expires = DateTime.UtcNow.AddMinutes(settings.ExpiresInMinutes);
            }

            if (expires == null || key == null)
            {
                throw new InvalidOperationException("We hit an unexpected problem while generating the token");
            }

            var token = new JwtSecurityToken("Pamaxie", "Pamaxie", null, DateTime.Now.ToUniversalTime(), expires, new SigningCredentials(new SymmetricSecurityKey(key),SecurityAlgorithms.HmacSha256Signature));
            token.Payload["userId"] = userId;
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.WriteToken(token);
            return new JwtToken { ExpiresAtUTC = (DateTime)expires, Token = jwt };
        }

        /// <summary>
        /// Decrypts a JWT bearer token
        /// </summary>
        /// <param name="authToken"></param>
        /// <returns></returns>
        public static string GetUserKey(string authToken)
        {
            var jwtToken = new JwtSecurityToken(authToken.Replace("Bearer ", string.Empty));
            return jwtToken.Claims.FirstOrDefault(x => x.Type == "userId").Value;
        }

        public static string GenerateSecret()
        {
            using RNGCryptoServiceProvider cryptRng = new RNGCryptoServiceProvider();
            
            byte[] tokenBuffer = new byte[64];
            cryptRng.GetBytes(tokenBuffer);
            return Convert.ToBase64String(tokenBuffer);
        }
    }
}