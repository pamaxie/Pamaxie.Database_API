using System;

namespace Pamaxie.Authentication
{
    /// <summary>
    /// Authentication Token
    /// </summary>
    public sealed class JwtToken
    {
        /// <summary>
        /// The token
        /// </summary>
        public string Token { get; init; }

        /// <summary>
        /// The expiration date of the token
        /// </summary>
        public DateTime ExpiresAtUTC { get; init; }
    }
}