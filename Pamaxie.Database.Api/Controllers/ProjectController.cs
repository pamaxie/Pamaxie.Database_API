using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Pamaxie.Authentication;
using Pamaxie.Data;
using Pamaxie.Database.Extensions;

namespace Pamaxie.Database.Api.Controllers;

/// <summary>
/// Controller for managing <see cref="IPamProject"/>
/// </summary>
[Authorize]
[ApiController]
[Route("db/v1/[controller]")]
public class ProjectController : ControllerBase
{
    private readonly JwtTokenGenerator _generator;
    private readonly IPamaxieDatabaseDriver _dbDriver;

    /// <summary>
    /// Constructor for <see cref="UserController"/>
    /// </summary>
    /// <param name="generator">Used for generating Jwt tokens for a user</param>
    /// <param name="dbDriver">Driver for talking to the requested database service</param>
    public ProjectController(JwtTokenGenerator generator, IPamaxieDatabaseDriver dbDriver)
    {
        _dbDriver = dbDriver;
        _generator = generator;
    }

    /// <summary>
    /// Creates a new Project
    /// </summary>
    /// <returns></returns>
    [HttpPost("Create")]
    public async Task<ActionResult<bool>> CreateProject()
    {
        var token = Request.Headers["authorization"];
        
        if (JwtTokenGenerator.IsApplicationToken(token))
        {
            return Unauthorized("Invalid Token type");
        }

        var requestingUserId = JwtTokenGenerator.GetOwnerKey(token);
        
        if (requestingUserId < 1)
        {
            return Unauthorized("The user is not authorized to access our system any longer");
        }
        
        var user = (PamUser) await _dbDriver.Service.Users.GetAsync(requestingUserId);

        if (user == null)
        {
            return Unauthorized("The user is not part of our system");
        }

        if (!await UserController.ValidateUserAccess(user))
        {
            return Unauthorized("The user is not authorized to access our system any longer");
        }

        var project = await GetProjectFromMethodBody();

        if (project == null)
        {
            return BadRequest("Malformed request body.");
        }

        if (string.IsNullOrWhiteSpace(project.Name))
        {
            return BadRequest("Please enter a project name");
        }

        project.Id = 0;
        project.Users = null;
        project.Owner = new LazyObject<(PamUser User, long UserId)>
        {
            Data = (user, requestingUserId),
            IsLoaded = true
        };

        project.LastModifiedUser = new LazyObject<(PamUser User, long UserId)>
        {
            Data = (user, requestingUserId),
            IsLoaded = true
        };

        project.LastModifiedAt = DateTime.Now;
        project.CreationDate = DateTime.Now;
        project.Flags = ProjectFlags.None;

        var (wasCreated, createdId) = await _dbDriver.Service.Projects.CreateAsync(project);

        if (!wasCreated)
        {
            return StatusCode(503, "Hit unexpected error while trying to modify this project");
        }

        var couldAddOwner = await _dbDriver.Service.Projects.AddUserAsync(
            createdId, 
            requestingUserId, 
            ProjectPermissions.Administrator);

        if (!couldAddOwner)
        {
            return StatusCode(503, "Could not add the owner to the project. Please contact support");
        }
        
        return Created("", createdId);
    }

