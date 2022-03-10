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
        /// <param name="authTokenSettings"></param>
        /// <param name="scanMachineSettings"></param>
        /// <param name="longLivedToken"></param>
        /// <returns>A authentication token object</returns>
        public JwtToken CreateToken(long ownerId, JwtTokenConfig authTokenSettings, JwtScanMachineSettings scanMachineSettings = null, bool longLivedToken = false)
        {

            if (authTokenSettings == null)
            {
                throw new ArgumentNullException(nameof(authTokenSettings));
            }

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

            if (scanMachineSettings != null)
            {
                token.Payload["isApiToken"] = scanMachineSettings.IsScanMachine;
                token.Payload["apiTokenMachineGuid"] = scanMachineSettings.ScanMachineGuid;
                token.Payload["projectId"] = scanMachineSettings.ProjectId;
            }
            
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.WriteToken(token);
            return new JwtToken { ExpiresAtUTC = (DateTime)expires, Token = jwt, IsLongLived = longLivedToken};
        }

        /// <summary>
        /// Decrypts a JWT bearer token for its OwnerId
        /// </summary>
        /// <param name="authToken"></param>
        /// <returns></returns>
        public static long GetOwnerKey(string authToken)
        {
            var jwtToken = new JwtSecurityToken(authToken.Replace("Bearer ", string.Empty));

            if (long.TryParse(jwtToken.Claims.FirstOrDefault(x => x.Type == "ownerId")?.Value, out var ownerId))
            {
                return ownerId;
            }

            return -100;
        }
        
        /// <summary>
        /// Decrypts a JWT bearer token For its IsApplicationToken
        /// </summary>
        /// <param name="authToken"></param>
        /// <returns></returns>
        public static bool IsApplicationToken(string authToken)
        {
            var jwtToken = new JwtSecurityToken(authToken.Replace("Bearer ", string.Empty));
            if (!bool.TryParse(jwtToken.Claims.FirstOrDefault(x => x.Type == "isApiToken")?.Value,
                    out var isAppToken))
            {
                return false;
            }

            return isAppToken;
        }
        
        /// <summary>
        /// Decrypts a JWT bearer token for its ProjectId
        /// </summary>
        /// <param name="authToken"></param>
        /// <returns></returns>
        public static long GetProjectId(string authToken)
        {
            var jwtToken = new JwtSecurityToken(authToken.Replace("Bearer ", string.Empty));
            if (!long.TryParse(jwtToken.Claims.FirstOrDefault(x => x.Type == "projectId")?.Value,
                    out var projectId))
            {
                return projectId;
            }

            return projectId;
        }
        
        /// <summary>
        /// Decrypts a JWT bearer token for its Machine GUID
        /// </summary>
        /// <param name="authToken"></param>
        /// <returns></returns>
        public static string GetMachineGuid(string authToken)
        {
            var jwtToken = new JwtSecurityToken(authToken.Replace("Bearer ", string.Empty));
            var machineGuid = jwtToken.Claims.FirstOrDefault(x => x.Type == "apiTokenMachineGuid");

            if (machineGuid != null && !string.IsNullOrEmpty(machineGuid.Value))
            {
                return machineGuid.Value;
            }

            return null;
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