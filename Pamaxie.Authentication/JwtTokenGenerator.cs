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
        /// <param name="ownerId">Id of the item that owns this Token (User or Project for example)</param>
        /// <returns>A authentication token object</returns>
        public JwtToken CreateToken(long ownerId, JwtTokenConfig authTokenSettings = null, bool IsApplicationToken = false, bool longLivedToken = false)
        {

            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();

            byte[] key = Encoding.ASCII.GetBytes(authTokenSettings.Secret);
            DateTime? expires = null;
            
            if (longLivedToken)
            {
                expires = DateTime.Now.AddDays(authTokenSettings.LongLivedExpiresInDays);
            }
            else
            {
                expires = DateTime.Now.AddMinutes(authTokenSettings.ExpiresInMinutes);
            }

            if (expires == null || key == null)
            {
                throw new InvalidOperationException("We hit an unexpected problem while generating the token");
            }

            var token = new JwtSecurityToken("Pamaxie", "Pamaxie", null, DateTime.Now.ToUniversalTime(), expires, new SigningCredentials(new SymmetricSecurityKey(key),SecurityAlgorithms.HmacSha256Signature));
            token.Payload["ownerId"] = ownerId;
            token.Payload["applicationToken"] = IsApplicationToken;
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.WriteToken(token);
            return new JwtToken { ExpiresAtUTC = (DateTime)expires, Token = jwt, IsLongLived = longLivedToken};
        }

        /// <summary>
        /// Decrypts a JWT bearer token
        /// </summary>
        /// <param name="authToken"></param>
        /// <returns></returns>
        public static long GetOwnerKey(string authToken)
        {
            var jwtToken = new JwtSecurityToken(authToken.Replace("Bearer ", string.Empty));

            if (long.TryParse(jwtToken.Claims.FirstOrDefault(x => x.Type == "userId")?.Value, out var userId))
            {
                return userId;
            }

            return -100;
        }
        
        /// <summary>
        /// Decrypts a JWT bearer token
        /// </summary>
        /// <param name="authToken"></param>
        /// <returns></returns>
        public static bool IsApplicationToken(string authToken)
        {
            var jwtToken = new JwtSecurityToken(authToken.Replace("Bearer ", string.Empty));
            if (!bool.TryParse(jwtToken.Claims.FirstOrDefault(x => x.Type == "applicationToken")?.Value,
                    out var isAppToken))
            {
                return false;
            }

            return isAppToken;
        }

        public static string GenerateSecret()
        {
            using var cryptRng = SHA256.Create();
            var tokenBuffer = new byte[64];
            var hash = cryptRng.ComputeHash(tokenBuffer);
            return Convert.ToBase64String(hash);
        }
    }
}