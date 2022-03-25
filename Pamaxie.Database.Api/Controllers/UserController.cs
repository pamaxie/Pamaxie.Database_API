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
using Newtonsoft.Json.Linq;
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
[Route("db/v1/[controller]")]
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
            return Unauthorized();
        }
        
        var authResult = await ValidateUserCredentials(authHeader.Substring("Basic ".Length).Trim());
        
        if (!authResult.SuccessfulAuth)
        {
            return Unauthorized("Invalid username or password.");
        }

        var newToken = _generator.CreateToken(authResult.user.Id, AppConfigManagement.JwtSettings, null, longLivedToken);
        authResult.user.PasswordHash = null;
        var items = new { Token = newToken, User = authResult.user};
        return Ok(JsonConvert.SerializeObject(items));
    }
    
    /// <summary>
    /// Signs in a user via Basic authentication and returns a token.
    /// </summary>
    /// <returns><see cref="JwtToken"/> Token for Authentication</returns>
    [HttpGet("RefreshToken")]
    public async Task<ActionResult<JwtToken>> UpdateTask()
    {
        var token = Request.Headers["authorization"];
        
        if (JwtTokenGenerator.IsApplicationToken(token))
        {
            return Unauthorized("Invalid Token type");
        }

        var userId = JwtTokenGenerator.GetOwnerKey(token);
        
        if (userId < 1 || !await ValidateUserAccess(userId))
        {
            return Unauthorized("The user is not authorized to access our system any longer");
        }
        
        
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

        var newToken = _generator.CreateToken(userId, AppConfigManagement.JwtSettings, null, longLivedToken);
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
        var user = await GetRequestBodyPamUserAsync();

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

        if (user.UserName.Length < 4)
        {
            return BadRequest("The users username should always be at least 4 characters.");
        }

        if (!user.PasswordHash.Contains("$argon2"))
        {
            return BadRequest(
                "The validity of the password hash algorithm could not be validated. Please make sure you use argon2.");
        }

        user.Id = 0;
        user.Flags = UserFlags.None;
        user.CreationDate = DateTime.Now;
        user.Projects = null;

        if (await _dbDriver.Service.Users.ExistsEmailAsync(user.Email))
        {
            return Conflict("This users email already exists");
        }

        if (await _dbDriver.Service.Users.ExistsUsernameAsync(user.UserName))
        {
            return Conflict("This users username already exists.");
        }

        var creationResult = await _dbDriver.Service.Users.CreateAsync(user);
        
        if (!creationResult.wasCreated)
        {
            return StatusCode(500, "We hit an unexpected error while attempting to create the user.");
        }

        user = (PamUser) await _dbDriver.Service.Users.GetAsync(creationResult.createdId);

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

        user.Flags |= UserFlags.ConfirmedAccount;
        await _dbDriver.Service.Users.UpdateAsync(user);

        return Ok("Thank you for confirming your account. We will get back in touch with you once we cleared you" +
                  "for our closed Beta");
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    [HttpPost("Update")]
    public async Task<ActionResult<PamUser>> UpdateUser()
    {
        var token = Request.Headers["authorization"];
        
        if (JwtTokenGenerator.IsApplicationToken(token))
        {
            return Unauthorized("Invalid Token type");
        }

        var userId = JwtTokenGenerator.GetOwnerKey(token);
        
        if (userId < 1 || !await ValidateUserAccess(userId))
        {
            return Unauthorized("The user is not authorized to access our system any longer");
        }

        PamUser user = await GetRequestBodyPamUserAsync();

        if (user.Id < 1)
        {
            return BadRequest("The user Id is not valid. Please ensure you are using a valid user id");
        }
        
        PamUser requestingUser;
        if (!await IsPamStaff(userId))
        {
            requestingUser = (PamUser) await _dbDriver.Service.Users.GetAsync(userId);
            user.Flags = UserFlags.None;
            user.CreationDate = DateTime.MinValue;
            user.Projects = null;
            user.KnownIps = null;
            
            if (user.Id != userId)
            {
                return Unauthorized("You are not allowed to change any users besides yourself.");
            }
        }
        else
        {
            requestingUser = user;
        }

        if (requestingUser == null)
        {
            return NotFound("The requesting user could not be found.");
        }

        if (string.IsNullOrWhiteSpace(user.FirstName) || 
            string.IsNullOrWhiteSpace(user.LastName) ||
            string.IsNullOrWhiteSpace(user.Email) ||
            user.Id < 0)
        {
            return BadRequest("The user object is not valid.");
        }

        if (!await _dbDriver.Service.Users.ExistsAsync(user.Id))
        {
            return NotFound();
        }

        var couldUpdate = await _dbDriver.Service.Users.UpdateAsync(user);

        if (!couldUpdate)
        {
            return StatusCode(503);
        }

        if (user.Email != requestingUser.Email && !requestingUser.Flags.HasFlag(UserFlags.PamaxieStaff))
        {
            if (!await SendConfirmationEmailAsync(userId, user.Email, user.UserName))
            {
                return StatusCode(500, "An error occured while trying to sent the activation email. Please reach out to support.");
            }
        }

        if (userId != user.Id)
        {
            if (!await SendChangeEmailAsync(user.Email, user.UserName))
            {
                return StatusCode(500,
                    "An error occured while trying to sent the change information email. Please contact your system administrator.");
            }
        }
        
        if (!await SendChangeEmailAsync(requestingUser.Email, user.UserName))
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
    [HttpGet("Get={queryParam}")]
    public async Task<ActionResult<PamUser>> GetUser(string queryParam)
    {
        var token = Request.Headers["authorization"];
        
        if (JwtTokenGenerator.IsApplicationToken(token))
        {
            return Unauthorized("Invalid Token type");
        }

        var requestingUserId = JwtTokenGenerator.GetOwnerKey(token);
        
        if (requestingUserId < 1 || !await ValidateUserAccess(requestingUserId))
        {
            return Unauthorized("The user is not authorized to access our system any longer");
        }
        
        if (long.TryParse(queryParam, out long userId))
        {
            if (!await IsPamStaff(requestingUserId))
            {
                if (userId != requestingUserId)
                {
                    return Unauthorized("You are not allowed to get any users besides yourself.");
                }
            }
            
            var userIdExists = await _dbDriver.Service.Users.ExistsAsync(userId);

            if (!userIdExists)
            {
                return NotFound("A user with this user id does not exist.");
            }

            var user = (IPamUser) await _dbDriver.Service.Users.GetAsync(userId);
            
            if (user == null)
            {
                return StatusCode(503, "Internal server error while attempting to get the user via their username");
            }
            
            user.PasswordHash = string.Empty;
            return Ok(JsonConvert.SerializeObject(user));
        }
        else
        {
            if (!await ValidateUserAccess(requestingUserId))
            {
                return Unauthorized("You are not allowed to access our servers at the moment. This might be due to your" +
                                    "account being locked.");
            }
            
            var user = await _dbDriver.Service.Users.GetAsync(queryParam);

            if (!await IsPamStaff(requestingUserId))
            {
                if (userId != requestingUserId)
                {
                    return Unauthorized("You are not allowed to get any users besides yourself.");
                }
            }

            if (user == null)
            {
                return NotFound();
            }

            user.PasswordHash = string.Empty;
            return Ok(JsonConvert.SerializeObject(user));
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    [HttpDelete("Delete={userId}")]
    public async Task<ActionResult<PamUser>> DeleteUser(long userId)
    {
        var token = Request.Headers["authorization"];
        
        if (JwtTokenGenerator.IsApplicationToken(token))
        {
            return Unauthorized("Invalid Token type");
        }

        var requestingUserId = JwtTokenGenerator.GetOwnerKey(token);
        
        if (requestingUserId < 1 || !await ValidateUserAccess(requestingUserId))
        {
            return Unauthorized("The user is not authorized to access our system any longer");
        }
        
        if (!await IsPamStaff(requestingUserId))
        {
            if (userId != requestingUserId)
            {
                return Unauthorized("You are not allowed to delete any users besides yourself.");
            }
        }
        
        if (userId < 1)
        {
            return BadRequest("The user Id is invalid.");
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
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="queryParam"></param>
    /// <returns></returns>
    [HttpGet("GetId={queryParam}")]
    public async Task<ActionResult<long>> GetId(string queryParam)
    {
        var token = Request.Headers["authorization"];
        
        if (JwtTokenGenerator.IsApplicationToken(token))
        {
            return Unauthorized("Invalid Token type");
        }

        var requestingUserId = JwtTokenGenerator.GetOwnerKey(token);
        
        if (requestingUserId < 1 || !await ValidateUserAccess(requestingUserId))
        {
            return Unauthorized("The user is not authorized to access our system any longer");
        }

        if (!await IsPamStaff(requestingUserId))
        {
            return Unauthorized("You are not allowed to access this API endpoint.");
        }
        
        if (string.IsNullOrWhiteSpace(queryParam))
        {
            return BadRequest("The Query parameter may not be empty.");
        }
        
        var isUsername = await _dbDriver.Service.Users.ExistsUsernameAsync(queryParam);
        var isEmail = await _dbDriver.Service.Users.ExistsEmailAsync(queryParam);

        if (!isEmail && !isUsername)
        {
            return NotFound();
        }

        if (isEmail)
        {
            var user = await _dbDriver.Service.Users.GetViaMailAsync(queryParam);
            return user.Id;
        } else
        {
            var user = await _dbDriver.Service.Users.GetAsync(queryParam);
            return user.Id;
        }
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="ipAddress"></param>
    /// <returns></returns>
    [HttpGet("IsIpKnown={ipAddress}")]
    public async Task<ActionResult<bool>> IsIpKnown(string ipAddress)
    {
        var token = Request.Headers["authorization"];
        
        if (JwtTokenGenerator.IsApplicationToken(token))
        {
            return Unauthorized("Invalid Token type");
        }

        var requestingUserId = JwtTokenGenerator.GetOwnerKey(token);
        
        if (requestingUserId < 1 || !await ValidateUserAccess(requestingUserId))
        {
            return Unauthorized("The user is not authorized to access our system any longer");
        }

        return await _dbDriver.Service.Users.IsIpKnownAsync(requestingUserId, ipAddress);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    [HttpPost("loadFully")]
    public async Task<ActionResult<bool>> LoadFully()
    {
        var token = Request.Headers["authorization"];
        
        if (JwtTokenGenerator.IsApplicationToken(token))
        {
            return Unauthorized("Invalid Token type");
        }

        var requestingUserId = JwtTokenGenerator.GetOwnerKey(token);
        
        if (requestingUserId < 1 || !await ValidateUserAccess(requestingUserId))
        {
            return Unauthorized("The user is not authorized to access our system any longer");
        }
        
        var user = await GetRequestBodyPamUserAsync();

        if (user == null || user.Id < 1)
        {
            return BadRequest("Could not read a user from body of request or it was invalid");
        }

        if (!await IsPamStaff(requestingUserId))
        {
            if (requestingUserId != user.Id)
            {
                return Unauthorized("You are not authorized to load any user fully besides yourself.");
            }
        }

        user = (PamUser) await _dbDriver.Service.Users.GetAsync(user.Id);
        var loadedUser = await _dbDriver.Service.Users.LoadFullyAsync(user);
        user.PasswordHash = null;
        return Ok(JsonConvert.SerializeObject(loadedUser));
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    [HttpPost("loadIps")]
    public async Task<ActionResult<bool>> LoadIps()
    {
        var token = Request.Headers["authorization"];
        
        if (JwtTokenGenerator.IsApplicationToken(token))
        {
            return Unauthorized("Invalid Token type");
        }

        var requestingUserId = JwtTokenGenerator.GetOwnerKey(token);
        
        if (requestingUserId < 1 || !await ValidateUserAccess(requestingUserId))
        {
            return Unauthorized("The user is not authorized to access our system any longer");
        }
        
        var user = await GetRequestBodyPamUserAsync();

        if (user == null)
        {
            return BadRequest("Could not read user from body of request");
        }

        if (!await IsPamStaff(requestingUserId))
        {
            if (requestingUserId != user.Id)
            {
                return Unauthorized("You are not authorized to load any user fully besides yourself.");
            }
        }

        user = (PamUser) await _dbDriver.Service.Users.GetAsync(user.Id);
        var loadedUser = await _dbDriver.Service.Users.LoadKnownIpsAsync(user);
        
        user.PasswordHash = null;
        return Ok(JsonConvert.SerializeObject(loadedUser));
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    [HttpPost("loadProjects")]
    public async Task<ActionResult<bool>> LoadProjects()
    {
        var token = Request.Headers["authorization"];
        
        if (JwtTokenGenerator.IsApplicationToken(token))
        {
            return Unauthorized("Invalid Token type");
        }

        var requestingUserId = JwtTokenGenerator.GetOwnerKey(token);
        
        if (requestingUserId < 1 || !await ValidateUserAccess(requestingUserId))
        {
            return Unauthorized("The user is not authorized to access our system any longer");
        }
        
        var user = await GetRequestBodyPamUserAsync();

        if (user == null)
        {
            return BadRequest("Could not read user from body of request");
        }

        if (!await IsPamStaff(requestingUserId))
        {
            if (requestingUserId != user.Id)
            {
                return Unauthorized("You are not authorized to load any user fully besides yourself.");
            }
        }

        user = (PamUser) await _dbDriver.Service.Users.GetAsync(user.Id);
        var loadedUser = await _dbDriver.Service.Users.LoadProjectsAsync(user);
        
        user.PasswordHash = null;
        return Ok(JsonConvert.SerializeObject(loadedUser));
    }

    private async Task<PamUser> GetRequestBodyPamUserAsync()
    {
        using StreamReader reader = new StreamReader(HttpContext.Request.Body);
        var body = await reader.ReadToEndAsync();

        PamUser user;
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
        var token = Environment.GetEnvironmentVariable(AppConfigManagement.SendGridVar);

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
        var token = Environment.GetEnvironmentVariable(AppConfigManagement.SendGridVar);

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
            SignupUrl = new Uri(AppConfigManagement.HostUrl + $"user/verifyUser={confirmationCode}")
        });

        await client.SendEmailAsync(msg);

        if (string.IsNullOrWhiteSpace(confirmationCode))
        {
            return false;
        }

        await _dbDriver.Service.Users.SetConfirmationCodeAsync(userId, confirmationCode);

        return true;
    }
    
    internal static Task<bool> ValidateUserAccess(IPamUser user)
    {
        if (user == null)
        {
            return Task.FromResult(false);
        }

        if (!user.Flags.HasFlag(UserFlags.HasClosedAccess))
        {
            return Task.FromResult(false);
        }
        else if (!user.Flags.HasFlag(UserFlags.ConfirmedAccount))
        {
            return Task.FromResult(false);
        }
        else if (user.Flags.HasFlag(UserFlags.Locked))
        {
            return Task.FromResult(false);
        }

        return Task.FromResult(true);
    }

    private async Task<bool> IsPamStaff(long userId)
    {
        var dbObj = await _dbDriver.Service.Users.GetAsync(userId);
        if (dbObj is PamUser user)
        {
            return user.Flags.HasFlag(UserFlags.PamaxieStaff);
        }

        return false;
    }
    
    private async Task<bool> ValidateUserAccess(long userId)
    {
        var dbObj = await _dbDriver.Service.Users.GetAsync(userId);
        if (dbObj is PamUser user)
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
        
        if (await _dbDriver.Service.Users.ValidatePassword(userPass, user.Id))
        {
            return (true, user);
        }

        return (false, user);
    }
}