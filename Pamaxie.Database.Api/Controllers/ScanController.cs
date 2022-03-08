using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Pamaxie.Authentication;
using Pamaxie.Data;
using Pamaxie.Database.Extensions;

namespace Pamaxie.Database.Api.Controllers;

/// <summary>
/// Api Controller for handling all Scan interactions
/// </summary>
[Authorize]
[ApiController]
[Route("[controller]")]
public sealed class ScanController : ControllerBase
{

    private readonly JwtTokenGenerator _generator;
    private readonly IPamaxieDatabaseDriver _dbDriver;

    /// <summary>
    /// Constructor for <see cref="UserController"/>
    /// </summary>
    /// <param name="generator">Used for generating Jwt tokens for a user</param>
    /// <param name="dbDriver">Driver for talking to the requested database service</param>
    public ScanController(JwtTokenGenerator generator, IPamaxieDatabaseDriver dbDriver)
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

        if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Token"))
        {
            return Unauthorized(authHeader ?? string.Empty);
        }

        var auth = await _dbDriver.Service.Projects.ValidateTokenAsync(authHeader.Substring("Token ".Length).Trim());
        if (auth.wasSuccess)
        {
            return Unauthorized("Invalid API Access token.");
        }
        
        //Create a new ScanMachine
        var scanMachine =
            await _dbDriver.Service.ScanMachines.CreateAsync(auth.projectId, auth.apiKeyId, Guid.NewGuid().ToString());

        if (!scanMachine.wasCreated)
        {
            return StatusCode(503,
                "Could not create scan machine GUID. Please try again or contract support if the issue persists.");
        }

        var jwtScanMachineSettings = new JwtScanMachineSettings()
            {IsScanMachine = true, ProjectId = auth.projectId, ScanMachineGuid = scanMachine.createdId};

