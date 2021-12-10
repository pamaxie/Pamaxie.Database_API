using Isopoh.Cryptography.Argon2;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Pamaxie.Authentication;
using Pamaxie.Data;
using Pamaxie.Database.Api;
using Pamaxie.Database.Design;
using Pamaxie.Helpers;
using System;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace Pamaxie.Api.Controllers
{
    /// <summary>
    /// Api Controller for handling all authentication interactions
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public sealed class AuthController : ControllerBase
    {
        private readonly JwtTokenGenerator _generator;
        private readonly IPamaxieDatabaseDriver _dbDriver;

        /// <summary>
        /// Constructor for <see cref="AuthController"/>
        /// </summary>
        /// <param name="generator">Token generator</param>
        /// <param name="dbService">Database Service</param>
        public AuthController(JwtTokenGenerator generator, IPamaxieDatabaseDriver dbService)
        {
            _generator = generator;
            _dbDriver = dbService;
        }

        /// <summary>
        /// Signs in a user via Basic authentication and returns a token.
        /// </summary>
        /// <returns><see cref="JwtToken"/> Token for Authentication</returns>
        [AllowAnonymous]
        [HttpPost("Login")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<JwtToken> LoginTask()
        {
            string authHeader = Request.Headers["authorization"].ToString();

            if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Basic"))
            {
                return Forbid(authHeader);
            }

            string encodedUsernamePassword = authHeader.Substring("Basic ".Length).Trim();

            //the coding should be iso or you could use ASCII and UTF-8 decoder
            Encoding encoding = Encoding.GetEncoding("iso-8859-1");
            string usernamePassword = encoding.GetString(Convert.FromBase64String(encodedUsernamePassword));
            int seperatorIndex = usernamePassword.IndexOf(':');
            var userName = usernamePassword.Substring(0, seperatorIndex);
            var userPass = usernamePassword.Substring(seperatorIndex + 1);
            var userId = PamaxieCryptoHelpers.GetUserId(new System.Net.NetworkCredential(userName, userPass));
            var user = _dbDriver.Service.PamaxieUserData.Get(userId);

            if (user == null)
            {
                return Unauthorized("Invalid username or password");
            }
            
            if (!Argon2.Verify(user.Password, userPass))
            {
                return Unauthorized("Invalid username or password");
            }

            var newToken = _generator.CreateToken(userId, ApiApplicationConfiguration.JwtSettings);
            return Ok(newToken);
        }

        /// <summary>
        /// Creates a new Api User, needs to be unauthorized
        /// </summary>
        /// <returns><see cref="string"/> Success?</returns>
        [AllowAnonymous]
        [HttpPost("CreateUser")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<string>> CreateUserTask()
        {
            using var reader = new StreamReader(HttpContext.Request.Body);
            var body = await reader.ReadToEndAsync();

            if (string.IsNullOrWhiteSpace(body))
            {
                return BadRequest("Please specify a user to create");
            }

            JObject googleSearch = JObject.Parse(body);
            var user = googleSearch["userData"].ToObject<PamaxieUser>();

            user.UniqueKey = PamaxieCryptoHelpers.GetUserId(new System.Net.NetworkCredential(user.UserName, user.Password));
            user.Password = Argon2.Hash(user.Password);
            user.EmailVerified = false;
            user.Disabled = false;
            user.TTL = DateTime.MaxValue;

            if (_dbDriver.Service.PamaxieUserData.Exists(user.UniqueKey))
            {
                return BadRequest("The specified user already exists in the database");
            }

            _dbDriver.Service.PamaxieUserData.Create(user);
            return Created(String.Empty, null);
        }

        /// <summary>
        /// Refreshes an exiting <see cref="JwtToken"/>
        /// </summary>
        /// <returns>Refreshed <see cref="JwtToken"/></returns>
        [Authorize]
        [HttpPost("Refresh")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<JwtToken> RefreshTask()
        {

            StringValues token = Request.Headers["authorization"];

            if (string.IsNullOrWhiteSpace(token))
            {
                return BadRequest();
            }

            string userId = JwtTokenGenerator.GetUserKey(token);

            //Validate if the user was maybe deleted since the last auth
            if (!_dbDriver.Service.PamaxieUserData.Exists(userId))
            {
                return Unauthorized();
            }

            var newToken = _generator.CreateToken(userId, ApiApplicationConfiguration.JwtSettings);
            return Ok(newToken);
        }
    }
}