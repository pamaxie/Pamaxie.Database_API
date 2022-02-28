using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Pamaxie.Helpers
{
    /// <summary>
    /// This class helps users to work with specific crypto related tasks like generating one way hashes from usernames and passwords and the likes. This is to ensure that users
    /// end up with a consistent expierence.
    /// </summary>
    public static class PamaxieCryptoHelpers
    {
        public static string GetUserId(NetworkCredential credentials)
            => ComputeSha512($"{credentials.UserName}\0{credentials.Password}");
    
        /// <summary>
        /// Computes the hash from some data
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private static string ComputeSha512(string data)
        {
            using (SHA512 sha256Hash = SHA512.Create())
            {
                // ComputeHash - returns byte array  
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF32.GetBytes(data));

                // Convert byte array to a string   
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}