    /// <summary>
    /// Updates an existing Project, please ensure all data is requested before reaching in data here.
    /// </summary>
    /// <returns></returns>
    [HttpPost("Update")]
    public async Task<ActionResult<bool>> UpdateProject()
    {
        var token = Request.Headers["authorization"];
        
        if (JwtTokenGenerator.IsApplicationToken(token))
        {
            return Unauthorized("Invalid Token type");
        }

        var requestingUserId = JwtTokenGenerator.GetOwnerKey(token);
        
        if (requestingUserId < 1)
        {
            return Unauthorized("The user is not authorized to access our system any longer");
        }
        
        var user = (PamUser) await _dbDriver.Service.Users.GetAsync(requestingUserId);

        if (user == null)
        {
            return Unauthorized("The user is not part of our system");
        }

        if (!await UserController.ValidateUserAccess(user))
        {
            return Unauthorized("The user is not authorized to access our system any longer");
        }
        
        var project = await GetProjectFromMethodBody();
        
        if (project == null)
        {
            return BadRequest("Malformed request body.");
        }


        if (string.IsNullOrWhiteSpace(project.Name) || project.Owner?.Data.UserId == 0 || project.Id == 0)
        {
            return BadRequest(
                "The reached in data is not valid. Please make sure the owner, name and Id contain a value");
        }

        if (project.Owner == null){
            return BadRequest("The owner of the project could not be found. Ensure that the Owner of the project was reached in with the request.");
        }

        if (!await _dbDriver.Service.Projects.ExistsAsync(project.Id))
        {
            return NotFound("The specified project with that Id does not exist in our Database");
        }

        if (!user.Flags.HasFlag(UserFlags.PamaxieStaff))
        {
            project.Flags = ProjectFlags.None;

            if (!await _dbDriver.Service.Projects.HasPermissionAsync(project.Id, requestingUserId,
                    ProjectPermissions.Read))
            {
                return Unauthorized("You are not allowed to access this project");
            }

            if (!await _dbDriver.Service.Projects.HasPermissionAsync(project.Id, requestingUserId,
                    ProjectPermissions.Write))
            {
                return Unauthorized("You are not allowed to modify this project");
            }
        }

        project.LastModifiedAt = DateTime.Now;
        project.LastModifiedUser = new LazyObject<(PamUser User, long UserId)>
        {
            Data = (user, requestingUserId),
            IsLoaded = true
        };

        if (!await _dbDriver.Service.Projects.UpdateAsync(project))
        {
            return StatusCode(503, "Hit unexpected error while trying to modify this project");
        }

        return Ok();
    }

    /// <summary>
    /// Gets an existing project via its Id
    /// </summary>
    /// <param name="projectId"></param>
    /// <returns></returns>
    [HttpGet("Get={projectId}")]
    public async Task<ActionResult<PamProject>> GetProject(long projectId)
    {
        var token = Request.Headers["authorization"];
        
        if (JwtTokenGenerator.IsApplicationToken(token))
        {
            return Unauthorized("Invalid Token type");
        }

        var requestingUserId = JwtTokenGenerator.GetOwnerKey(token);
        
        if (requestingUserId < 1)
        {
            return Unauthorized("The user is not authorized to access our system any longer");
        }
        
        var user = (PamUser) await _dbDriver.Service.Users.GetAsync(requestingUserId);

        if (user == null)
        {
            return Unauthorized("The user is not part of our system");
        }

        if (!await UserController.ValidateUserAccess(user))
        {
            return Unauthorized("The user is not authorized to access our system any longer");
        }

        if (!user.Flags.HasFlag(UserFlags.PamaxieStaff))
        {
            if (!await _dbDriver.Service.Projects.HasPermissionAsync(projectId, requestingUserId,
                    ProjectPermissions.Read))
            {
                return Unauthorized("You are not allowed to access this project");
            }
        }

        if (!await _dbDriver.Service.Projects.ExistsAsync(projectId))
        {
            return NotFound("The specified project does not exist.");
        }

        var project = await _dbDriver.Service.Projects.GetAsync(projectId);
        return Ok(JsonConvert.SerializeObject(project));
    }

    /// <summary>
    /// Deletes an existing project via its Id
    /// </summary>
    /// <param name="projectId"></param>
    /// <returns></returns>
    [HttpDelete("Delete={projectId}")]
    public async Task<ActionResult<PamUser>> Deleteoject(long projectId)
    {
        var token = Request.Headers["authorization"];
        
        if (JwtTokenGenerator.IsApplicationToken(token))
        {
            return Unauthorized("Invalid Token type");
        }

        var requestingUserId = JwtTokenGenerator.GetOwnerKey(token);
        
        if (requestingUserId < 1)
        {
            return Unauthorized("The user is not authorized to access our system any longer");
        }
        
        var user = (PamUser) await _dbDriver.Service.Users.GetAsync(requestingUserId);

        if (user == null)
        {
            return Unauthorized("The user is not part of our system");
        }

        if (!await UserController.ValidateUserAccess(user))
        {
            return Unauthorized("The user is not authorized to access our system any longer");
        }
        
        if (!await _dbDriver.Service.Projects.ExistsAsync(projectId))
        {
            return NotFound("Specified project does not exist.");
        }

        if (user.Flags.HasFlag(UserFlags.PamaxieStaff))
        {
            if (!await _dbDriver.Service.Projects.IsOwnerAsync(requestingUserId, projectId))
            {
                return Unauthorized("You are not allowed to delete any projects, besides the ones owned yourself.");
            }
        }

        if (!await _dbDriver.Service.Projects.ExistsAsync(projectId))
        {
            return NotFound("The specified project does not exist.");
        }

        var deletedProject = await _dbDriver.Service.Projects.DeleteAsync(projectId);

        if (!deletedProject)
        {
            return StatusCode(503,
                "Could not successfully delete the project from our database. Please contact support.");
        }

        return Ok("The Project was successfully deleted from our Database.");
    }