        var newToken = _generator.CreateToken(auth.apiKeyId, AppConfigManagement.JwtSettings, jwtScanMachineSettings, longLivedToken);
        var items = new { Token = newToken, ProjectId = auth};
        return Ok(JsonConvert.SerializeObject(items));
    }
    
    /// <summary>
    /// Signs in a user via Basic authentication and returns a token.
    /// </summary>
    /// <returns><see cref="JwtToken"/> Token for Authentication</returns>
    [HttpGet("RefreshToken")]
    public async Task<ActionResult<JwtToken>> UpdateTokenTask()
    {
        var token = Request.Headers["authorization"];

        if (!JwtTokenGenerator.IsApplicationToken(token))
        {
            return Unauthorized("Invalid Token type");
        }

        var apiKeyId = JwtTokenGenerator.GetOwnerKey(token);
        
        if (apiKeyId < 1 || !await ValidateApiKeyAccess(apiKeyId))
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

        var projectId = JwtTokenGenerator.GetProjectId(token);
        var scanMachine = JwtTokenGenerator.GetMachineGuid(token);

        if (projectId < 1 || string.IsNullOrWhiteSpace(scanMachine))
        {
            return Unauthorized("The projectId or scanMachineGuid in the token could not be read");
        }
        
        var jwtScanMachineSettings = new JwtScanMachineSettings()
            {IsScanMachine = true, ProjectId = projectId, ScanMachineGuid = scanMachine};
        var newToken = _generator.CreateToken(apiKeyId, AppConfigManagement.JwtSettings, jwtScanMachineSettings, longLivedToken);
        return newToken;
    }
    
    /// <summary>
    /// Signs in a user via Basic authentication and returns a token.
    /// </summary>
    /// <returns><see cref="JwtToken"/> Token for Authentication</returns>
    [HttpPost("Update")]
    public async Task<ActionResult> UpdateTask()
    {
        var token = Request.Headers["authorization"];
        
        if (!JwtTokenGenerator.IsApplicationToken(token))
        {
            return Unauthorized("Invalid Token type");
        }

        var apiKeyId = JwtTokenGenerator.GetOwnerKey(token);
        
        if (apiKeyId < 1 || !await ValidateApiKeyAccess(apiKeyId))
        {
            return Unauthorized("The user is not authorized to access our system any longer");
        }
        
        var projectId = JwtTokenGenerator.GetProjectId(token);
        var scanMachine = JwtTokenGenerator.GetMachineGuid(token);
        
        bool longLivedToken = false;
        using StreamReader reader = new StreamReader(HttpContext.Request.Body);
        var body = await reader.ReadToEndAsync();

        if (string.IsNullOrWhiteSpace(body))
        {
            return BadRequest("The method body is empty. Please Specify an object that should be created / updated");
        }

        PamScanData<IPamNoSqlObject> obj;
        try
        {
            obj = JsonConvert.DeserializeObject<PamScanData<IPamNoSqlObject>>(body);
        }
        catch (JsonException ex)
        {
            return BadRequest("The method body did not contain a scan result that could be deserialize");
        }

        if (obj == null ||obj.Key == null)
        {
            return BadRequest("The scan result cannot be reached in because the Objects key is null");
        }

        obj.IsUserScan = !await _dbDriver.Service.Projects.IsPamProject(projectId);
        obj.ScanMachineGuid = scanMachine;

        if (!await _dbDriver.Service.Scans.ExistsAsync(obj.Key))
        {
            var createdItem = await _dbDriver.Service.Scans.CreateAsync(obj);
            if (!createdItem.wasCreated)
            {
                return StatusCode(503, "The database object could not be created.");
            }

            return Created("", createdItem.createdId);
        }
        else
        {
            if (!await _dbDriver.Service.Scans.UpdateAsync(obj))
            {
                return StatusCode(503, "The database object could not be created.");
            }
            
            return Ok();
        }
    }
    
    /// <summary>
    /// Signs in a user via Basic authentication and returns a token.
    /// </summary>
    /// <returns><see cref="JwtToken"/> Token for Authentication</returns>
    [HttpDelete("Get={scanHash}")]
    public async Task<ActionResult> Exists(string scanHash)
    {
        var token = Request.Headers["authorization"];
        
        if (!JwtTokenGenerator.IsApplicationToken(token))
        {
            return Unauthorized("Invalid Token type");
        }

        var apiKeyId = JwtTokenGenerator.GetOwnerKey(token);
        
        if (apiKeyId < 1 || !await ValidateApiKeyAccess(apiKeyId))
        {
            return Unauthorized("The user is not authorized to access our system any longer");
        }
        
        var projectId = JwtTokenGenerator.GetProjectId(token);
        var scanMachine = JwtTokenGenerator.GetMachineGuid(token);

        if (!await _dbDriver.Service.Projects.IsPamProject(projectId))
        {
            return Unauthorized("You are not allowed to delete any scan data, if you are not part of Pamaxies staff.");
        }

        if (string.IsNullOrWhiteSpace(token))
        {
            return BadRequest("The token for deletion may not be empty.");
        }


        var existsAsync = await _dbDriver.Service.Scans.ExistsAsync(token);
        if (!existsAsync)
        {
            return NotFound();
        }

        var scan = await _dbDriver.Service.Scans.GetAsync(token);
        return Ok(JsonConvert.SerializeObject(scan));
    }
    
    /// <summary>
    /// Signs in a user via Basic authentication and returns a token.
    /// </summary>
    /// <returns><see cref="JwtToken"/> Token for Authentication</returns>
    [HttpDelete("Delete={scanHash}")]
    public async Task<ActionResult> DeleteTask(string scanHash)
    {
        var token = Request.Headers["authorization"];
        
        if (!JwtTokenGenerator.IsApplicationToken(token))
        {
            return Unauthorized("Invalid Token type");
        }

        var apiKeyId = JwtTokenGenerator.GetOwnerKey(token);
        
        if (apiKeyId < 1 || !await ValidateApiKeyAccess(apiKeyId))
        {
            return Unauthorized("The user is not authorized to access our system any longer");
        }
        
        var projectId = JwtTokenGenerator.GetProjectId(token);
        var scanMachine = JwtTokenGenerator.GetMachineGuid(token);

        if (!await _dbDriver.Service.Projects.IsPamProject(projectId))
        {
            return Unauthorized("You are not allowed to delete any scan data, if you are not part of Pamaxies staff.");
        }

        if (string.IsNullOrWhiteSpace(token))
        {
            return BadRequest("The token for deletion may not be empty.");
        }


        var deleted = await _dbDriver.Service.Scans.DeleteAsync(token);

        if (!deleted)
        {
            return StatusCode(503, "Encountered an error while deleting scan result");
        }

        return Accepted();
    }

    public async Task<bool> ValidateApiKeyAccess(long apiKeyId)
    {
        return await _dbDriver.Service.Projects.IsTokenActiveAsync(apiKeyId);
    }
}