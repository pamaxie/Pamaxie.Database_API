using System;
using System.IO;
using System.Net.Mime;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CoenM.ImageHash.HashAlgorithms;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using Pamaxie.Authentication;
using Pamaxie.Data;
using Pamaxie.Database.Extensions;
using Pamaxie.Database.Native.NoSql;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Pamaxie.Database.Api.Controllers;

/// <summary>
/// Api Controller for handling all Scan interactions
/// </summary>
[Authorize]
[ApiController]
[Route("db/v1/[controller]")]
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
    /// Checks if we can connect with our database
    /// </summary>
    /// <returns></returns>
    [HttpGet("CanConnect")]
    [AllowAnonymous]
    public Task<ActionResult> CanConnect()
    {
        if (!_dbDriver.Service.IsDbConnected)
        {
            return Task.FromResult<ActionResult>(StatusCode(503, "We are having trouble connecting to our database severs."));
        }
        
        return Task.FromResult<ActionResult>(Ok());
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
        using StreamReader reader = new StreamReader(HttpContext.Request.Body);
        var body = await reader.ReadToEndAsync();

        if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Token"))
        {
            return Unauthorized();
        }

        var auth = await _dbDriver.Service.Projects.ValidateTokenAsync(authHeader.Substring("Token ".Length)?.Trim());
        if (!auth.wasSuccess)
        {
            return Unauthorized("Invalid API Access token.");
        }
        
        //TODO: Think about a concept for these scan GUIDs that makes sense, This currently jsut pretty much garbages up our database ATM so it's disabled.
        /*var scanMachine =
            await _dbDriver.Service.ScanMachines.CreateAsync(auth.projectId, auth.apiKeyId, Guid.NewGuid().ToString());

        if (!scanMachine.wasCreated)
        {
            return StatusCode(503,
                "Could not create scan machine GUID. Please try again or contract support if the issue persists.");
        }*/

        var jwtScanMachineSettings = new JwtScanMachineSettings()
            {IsScanMachine = true, ProjectId = auth.projectId, ScanMachineGuid = Guid.NewGuid().ToString()};

        var newToken = _generator.CreateToken(auth.apiKeyId, AppConfigManagement.JwtSettings, jwtScanMachineSettings, true);
        var items = new { Token = newToken, ProjectId = auth.projectId};
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
        
        using StreamReader reader = new StreamReader(HttpContext.Request.Body);
        var body = await reader.ReadToEndAsync();

        if (string.IsNullOrWhiteSpace(token))
        {
            return Unauthorized();
        }

        var projectId = JwtTokenGenerator.GetProjectId(token);
        var scanMachineGuid = JwtTokenGenerator.GetMachineGuid(token);

        if (projectId < 1 || string.IsNullOrWhiteSpace(scanMachineGuid))
        {
            return Unauthorized("Invalid jwt bearer token.");
        }
        
        //Create a new ScanMachine
        var (wasCreated, createdId) = await _dbDriver.Service.ScanMachines.CreateAsync(projectId, apiKeyId, scanMachineGuid);

        if (!wasCreated)
        {
            return StatusCode(503,
                "Could not create scan machine GUID. Please try again or contract support if the issue persists.");
        }

        var jwtScanMachineSettings = new JwtScanMachineSettings()
            {IsScanMachine = true, ProjectId = projectId, ScanMachineGuid = createdId};

        var newToken = _generator.CreateToken(apiKeyId, AppConfigManagement.JwtSettings, jwtScanMachineSettings, true);
        var items = new { Token = newToken, ProjectId = projectId};
        return Ok(JsonConvert.SerializeObject(items));
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
        using StreamReader reader = new StreamReader(HttpContext.Request.Body);
        var body = await reader.ReadToEndAsync();

        if (string.IsNullOrWhiteSpace(body))
        {
            return BadRequest("The method body is empty. Please Specify an object that should be created / updated");
        }

        PamScanData<PamImageScanResult> obj;
        try
        {
            obj = JsonConvert.DeserializeObject<PamScanData<PamImageScanResult>>(body);
        }
        catch (JsonException)
        {
            return BadRequest("The method body did not contain a scan result that could be deserialize");
        }

        if (obj?.Key == null || string.IsNullOrWhiteSpace(obj.Key))
        {
            return BadRequest("The scan result cannot be reached in because the Objects key is null");
        }

        if (IsValidMd5(obj.Key))
        {
            return BadRequest("The objects key is not a valid MD5 hash, we do not accept invalid hash data. Please" +
                              "hash the received data to receive your MD5 hash as a key for storing the object.");
        }

        if (!await _dbDriver.Service.Projects.IsPamProject(projectId))
        {
            return Unauthorized("You are not allowed to access this endpoint. Please use the scan endpoint instead.");
        }
        
        obj.ScanMachineGuid = scanMachine;

        if (!await _dbDriver.Service.Scans.ExistsAsync(obj.Key))
        {
            var (wasCreated, createdId) = await _dbDriver.Service.Scans.CreateAsync(obj);

            if (createdId == null)
            {
                return BadRequest("Invalid or unsupported Scan Data format");
            }
            
            return !wasCreated ? StatusCode(503, "The database object could not be created.") : Created("", createdId);
        }
        else
        {
            if (!await _dbDriver.Service.Scans.UpdateAsync(obj))
            {
                return BadRequest("Invalid or unsupported Scan Data format");
            }
            
            return Ok();
        }
    }
    
    /// <summary>
    /// Checks if a hash or a hash similar to the one provided exists
    /// </summary>
    /// <returns><see cref="JwtToken"/> Token for Authentication</returns>
    [HttpGet("Get={scanHash}")]
    public async Task<ActionResult> Exists(string scanHash)
    {
        var token = Request.Headers["authorization"];
        
        if (IsValidMd5(scanHash))
        {
            return BadRequest("The objects key is not a valid MD5 hash, we do not accept invalid hash data. Please make sure" +
                              "you possess the right hash to poll data.");
        }
        
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
            return Unauthorized("You are not allowed to get any scan data, if you are not part of Pamaxie's staff.");
        }

        if (string.IsNullOrWhiteSpace(scanHash))
        {
            return BadRequest("The token for deletion may not be empty.");
        }


        var existsAsync = await _dbDriver.Service.Scans.ExistsAsync(scanHash);
        if (!existsAsync)
        {
            var hammingSearch = await _dbDriver.Service.Scans.GetWithHammingDistance(scanHash, 0.88);

            if(hammingSearch == null){
                return NotFound();
            }
            
            return Ok(hammingSearch);
        }

        var scan = await _dbDriver.Service.Scans.GetSerializedData(scanHash);
        return Ok(scan);
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
            return Unauthorized("You are not allowed to delete any scan data, if you are not part of Pamaxie's staff.");
        }

        if (string.IsNullOrWhiteSpace(scanHash))
        {
            return BadRequest("The hash for deletion may not be empty.");
        }

        if (!await _dbDriver.Service.Scans.ExistsAsync(scanHash))
        {
            return NotFound();
        }

        var deleted = await _dbDriver.Service.Scans.DeleteAsync(scanHash);
        return !deleted ? StatusCode(503, "Encountered an error while deleting scan result") : Accepted();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    [HttpGet("CanAuthenticate")]
    public async Task<ActionResult> CanAuthenticate()
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

        return Ok("The current token seems valid to our system");
    }

    /// <summary>
    /// Validates if a token is an internal token.
    /// </summary>
    /// <returns></returns>
    [HttpGet("IsInternalToken")]
    public async Task<ActionResult> IsInternalToken(){
        var token = Request.Headers["authorization"];
        
        if (!JwtTokenGenerator.IsApplicationToken(token))
        {
            return Unauthorized("Invalid Token type");
        }

        var apiKeyId = JwtTokenGenerator.GetOwnerKey(token);
        
        if (apiKeyId < 1 || !await ValidateApiKeyAccess(apiKeyId))
        {
            return Unauthorized("The project is no longer authorized to access our system");
        }

        if (await _dbDriver.Service.Projects.IsPamProject(JwtTokenGenerator.GetProjectId(token)))
        {
            return Ok("The current token was recognized as a valid internal token");
        }

        return Unauthorized("The token was not recognized as a valid internal token. Please ensure you are using a valid token.");
    }

    /// <summary>
    /// Calculates the Perceptual hash for an Image
    /// </summary>
    /// <returns></returns>
    [HttpPost("GetImageHash")]
    public async Task<ActionResult> GetImageHash(){
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
            return Unauthorized("You are not allowed to delete any scan data, if you are not part of Pamaxie's staff.");
        }

        try{
            var hashAlgorithm = new AverageHash();
            using Image<Rgba32> image = Image.Load<Rgba32>(Request.BodyReader.AsStream());
            ulong imageHash = hashAlgorithm.Hash(image);
            return Ok(imageHash);
        }catch(UnknownImageFormatException){
            return BadRequest();
        }
    }
    
    private bool IsValidMd5(string s)
    {
        var regex = new Regex("^[a-fA-F0-9]{32}$");
        return regex.Match(s).Success;
    }
    
    private async Task<bool> ValidateApiKeyAccess(long apiKeyId)
    {
        return await _dbDriver.Service.Projects.IsTokenActiveAsync(apiKeyId);
    }
}