    /// <summary>
    /// Checks if a user has the permissions to a projec
    /// </summary>
    /// <returns></returns>
    [HttpPost("hasPermission")]
    public async Task<ActionResult<bool>> HasPermission()
    {
        var token = Request.Headers["authorization"];
        
        if (JwtTokenGenerator.IsApplicationToken(token))
        {
            return Unauthorized("Invalid Token type");
        }

        var requestingUserId = JwtTokenGenerator.GetOwnerKey(token);
        
        if (requestingUserId < 1)
        {
            return Unauthorized("The user is not authorized to access our system any longer");
        }
        
        var user = (PamUser) await _dbDriver.Service.Users.GetAsync(requestingUserId);

        if (user == null)
        {
            return Unauthorized("The user is not part of our system");
        }

        if (!await UserController.ValidateUserAccess(user))
        {
            return Unauthorized("The user is not authorized to access our system any longer");
        }

        using StreamReader reader = new StreamReader(HttpContext.Request.Body);
        var body = await reader.ReadToEndAsync();

        long userId, projectId;
        ProjectPermissions permissions;
        try
        {
            JObject jObject = JObject.Parse(body);
            var jsonUser = jObject.SelectToken("UserId");
            var jsonProject = jObject.SelectToken("ProjectId");
            var jsonPermissions = jObject.SelectToken("Permissions");

            if (jsonUser == null || jsonProject == null || jsonPermissions == null)
            {
                return BadRequest("Invalid or malformed Body data.");
            }

            userId = jsonUser.ToObject<long>();
            projectId = jsonProject.ToObject<long>();
            permissions = jsonPermissions.ToObject<ProjectPermissions>();
        }
        catch (JsonException)
        {
            return BadRequest("Malformed and unreadable body data");
        }

        if (userId < 1 || projectId < 1 || permissions == ProjectPermissions.None)
        {
            return BadRequest(
                "Invalid Request data. Please make sure you request some form of permissions and the user and project Id are higher than 0");
        }
        
        if (!await _dbDriver.Service.Projects.ExistsAsync(projectId))
        {
            return NotFound("Specified project does not exist.");
        }

        if (!user.Flags.HasFlag(UserFlags.PamaxieStaff))
        {
            //User is requesting permissions for not themselves.
            if (userId != requestingUserId)
            {
                //User is requesting permissions without being an administrator
                if (!await _dbDriver.Service.Projects.HasPermissionAsync(projectId, requestingUserId,
                        ProjectPermissions.Administrator))
                {
                    return Unauthorized(
                        "You cannot change or request any permissions for any projects where you are not an administrator. " +
                        "(Or for requesting them, where you aren't requesting the permissions for yourself)");
                }
            }
        }
        

        if (!await _dbDriver.Service.Projects.HasPermissionAsync(projectId, userId, permissions))
        {
            return Ok(false);
        }

        return Ok(true);
    }

