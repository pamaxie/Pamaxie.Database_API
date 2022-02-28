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
        public JwtToken CreateToken(long userId, JwtTokenConfig authTokenSettings = null, bool IsApplicationToken = false, bool longLivedToken = false)
        {
            //Application tokens are always long lived
            longLivedToken = IsApplicationToken;
            
            //Authentication successful so generate JWT Token
            var tokenHandler = new JwtSecurityTokenHandler();

            var key = Encoding.ASCII.GetBytes(authTokenSettings.Secret);
            DateTime? expires;

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

            var token = new JwtSecurityToken("Pamaxie", "Pamaxie", null, DateTime.Now.ToUniversalTime(), expires, new SigningCredentials(new SymmetricSecurityKey(key),SecurityAlgorithms.HmacSha256Signature))
                {
                    Payload =
                    {
                        ["userId"] = userId,
                        ["applicationToken"] = IsApplicationToken
                    }
                };
            
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
            return jwtToken.Claims.FirstOrDefault(x => x.Type == "userId")?.Value;
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