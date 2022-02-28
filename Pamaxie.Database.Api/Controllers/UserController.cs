using System;
using System.IO;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Isopoh.Cryptography.Argon2;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Pamaxie.Authentication;
using Pamaxie.Data;
using Pamaxie.Database.Extensions;

namespace Pamaxie.Database.Api.Controllers;

/// <summary>
/// Api Controller for handling all <see>
///     <cref>PamaxieUser</cref>
/// </see>
/// interactions
/// </summary>
[Authorize]
[ApiController]
[Route("[controller]")]
public sealed class UserController : ControllerBase
{
    private readonly JwtTokenGenerator _generator;
    private readonly IPamaxieDatabaseDriver _dbDriver;

    /// <summary>
    /// Constructor for <see cref="UserController"/>
    /// </summary>
    /// <param name="generator">Used for generating Jwt tokens for a user</param>
    /// <param name="dbDriver">Driver for talking to the requested database service</param>
    public UserController(JwtTokenGenerator generator, IPamaxieDatabaseDriver dbDriver)
    {
        _dbDriver = dbDriver;
        _generator = generator;
    }


    /// <summary>
    /// Signs in a user via Basic authentication and returns a token.
    /// </summary>
    /// <returns><see cref="JwtToken"/> Token for Authentication</returns>
    [AllowAnonymous]
    [HttpPost("Login")]
    [Consumes(MediaTypeNames.Application.Json)]
    public async Task<ActionResult<JwtToken>> LoginTask()
    {
        var authHeader = Request.Headers["authorization"].ToString();

        if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Basic"))
        {
            return Unauthorized(authHeader ?? string.Empty);
        }

        var encodedUsernamePassword = authHeader.Substring("Basic ".Length).Trim();

        //the coding should be iso or you could use ASCII and UTF-8 decoder
        var encoding = Encoding.GetEncoding("iso-8859-1");
        var usernamePassword = encoding.GetString(Convert.FromBase64String(encodedUsernamePassword));
        var separatorIndex = usernamePassword.IndexOf(':');
        var userName = usernamePassword.Substring(0, separatorIndex);
        var userPass = usernamePassword.Substring(separatorIndex + 1);

        var user = await _dbDriver.Service.Users.GetAsync(userName);

        if (!await ValidateUserAccessAsync(user.Id, userPass))
        {
            return Unauthorized(
                "User is not authorized to login, this maybe due to an invalid username or password" +
                "or the user being locked out of our system. If you are sure your credentials are correct" +
                "please contact support.");
        }

        var newToken = _generator.CreateToken(user.Id, AppConfigManagement.JwtSettings);
        return Ok(newToken);
    }

    /// <summary>
    /// Creates a new Api User, needs to be unauthorized
    /// </summary>
    /// <returns><see cref="string"/> Success?</returns>
    [AllowAnonymous]
    [HttpPost("Create")]
    [Consumes(MediaTypeNames.Application.Json)]
    public async Task<ActionResult<string>> CreateUserTask()
    {
        using var reader = new StreamReader(HttpContext.Request.Body);
        var body = await reader.ReadToEndAsync();

        if (string.IsNullOrWhiteSpace(body))
        {
            return BadRequest("Please specify a user to create");
        }

        var user = JsonConvert.DeserializeObject<PamUser>(body);

        if (user == null)
        {
            return BadRequest("Expected a PamUser object for creation as the body but couldn't find one. Please" +
                              "ensure your Json is correct.");
        }


        if (await _dbDriver.Service.Users.ExistsUsernameAsync(user.UserName) ||
            await _dbDriver.Service.Users.ExistsEmailAsync(user.Email))
        {
            return Conflict("The specified username or email already exists in our database. " +
                            "Please make sure they are unique.");
        }

        //By default we are not granting close access at the moment.
        user.HasClosedAccess = false;
        user.Flags = UserFlags.None;

        var wasSuccess = await _dbDriver.Service.Users.CreateAsync(user);

        if (wasSuccess)
        {
        }
        else
        {
            return StatusCode(500, "Unable to create the user because of an unexpected error during creation. " +
                                   "Please contact your server administrator");
        }

        return Created("/users", null);
    }

    /// <summary>
    /// Refreshes an exiting <see cref="JwtToken"/>
    /// </summary>
    /// <returns>Refreshed <see cref="JwtToken"/></returns>
    [Authorize]
    [HttpPost("RefreshToken")]
    [Consumes(MediaTypeNames.Application.Json)]
    public async Task<ActionResult<JwtToken>> RefreshTask()
    {
        var token = Request.Headers["authorization"];

        if (string.IsNullOrWhiteSpace(token))
        {
            return Unauthorized();
        }

        var userKey = JwtTokenGenerator.GetUserKey(token);

        if (long.TryParse(userKey, out var userId))
        {
            return BadRequest("The entered token does not contain a valid user ID that can be read from it. " +
                              "Please ensure this token is meant for this application and hasn't been tampered with.");
        }

        //Validate if the user was maybe deleted since the last auth
        if (!await ValidateUserAccessAsync(userId))
        {
            return Unauthorized(
                "The token you entered is incorrect or the user of the Token was locked out of our system." +
                "Please check your credentials. If you are sure they are correct please contact support.");
        }

        var newToken = _generator.CreateToken(userId, AppConfigManagement.JwtSettings);
        return Ok(newToken);
    }
    
    /// <summary>
    /// Refreshes an exiting <see cref="JwtToken"/>
    /// </summary>
    /// <returns>Refreshed <see cref="JwtToken"/></returns>
    [Authorize]
    [HttpPost("Get")]
    public async Task<ActionResult<JwtToken>> Get()
    {
        throw new NotImplementedException();
    }

    private async Task<bool> ValidateUserAccessAsync(long userId, string password = null)
    {
        //User does not exist in our Database.
        if (!await _dbDriver.Service.Users.ExistsAsync(userId))
        {
            return false;
        }

        var userObj = await _dbDriver.Service.Users.GetAsync(userId);

        //User object isn't correct which may mean wrong type of ID was entered
        if (userObj is not IPamUser user)
        {
            return false;
        }

        //The users account has not been verified.
        if (!user.Flags.HasFlag(UserFlags.ConfirmedAccount))
        {
            return false;
        }

        //The users account has been locked by our moderation team
        if (!user.Flags.HasFlag(UserFlags.Locked))
        {
            return false;
        }

        //Closed access is currently required to access our service
        if (!user.HasClosedAccess)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            return true;
        }

        return Argon2.Verify(user.PasswordHash, password);
    }
}