    /// <summary>
    /// Sets the permissions for a user
    /// </summary>
    /// <returns></returns>
    [HttpPost("setPermissions")]
    public async Task<ActionResult<bool>> SetPermissions()
    {
        var token = Request.Headers["authorization"];
        
        if (JwtTokenGenerator.IsApplicationToken(token))
        {
            return Unauthorized("Invalid Token type");
        }

        var requestingUserId = JwtTokenGenerator.GetOwnerKey(token);
        
        if (requestingUserId < 1)
        {
            return Unauthorized("The user is not authorized to access our system any longer");
        }
        
        var user = (PamUser) await _dbDriver.Service.Users.GetAsync(requestingUserId);

        if (user == null)
        {
            return Unauthorized("The user is not part of our system");
        }

        if (!await UserController.ValidateUserAccess(user))
        {
            return Unauthorized("The user is not authorized to access our system any longer");
        }

        using StreamReader reader = new StreamReader(HttpContext.Request.Body);
        var body = await reader.ReadToEndAsync();

        long userId, projectId;
        ProjectPermissions permissions;
        try
        {
            JObject jObject = JObject.Parse(body);
            var jsonUser = jObject.SelectToken("UserId");
            var jsonProject = jObject.SelectToken("ProjectId");
            var jsonPermissions = jObject.SelectToken("Permissions");

            if (jsonUser == null || jsonProject == null || jsonPermissions == null)
            {
                return BadRequest("Invalid or malformed Body data.");
            }

            userId = jsonUser.ToObject<long>();
            projectId = jsonProject.ToObject<long>();
            permissions = jsonPermissions.ToObject<ProjectPermissions>();
        }
        catch (JsonException)
        {
            return BadRequest("Malformed and unreadable body data");
        }

        if (userId < 1 || projectId < 1 || permissions == ProjectPermissions.None)
        {
            return BadRequest(
                "Invalid Request data. Please make sure you request some form of permissions and the user and project Id are higher than 0");
        }
        
        if (!await _dbDriver.Service.Projects.ExistsAsync(projectId))
        {
            return NotFound("Specified project does not exist.");
        }

        if (!user.Flags.HasFlag(UserFlags.PamaxieStaff))
        {
            if (!await _dbDriver.Service.Projects.HasPermissionAsync(projectId, requestingUserId,
                    ProjectPermissions.Administrator))
            {
                return BadRequest(
                    "You cannot change or request any permissions for any projects where you are not an administrator.");
            }
        }

        if (!await _dbDriver.Service.Projects.SetPermissionsAsync(projectId, userId, permissions))
        {
            return StatusCode(503, "Error while attempting to change the permissions for the project");
        }

        return Ok(true);
    }

