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
        
        using StreamReader reader = new StreamReader(HttpContext.Request.Body);
        var body = await reader.ReadToEndAsync();

        if (!string.IsNullOrWhiteSpace(body)&& body.Contains("LongLived = true"))
        {
            longLivedToken = true;
        }

        if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Basic"))
        {
            return Unauthorized(authHeader ?? string.Empty);
        }
        
        var authResult = await ValidateUserCredentials(authHeader.Substring("Basic ".Length).Trim());
        
        if (!authResult.SuccessfulAuth)
        {
            return Unauthorized("Invalid username or password.");
        }

        var newToken = _generator.CreateToken(authResult.user.Id, AppConfigManagement.JwtSettings, false, longLivedToken);
        var items = new { Token = newToken, User = authResult.user};
        return Ok(JsonConvert.SerializeObject(items, Formatting.Indented));
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
        using StreamReader reader = new StreamReader(HttpContext.Request.Body);
        var body = await reader.ReadToEndAsync();

        if (!string.IsNullOrWhiteSpace(body) && body.Contains("LongLived = true"))
        {
            longLivedToken = true;
        }

        if (string.IsNullOrWhiteSpace(token))
        {
            return Unauthorized();
        }

        var userId = JwtTokenGenerator.GetUserKey(token);

        if (userId == 0)
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
    public async Task<ActionResult> CreateTask()
    {
        var user = await GetRequestingPamUserAsync();

        if (user == null)
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

        user = (PamUser) await _dbDriver.Service.Users.GetAsync(user.UserName);

        if (!await SendConfirmationEmailAsync(user.Id, user.Email, user.UserName))
        {
            return StatusCode(500,
                "An error occured while trying to sent the activation email. Please reach out to support.");
        }

        return Ok("A confirmation email has been sent to you");
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="secretCode"></param>
    /// <returns></returns>
    [HttpGet("VerifyUser={secretCode}")]
    [AllowAnonymous]
    public async Task<ActionResult<bool>> VerifyUser(string secretCode)
    {
        if (string.IsNullOrWhiteSpace(secretCode))
        {
            return BadRequest("Invalid email activation link");
        }

        var confirmationCode = await _dbDriver.Service.Users.ValidateConfirmationCodeAsync(secretCode);

        if (!confirmationCode.ConfirmationSuccessful)
        {
            return BadRequest("Invalid or unknown activation link");
        }

        var userDbObj = await _dbDriver.Service.Users.GetAsync(confirmationCode.UserId);

        if (userDbObj is not IPamUser user)
        {
            return StatusCode(500, "We hit an unexpected error while attempting to create the user.");
        }

        user.Flags = UserFlags.ConfirmedAccount;
        await _dbDriver.Service.Users.UpdateAsync(user);

        return Ok("Thank you for confirming your account. We will get back in touch with you once we cleared you" +
                  "for our closed Beta");
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="queryParam"></param>
    /// <returns></returns>
    [HttpGet("Get={queryParam}")]
    public async Task<ActionResult<PamUser>> GetUser(string queryParam)
    {
        if (long.TryParse(queryParam, out long userId))
        {
            var usernameExists = await _dbDriver.Service.Users.ExistsAsync(userId);

            if (!usernameExists)
            {
                return NotFound("A user with this user id does not exist.");
            }
            
            await ValidateUserAccess(userId);
            var user = await _dbDriver.Service.Users.GetAsync(userId);
            
            if (user == null)
            {
                return StatusCode(503, "Internal server error while attempting to get the user via their username");
            }

            return Ok(JsonConvert.SerializeObject(user));
        }
        else
        {
            
            var usernameExists = await _dbDriver.Service.Users.ExistsUsernameAsync(queryParam);
            if (!usernameExists)
            {
                return NotFound("A user with this username does not exist.");
            }
            
            await ValidateUserAccess(queryParam);
            var user = _dbDriver.Service.Users.GetAsync(queryParam);

            if (user == null)
            {
                return StatusCode(503, "Internal server error while attempting to get the user via their username");
            }

            return Ok(JsonConvert.SerializeObject(user));
        }
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="queryParam"></param>
    /// <returns></returns>
    [HttpPost("Update")]
    public async Task<ActionResult<PamUser>> UpdateUser()
    {
        
        var token = Request.Headers["authorization"];
        var requestingUserId = JwtTokenGenerator.GetUserKey(token);
        
        if (!await ValidateUserAccess(requestingUserId))
        {
            return Unauthorized("The user you are trying to access our server with is no longer authorized to use our services." +
                                "This might be because the user was deleted, or because the user was locked for malicious use.");
        }

        PamUser user = await GetRequestingPamUserAsync();
        PamUser requestingUser = new PamUser();
        if (!await IsPamStaff(requestingUserId))
        {
             requestingUser = (PamUser) await _dbDriver.Service.Users.GetAsync(requestingUserId);

             if (user.Id != requestingUserId)
             {
                 return Unauthorized("You are not allowed to change any users besides yourself.");
             }
        }
        else
        {
            requestingUser = await GetRequestingPamUserAsync();
        }

        if (user == null)
        {
            return BadRequest("The requested user could not be found.");
        }

        var couldUpdate = await _dbDriver.Service.Users.UpdateAsync(user);

        if (!couldUpdate)
        {
            return StatusCode(503);
        }

        if (user.Email != requestingUser.Email)
        {
            if (!await SendConfirmationEmailAsync(requestingUserId, user.Email, user.UserName))
            {
                return StatusCode(500, "An error occured while trying to sent the activation email. Please reach out to support.");
            }
        }

        if (await SendChangeEmailAsync(requestingUser.Email, user.UserName))
        {
            return StatusCode(500,
                "An error occured while trying to sent the change information email. Please contact your system administrator.");
        }
        
        return Ok("The user was successfully changed and a notification was sent to their email.");
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="queryParam"></param>
    /// <returns></returns>
    [HttpDelete("Delete={userId}")]
    public async Task<ActionResult<PamUser>> UpdateUser(long userId)
    {
        if (userId == 0)
        {
            return BadRequest("The user Id is invalid.");
        }

        

        var token = Request.Headers["authorization"];
        var requestingUserId = JwtTokenGenerator.GetUserKey(token);
        
        if (!await ValidateUserAccess(requestingUserId))
        {
            return Unauthorized("The user you are trying to access our server with is no longer authorized to use our services." +
                                "This might be because the user was deleted, or because the user was locked for malicious use.");
        }
        
        if (!await IsPamStaff(requestingUserId))
        {
            if (userId!= requestingUserId)
            {
                return Unauthorized("You are not allowed to change any users besides yourself.");
            }
        }

        if (!await _dbDriver.Service.Users.ExistsAsync(userId))
        {
            return NotFound("The specified user does not exist.");
        }

        var toBeDeletedUser = (IPamUser) await _dbDriver.Service.Users.GetAsync(userId);

        var deletedUser = await _dbDriver.Service.Users.DeleteAsync(toBeDeletedUser);

        if (!deletedUser)
        {
            return StatusCode(503, "Could not successfully delete the user from our database. Please contact support.");
        }

        await SendChangeEmailAsync(toBeDeletedUser.Email, toBeDeletedUser.UserName, true);
        return Ok("The user was successfully deleted from our Database.");
    }

    private async Task<PamUser> GetRequestingPamUserAsync()
    {
        using StreamReader reader = new StreamReader(HttpContext.Request.Body);
        var body = await reader.ReadToEndAsync();

        PamUser user = new PamUser();
        try
        {
            user = JsonConvert.DeserializeObject<PamUser>(body);
        }
        catch (JsonException)
        {
            return null;
        }

        return user;
    }

    private async Task<bool> SendChangeEmailAsync(string userEmail, string userName, bool wasDeleted = false)
    {
        var token = Environment.GetEnvironmentVariable(AppConfigManagement.SendGridVar, EnvironmentVariableTarget.Process);

        if (string.IsNullOrWhiteSpace(token))
        {
            return false;
        }
        
        var client = new SendGridClient(token);
        var msg = new SendGridMessage();
        msg.SetFrom("noreply@pamaxie.com", "Pamaxie DevTeam");
        msg.AddTo(userEmail);
        
        if (wasDeleted)
        {
            msg.SetTemplateId("d-05745fda075b43ff9c6d66dbab9c11a0");
        }
        else
        {
            msg.SetTemplateId("d-007c134416324c55a6cadbc5718a9217");
        }
        
        msg.SetTemplateData(new
        {
            UserName = userName
        });

        await client.SendEmailAsync(msg);
        
        return true;
    }
    
    private async Task<bool> SendConfirmationEmailAsync(long userId, string userEmail, string userName)
    {
        var token = Environment.GetEnvironmentVariable(AppConfigManagement.SendGridVar, EnvironmentVariableTarget.Process);

        if (string.IsNullOrWhiteSpace(token))
        {
            return false;
        }
        
        var client = new SendGridClient(token);
        var msg = new SendGridMessage();
        msg.SetFrom("noreply@pamaxie.com", "Pamaxie DevTeam");
        msg.AddTo(userEmail);
        msg.SetTemplateId("d-2cf9999301204c77a0c7a960c759a77a");


        var confirmationCode = Convert.ToHexString(RandomNumberGenerator.GetBytes(10));
        
        msg.SetTemplateData(new
        {
            UserName = userName,
            SignupUrl = new Uri(AppConfigManagement.HostUrl, $"/user/verifyUser={confirmationCode}")
        });

        await client.SendEmailAsync(msg);

        if (string.IsNullOrWhiteSpace(confirmationCode))
        {
            return false;
        }

        await _dbDriver.Service.Users.SetConfirmationCodeAsync(userId, confirmationCode);

        return true;
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

    private async Task<bool> ValidateUserAccess(IPamUser user)
    {
        if (user == null)
        {
            return false;
        }
        
        if (!user.Flags.HasFlag(UserFlags.HasClosedAccess))
        {
            return false;
        }
        else if (!user.Flags.HasFlag(UserFlags.ConfirmedAccount))
        {
            return false;
        }
        else if (user.Flags.HasFlag(UserFlags.Locked))
        {
            return false;
        }

        return true;
    }
    
    private async Task<bool> ValidateUserAccess(string username)
    {
        var dbObj = await _dbDriver.Service.Users.GetAsync(username);
        return await ValidateUserAccess(dbObj);
    }
    
    private async Task<bool> ValidateUserAccess(long userId)
    {
        var dbObj = await _dbDriver.Service.Users.GetAsync(userId);
        if (dbObj is IPamUser user)
        {
            return await ValidateUserAccess(user);
        }

        return false;
    }

    private async Task<(bool SuccessfulAuth, IPamUser user)> ValidateUserCredentials(string credentials)
    {
        var encoding = Encoding.GetEncoding("iso-8859-1");
        var usernamePassword = encoding.GetString(Convert.FromBase64String(credentials));
        var separatorIndex = usernamePassword.IndexOf(':');
        var userName = usernamePassword.Substring(0, separatorIndex);
        var userPass = usernamePassword.Substring(separatorIndex + 1);

        if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(userPass))
        {
            return (false, null);
        }
        
        IPamUser user = await _dbDriver.Service.Users.GetAsync(userName);

        if (user == null)
        {
            return (false, null);
        }

        //Validate the user has access to our closed access alpha and has a confirmed account
        if (!user.Flags.HasFlag(UserFlags.HasClosedAccess) || !user.Flags.HasFlag(UserFlags.ConfirmedAccount))
        {
            return (false, user);
        }
        
        if (Argon2.Verify(user.PasswordHash, userPass))
        {
            return (true, user);
        }

        return (false, user);
    }
}