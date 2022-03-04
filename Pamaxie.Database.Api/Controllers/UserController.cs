using System;
using System.IO;
using System.Net.Mime;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Isopoh.Cryptography.Argon2;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Pamaxie.Authentication;
using Pamaxie.Data;
using Pamaxie.Database.Extensions;
using SendGrid;
using SendGrid.Helpers.Mail;

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
    [HttpGet("Login")]
    public async Task<ActionResult<(JwtToken token, IPamUser user)>> LoginTask()
    {
        var authHeader = Request.Headers["authorization"].ToString();

        bool longLivedToken = false;
        if (Request.Body.Length > 0)
        {
            using StreamReader reader = new StreamReader(HttpContext.Request.Body);
            var body = reader.ReadToEnd();

            if (body.Contains("LongLived = true"))
            {
                longLivedToken = true;
            }
        }

        if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Basic"))
        {
            return Unauthorized(authHeader ?? string.Empty);
        }
        
        var authResult = await ValidateUserAccess(authHeader.Substring("Basic ".Length).Trim());
        
        if (!authResult.SuccessfulAuth)
        {
            return Unauthorized("Invalid username or password.");
        }

        var newToken = _generator.CreateToken(authResult.user.Id, AppConfigManagement.JwtSettings, false, longLivedToken);
        return (newToken, authResult.user);
    }
    
    /// <summary>
    /// Signs in a user via Basic authentication and returns a token.
    /// </summary>
    /// <returns><see cref="JwtToken"/> Token for Authentication</returns>
    [HttpGet("RefreshToken")]
    public async Task<ActionResult<JwtToken>> UpdateTask()
    {
        var token = Request.Headers["authorization"];
        
        bool longLivedToken = false;
        if (Request.Body.Length > 0)
        {
            using StreamReader reader = new StreamReader(HttpContext.Request.Body);
            var body = await reader.ReadToEndAsync();

            if (body.Contains("LongLived = true"))
            {
                longLivedToken = true;
            }
        }

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
        if (!await ValidateUserAccess(userId))
        {
            return Unauthorized(
                "The token you entered is incorrect or the user of the Token was locked out of our system." +
                "Please check your credentials. If you are sure they are correct please contact support.");
        }

        var newToken = _generator.CreateToken(userId, AppConfigManagement.JwtSettings, false, longLivedToken);
        return newToken;
    }
    
    /// <summary>
    /// Signs in a user via Basic authentication and returns a token.
    /// </summary>
    /// <returns><see cref="JwtToken"/> Token for Authentication</returns>
    [HttpPost("Create")]
    [AllowAnonymous]
    [Consumes(MediaTypeNames.Application.Json)]
    public async Task<ActionResult<JwtToken>> CreateTask()
    {
        var authHeader = Request.Headers["authorization"].ToString();
        if (Request.Body.Length == 0)
        {
            return BadRequest("Please post a user that should be created");
        }
        
        using StreamReader reader = new StreamReader(HttpContext.Request.Body);
        var body = await reader.ReadToEndAsync();
        var userObject = JsonConvert.DeserializeObject(body);
        if (userObject is not IPamUser user)
        {
            return BadRequest("The input data was in an invalid format or contained an unexpected item.");
        }

        if (string.IsNullOrWhiteSpace(user.Email))
        {
            return BadRequest("The users email was empty. This field is required");
        }

        if (string.IsNullOrWhiteSpace(user.PasswordHash))
        {
            return BadRequest("The users password was empty. This field is required.");
        }

        if (string.IsNullOrWhiteSpace(user.UserName))
        {
            return BadRequest("The users username was empty. This field is required.");
        }
        
        user.Flags = UserFlags.None;
        user.CreationDate = DateTime.Now;
        user.Projects = null;

        if (await _dbDriver.Service.Users.ExistsEmailAsync(user.Email))
        {
            return Conflict("This email already exists.");
        }

        if (await _dbDriver.Service.Users.ExistsUsernameAsync(user.UserName))
        {
            return Conflict("This username already exists.");
        }

        if (!await _dbDriver.Service.Users.CreateAsync(user))
        {
            return StatusCode(500, "We hit an unexpected error while attempting to create the user.");
        }

        user = await _dbDriver.Service.Users.GetAsync(user.UserName);

        var confirmationCode = await SendConfirmationEmailAsync(user.Email, user.UserName);
        
        if (string.IsNullOrWhiteSpace(confirmationCode))
        {
            return StatusCode(500,
                "An error occured while trying to sent the activation email. Please reach out to support.");
        }

        await _dbDriver.Service.Users.SetConfirmationCodeAsync(user.Id, user.UserName);

        return Ok("An email was sent to the users email to confirm their account.");
    }

    private async Task<string> SendConfirmationEmailAsync(string userEmail, string userName)
    {
        var token = Environment.GetEnvironmentVariable(AppConfigManagement.SendGridEnvVar, EnvironmentVariableTarget.Process);

        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }
        
        var client = new SendGridClient(token);
        var msg = new SendGridMessage();
        msg.SetFrom("noreply@pamaxie.com", "Pamaxie DevTeam");
        msg.AddTo(userEmail);
        msg.SetTemplateId("d-2cf9999301204c77a0c7a960c759a77a");


        var confirmationCode = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        msg.SetTemplateData(new
        {
            UserName = userName,
            SignupUrl = $"https://api.pamaxie.com/confirmMail={confirmationCode}"
        });

        await client.SendEmailAsync(msg);

        return confirmationCode;
    }

    private async Task<bool> IsPamStaff(long userId)
    {
        var dbObj = await _dbDriver.Service.Users.GetAsync(userId);
        if (dbObj is IPamUser user)
        {
            return user.Flags.HasFlag(UserFlags.PamaxieStaff);
        }

        return false;
    }

    private async Task<bool> ValidateUserAccess(long userId)
    {
        var dbObj = await _dbDriver.Service.Users.GetAsync(userId);
        if (dbObj is IPamUser user)
        {
            if (!user.Flags.HasFlag(UserFlags.HasClosedAccess))
            {
                return false;
            }
            else if (!user.Flags.HasFlag(UserFlags.ConfirmedAccount))
            {
                return false;
            }
            else if (!user.Flags.HasFlag(UserFlags.Locked))
            {
                return false;
            }

            return true;
        }

        return false;
    }

    private async Task<(bool SuccessfulAuth, IPamUser user)> ValidateUserAccess(string credentials)
    {
        var encoding = Encoding.GetEncoding("iso-8859-1");
        var usernamePassword = encoding.GetString(Convert.FromBase64String(credentials));
        var separatorIndex = usernamePassword.IndexOf(':');
        var userName = usernamePassword.Substring(0, separatorIndex);
        var userPass = usernamePassword.Substring(separatorIndex + 1);
        IPamUser user = await _dbDriver.Service.Users.GetAsync(userName);

        //Validate the user has access to our closed access alpha and has a confirmed account
        if (!user.Flags.HasFlag(UserFlags.HasClosedAccess) || !user.Flags.HasFlag(UserFlags.ConfirmedAccount))
        {
            return (false, user);
        }
        
        if (Argon2.Verify(userPass, user.PasswordHash))
        {
            return (true, user);
        }

        return (false, user);
    }
}