    /// <summary>
    /// Creates a new API Token
    /// </summary>
    /// <returns></returns>
    [HttpPost("createToken")]
    public async Task<ActionResult<string>> CreateToken()
    {
        var token = Request.Headers["authorization"];
        
        if (JwtTokenGenerator.IsApplicationToken(token))
        {
            return Unauthorized("Invalid Token type");
        }

        var requestingUserId = JwtTokenGenerator.GetOwnerKey(token);
        
        if (requestingUserId < 1)
        {
            return Unauthorized("The user is not authorized to access our system any longer");
        }
        
        var user = (PamUser) await _dbDriver.Service.Users.GetAsync(requestingUserId);

        if (user == null)
        {
            return Unauthorized("The user is not part of our system");
        }

        if (!await UserController.ValidateUserAccess(user))
        {
            return Unauthorized("The user is not authorized to access our system any longer");
        }
        
        using StreamReader reader = new StreamReader(HttpContext.Request.Body);
        var body = await reader.ReadToEndAsync();

        long projectId;
        DateTime expiresAt;
        try
        {
            JObject jObject = JObject.Parse(body);
            var projectIdJson = jObject.SelectToken("ProjectId");
            var expiresAtJson = jObject.SelectToken("ExpiresAtUTC");

            if (projectIdJson == null || expiresAtJson == null)
            {
                return BadRequest("Invalid or malformed Body data.");
            }

            projectId = projectIdJson.ToObject<long>();
            expiresAt = expiresAtJson.ToObject<DateTime>();
            expiresAt = expiresAt.ToUniversalTime();
        }
        catch (JsonException)
        {
            return BadRequest("Malformed and unreadable body data");
        }
        
        if (projectId < 1 || expiresAt <= DateTime.Now.AddMinutes(10))
        {
            return BadRequest(
                "Invalid Request data. Please make sure you request some form of permissions and the user and project Id are higher than 0");
        }
        
        if (!await _dbDriver.Service.Projects.ExistsAsync(projectId))
        {
            return NotFound("Specified project does not exist.");
        }

        if (!user.Flags.HasFlag(UserFlags.PamaxieStaff))
        {
            if (!await _dbDriver.Service.Projects.HasPermissionAsync(projectId, requestingUserId,
                    ProjectPermissions.Write))
            {
                return Unauthorized("You are not allowed to access this project, or lack the permissions to modify it");
            }
        }

        var apiToken = await _dbDriver.Service.Projects.CreateTokenAsync(projectId, expiresAt);
        return Ok(apiToken);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    [HttpDelete("removeToken")]
    public async Task<ActionResult<string>> RemoveToken()
    {
        var token = Request.Headers["authorization"];
        
        if (JwtTokenGenerator.IsApplicationToken(token))
        {
            return Unauthorized("Invalid Token type");
        }

        var requestingUserId = JwtTokenGenerator.GetOwnerKey(token);
        
        if (requestingUserId < 1)
        {
            return Unauthorized("The user is not authorized to access our system any longer");
        }
        
        var user = (PamUser) await _dbDriver.Service.Users.GetAsync(requestingUserId);

        if (user == null)
        {
            return Unauthorized("The user is not part of our system");
        }

        if (!await UserController.ValidateUserAccess(user))
        {
            return Unauthorized("The user is not authorized to access our system any longer");
        }
        
        using StreamReader reader = new StreamReader(HttpContext.Request.Body);
        var body = await reader.ReadToEndAsync();

        long projectId, tokenId;
        
        try
        {
            JObject jObject = JObject.Parse(body);
            var projectIdJson = jObject.SelectToken("ProjectId");
            var tokenIdJson = jObject.SelectToken("TokenId");

            if (projectIdJson == null || tokenIdJson == null)
            {
                return BadRequest("Invalid or malformed Body data.");
            }

            projectId = projectIdJson.ToObject<long>();
            tokenId = tokenIdJson.ToObject<long>();
        }
        catch (JsonException)
        {
            return BadRequest("Malformed and unreadable body data");
        }
        
        if (projectId < 1 || tokenId < 1)
        {
            return BadRequest(
                "Invalid Request data. Please make sure you request some form of permissions and the user and project Id are higher than 0");
        }
        
        if (!await _dbDriver.Service.Projects.ExistsAsync(projectId))
        {
            return NotFound("Specified project does not exist.");
        }

        if (!user.Flags.HasFlag(UserFlags.PamaxieStaff))
        {
            if (!await _dbDriver.Service.Projects.HasPermissionAsync(projectId, requestingUserId,
                    ProjectPermissions.Write))
            {
                return Unauthorized("You are not allowed to access this project, or lack the permissions to modify it");
            }
        }


        if (!await _dbDriver.Service.Projects.RemoveTokenAsync(projectId, tokenId))
        {
            return StatusCode(503, "Error while attempting to delete database Object");
        }

        return Ok();
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    [HttpPost("addUser")]
    public async Task<ActionResult<string>> AddUser()
    {
        var token = Request.Headers["authorization"];
        
        if (JwtTokenGenerator.IsApplicationToken(token))
        {
            return Unauthorized("Invalid Token type");
        }

        var requestingUserId = JwtTokenGenerator.GetOwnerKey(token);
        
        if (requestingUserId < 1)
        {
            return Unauthorized("The user is not authorized to access our system any longer");
        }
        
        var user = (PamUser) await _dbDriver.Service.Users.GetAsync(requestingUserId);

        if (user == null)
        {
            return Unauthorized("The user is not part of our system");
        }

        if (!await UserController.ValidateUserAccess(user))
        {
            return Unauthorized("The user is not authorized to access our system any longer");
        }
        
        using StreamReader reader = new StreamReader(HttpContext.Request.Body);
        var body = await reader.ReadToEndAsync();

        long projectId, userId;
        ProjectPermissions permissions;
        
        try
        {
            JObject jObject = JObject.Parse(body);
            var jsonUserId = jObject.SelectToken("UserId");
            var jsonProjectId = jObject.SelectToken("ProjectId");
            var jsonPermissions = jObject.SelectToken("Permissions");

            if (jsonProjectId == null || jsonUserId == null || jsonPermissions == null)
            {
                return BadRequest("Invalid or malformed Body data.");
            }

            projectId = jsonProjectId.ToObject<long>();
            userId = jsonUserId.ToObject<long>();
            permissions = jsonPermissions.ToObject<ProjectPermissions>();
        }
        catch (JsonException)
        {
            return BadRequest("Malformed and unreadable body data");
        }
        
        if (projectId < 1 || userId < 1)
        {
            return BadRequest(
                "Invalid Request data. Please make sure you request some form of permissions and the user and project Id are higher than 0");
        }
        
        if (!await _dbDriver.Service.Projects.ExistsAsync(projectId))
        {
            return NotFound("Specified project does not exist.");
        }

        if (!user.Flags.HasFlag(UserFlags.PamaxieStaff))
        {
            if (!await _dbDriver.Service.Projects.HasPermissionAsync(projectId, requestingUserId,
                    ProjectPermissions.Administrator))
            {
                return Unauthorized("You are not allowed to access this project, or lack the permissions to modify it");
            }
        }

        if (await _dbDriver.Service.Projects.HasUserAsync(projectId, userId))
        {
            return Conflict("User is already part of the project");
        }

        if (!await _dbDriver.Service.Projects.AddUserAsync(projectId, userId, permissions))
        {
            return StatusCode(503, "Error while attempting to delete database Object");
        }

        return Ok();
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    [HttpDelete("RemoveUser")]
    public async Task<ActionResult<string>> RemoveUser()
    {
        var token = Request.Headers["authorization"];
        
        if (JwtTokenGenerator.IsApplicationToken(token))
        {
            return Unauthorized("Invalid Token type");
        }

        var requestingUserId = JwtTokenGenerator.GetOwnerKey(token);
        
        if (requestingUserId < 1)
        {
            return Unauthorized("The user is not authorized to access our system any longer");
        }
        
        var user = (PamUser) await _dbDriver.Service.Users.GetAsync(requestingUserId);

        if (user == null)
        {
            return Unauthorized("The user is not part of our system");
        }

        if (!await UserController.ValidateUserAccess(user))
        {
            return Unauthorized("The user is not authorized to access our system any longer");
        }
        
        using StreamReader reader = new StreamReader(HttpContext.Request.Body);
        var body = await reader.ReadToEndAsync();

        long projectId, userId;
        
        try
        {
            JObject jObject = JObject.Parse(body);
            var jsonUserId = jObject.SelectToken("UserId");
            var jsonProjectId = jObject.SelectToken("ProjectId");

            if (jsonProjectId == null || jsonUserId == null)
            {
                return BadRequest("Invalid or malformed Body data.");
            }

            projectId = jsonProjectId.ToObject<long>();
            userId = jsonUserId.ToObject<long>();
        }
        catch (JsonException)
        {
            return BadRequest("Malformed and unreadable body data");
        }
        
        if (projectId < 1 || userId < 1)
        {
            return BadRequest(
                "Invalid Request data. Please make sure you request some form of permissions and the user and project Id are higher than 0");
        }
        
        if (!await _dbDriver.Service.Projects.ExistsAsync(projectId))
        {
            return NotFound("Specified project does not exist.");
        }

        if (!user.Flags.HasFlag(UserFlags.PamaxieStaff))
        {
            if (userId != requestingUserId)
            {
                if (!await _dbDriver.Service.Projects.HasPermissionAsync(projectId, requestingUserId,
                        ProjectPermissions.Administrator))
                {
                    return Unauthorized("You are not allowed to access this project, or lack the permissions to modify it");
                }
            }
        }
        
        if (await _dbDriver.Service.Projects.IsOwnerAsync(projectId, userId))
        {
            return BadRequest("The owner of the project cannot be deleted.");
        }
        
        if (!await _dbDriver.Service.Projects.HasUserAsync(projectId, userId))
        {
            return NotFound("The user is not part of this project.");
        }

        if (!await _dbDriver.Service.Projects.RemoveUserAsync(projectId, userId))
        {
            return StatusCode(503, "Error while attempting to delete database Object");
        }

        return Ok();
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    [HttpPost("LoadOwner")]
    public async Task<ActionResult<string>> LoadOwner()
    {
        var token = Request.Headers["authorization"];
        
        if (JwtTokenGenerator.IsApplicationToken(token))
        {
            return Unauthorized("Invalid Token type");
        }

        var requestingUserId = JwtTokenGenerator.GetOwnerKey(token);
        
        if (requestingUserId < 1)
        {
            return Unauthorized("The user is not authorized to access our system any longer");
        }
        
        var user = (PamUser) await _dbDriver.Service.Users.GetAsync(requestingUserId);

        if (user == null)
        {
            return Unauthorized("The user is not part of our system");
        }

        if (!await UserController.ValidateUserAccess(user))
        {
            return Unauthorized("The user is not authorized to access our system any longer");
        }

        var project = await GetProjectFromMethodBody();

        if (project == null)
        {
            return BadRequest("Method body is misshapen or malformed data");
        }

        if (!await _dbDriver.Service.Projects.ExistsAsync(project.Id))
        {
            return NotFound("Specified project does not exist.");
        }
        
        if (!user.Flags.HasFlag(UserFlags.PamaxieStaff))
        {
            if (!await _dbDriver.Service.Projects.HasPermissionAsync(project.Id, requestingUserId,
                    ProjectPermissions.Read))
            {
                return Unauthorized("You are not allowed to access this project, or lack the permissions to read it");
            }
        }

        project = (PamProject) await _dbDriver.Service.Projects.LoadOwnerAsync(project);
        
        if (project == null)
        {
            return StatusCode(503, "Error while attempting to load database Object");
        }

        return Ok(JsonConvert.SerializeObject(project));
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    [HttpPost("LoadLastModifiedUser")]
    public async Task<ActionResult<string>> LoadLastModifiedUser()
    {
        var token = Request.Headers["authorization"];
        
        if (JwtTokenGenerator.IsApplicationToken(token))
        {
            return Unauthorized("Invalid Token type");
        }

        var requestingUserId = JwtTokenGenerator.GetOwnerKey(token);
        
        if (requestingUserId < 1)
        {
            return Unauthorized("The user is not authorized to access our system any longer");
        }
        
        var user = (PamUser) await _dbDriver.Service.Users.GetAsync(requestingUserId);

        if (user == null)
        {
            return Unauthorized("The user is not part of our system");
        }

        if (!await UserController.ValidateUserAccess(user))
        {
            return Unauthorized("The user is not authorized to access our system any longer");
        }

        var project = await GetProjectFromMethodBody();

        if (project == null)
        {
            return BadRequest("Method body is misshapen or malformed data");
        }

        if (!await _dbDriver.Service.Projects.ExistsAsync(project.Id))
        {
            return NotFound("Specified project does not exist.");
        }
        
        if (!user.Flags.HasFlag(UserFlags.PamaxieStaff))
        {
            if (!await _dbDriver.Service.Projects.HasPermissionAsync(project.Id, requestingUserId,
                    ProjectPermissions.Read))
            {
                return Unauthorized("You are not allowed to access this project, or lack the permissions to read it");
            }
        }

        project = (PamProject) await _dbDriver.Service.Projects.LoadLastModifiedUserAsync(project);

        if (project == null)
        {
            return StatusCode(503, "Error while attempting to load database Object");
        }
        
        return Ok(JsonConvert.SerializeObject(project));
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    [HttpPost("LoadApiTokens")]
    public async Task<ActionResult<string>> LoadApiTokens()
    {
        var token = Request.Headers["authorization"];
        var requestingUserId = JwtTokenGenerator.GetOwnerKey(token);
        var user = (PamUser) await _dbDriver.Service.Users.GetAsync(requestingUserId);
        
        if (user == null)
        {
            return Unauthorized("The user is not part of our system");
        }
        
        if (!await UserController.ValidateUserAccess(user))
        {
            return Unauthorized(
                "The user does no longer have access to our systems. The user may be deleted or blocked by us internally.");
        }

        var project = await GetProjectFromMethodBody();

        if (project == null)
        {
            return BadRequest("Method body is misshapen or malformed data");
        }

        if (!await _dbDriver.Service.Projects.ExistsAsync(project.Id))
        {
            return NotFound("Specified project does not exist.");
        }
        
        if (!user.Flags.HasFlag(UserFlags.PamaxieStaff))
        {
            if (!await _dbDriver.Service.Projects.HasPermissionAsync(project.Id, requestingUserId,
                    ProjectPermissions.Read))
            {
                return Unauthorized("You are not allowed to access this project, or lack the permissions to read it");
            }
        }

        project = (PamProject) await _dbDriver.Service.Projects.LoadApiTokensAsync(project);
        
        if (project == null)
        {
            return StatusCode(503, "Error while attempting to load database Object");
        }

        return Ok(JsonConvert.SerializeObject(project));
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    [HttpPost("LoadUsers")]
    public async Task<ActionResult<string>> LoadUsers()
    {
        var token = Request.Headers["authorization"];
        var requestingUserId = JwtTokenGenerator.GetOwnerKey(token);
        var user = (PamUser) await _dbDriver.Service.Users.GetAsync(requestingUserId);
        
        if (user == null)
        {
            return Unauthorized("The user is not part of our system");
        }
        
        if (!await UserController.ValidateUserAccess(user))
        {
            return Unauthorized(
                "The user does no longer have access to our systems. The user may be deleted or blocked by us internally.");
        }

        var project = await GetProjectFromMethodBody();

        if (project == null)
        {
            return BadRequest("Method body is misshapen or malformed data");
        }

        if (!await _dbDriver.Service.Projects.ExistsAsync(project.Id))
        {
            return NotFound("Specified project does not exist.");
        }
        
        if (!user.Flags.HasFlag(UserFlags.PamaxieStaff))
        {
            if (!await _dbDriver.Service.Projects.HasPermissionAsync(project.Id, requestingUserId,
                    ProjectPermissions.Administrator))
            {
                return Unauthorized("You are not allowed to access this project, or lack the permissions to read it");
            }
        }

        project = (PamProject) await _dbDriver.Service.Projects.LoadUsersAsync(project);
        
        if (project == null)
        {
            return StatusCode(503, "Error while attempting to load database Object");
        }

        return Ok(JsonConvert.SerializeObject(project));
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    [HttpPost("LoadFully")]
    public async Task<ActionResult<string>> LoadFully()
    {
        var token = Request.Headers["authorization"];
        var requestingUserId = JwtTokenGenerator.GetOwnerKey(token);
        var user = (PamUser) await _dbDriver.Service.Users.GetAsync(requestingUserId);
        
        if (user == null)
        {
            return Unauthorized("The user is not part of our system");
        }
        
        if (!await UserController.ValidateUserAccess(user))
        {
            return Unauthorized(
                "The user does no longer have access to our systems. The user may be deleted or blocked by us internally.");
        }

        var project = await GetProjectFromMethodBody();

        if (project == null)
        {
            return BadRequest("Method body is misshapen or malformed data");
        }

        if (!await _dbDriver.Service.Projects.ExistsAsync(project.Id))
        {
            return NotFound("Specified project does not exist.");
        }
        
        if (!user.Flags.HasFlag(UserFlags.PamaxieStaff))
        {
            if (!await _dbDriver.Service.Projects.HasPermissionAsync(project.Id, requestingUserId,
                    ProjectPermissions.Administrator))
            {
                return Unauthorized("You are not allowed to access this project, or lack the permissions to read it");
            }
        }

        project = (PamProject) await _dbDriver.Service.Projects.LoadFullyAsync(project);
        
        
        if (project == null)
        {
            return StatusCode(503, "Error while attempting to fully load the database object");
        }

        return Ok(JsonConvert.SerializeObject(project));
    }

    private async Task<PamProject> GetProjectFromMethodBody()
    {
        using StreamReader reader = new StreamReader(HttpContext.Request.Body);
        var body = await reader.ReadToEndAsync();

        PamProject project;
        try
        {
            project = JsonConvert.DeserializeObject<PamProject>(body);
        }
        catch (JsonException)
        {
            return null;
        }

        return project;
    }
}