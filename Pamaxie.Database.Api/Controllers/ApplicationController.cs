using System.Net.Mime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Pamaxie.Authentication;
using Pamaxie.Data;
using Pamaxie.Database.Design;


namespace Pamaxie.Api.Controllers
{
    /// <summary>
    /// Api Controller for handling all <see cref="PamaxieApplication"/> interactions
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public sealed class ApplicationController : ControllerBase
    {

        private readonly IPamaxieDatabaseDriver _dbDriver;

        /// <summary>
        /// Constructor for <see cref="ApplicationController"/>
        /// </summary>
        /// <param name="dbDriver">Database Service</param>
        public ApplicationController(IPamaxieDatabaseDriver dbDriver)
        {
            _dbDriver = dbDriver;
        }

        /// <summary>
        /// Gets a <see cref="PamaxieApplication"/> from the database by a key
        /// </summary>
        /// <param name="applicationId">Unique UniqueKey of the <see cref="PamaxieApplication"/></param>
        /// <returns>A <see cref="PamaxieApplication"/> from the database</returns>
        [Authorize]
        [HttpGet("Get={key}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PamaxieApplication))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<PamaxieApplication> GetTask(string applicationId)
        {

            if (string.IsNullOrWhiteSpace(applicationId))
            {
                return BadRequest();
            }

            if (string.IsNullOrEmpty(applicationId))
            {
                return BadRequest();
            }

            if (!ValidateOwnership(applicationId))
            {
                return Unauthorized();
            }

            if (!_dbDriver.Service.PamaxieApplicationData.Exists(applicationId))
            {
                return NotFound();
            }

            return Ok(_dbDriver.Service.PamaxieApplicationData.Get(applicationId));
        }

        /// <summary>
        /// Creates a new <see cref="PamaxieApplication"/> in the database
        /// </summary>
        /// <param name="application">The <see cref="PamaxieApplication"/> to be created</param>
        /// <returns>Created <see cref="PamaxieApplication"/></returns>
        [Authorize]
        [HttpPost("Create")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(PamaxieApplication))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<PamaxieApplication> CreateTask(PamaxieApplication application)
        {
            if (application == null)
            {
                return BadRequest();
            }

            if (string.IsNullOrEmpty(application.OwnerKey))
            {
                return BadRequest();
            }


            string token = Request.Headers["authorization"];
            string userId = JwtTokenGenerator.GetUserKey(token);

            if (application.OwnerKey != userId)
            {
                return BadRequest();
            }

            return Created("", _dbDriver.Service.PamaxieApplicationData.Create(application));
        }

        /// <summary>
        /// Tries to create a new <see cref="PamaxieApplication"/> in the database
        /// </summary>
        /// <param name="application">The <see cref="PamaxieApplication"/> to be created</param>
        /// <returns>Created <see cref="PamaxieApplication"/></returns>
        [Authorize]
        [HttpPost("TryCreate")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(PamaxieApplication))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<PamaxieApplication> TryCreateTask(PamaxieApplication application)
        {
            if (application == null)
            {
                return BadRequest();
            }

            if (string.IsNullOrEmpty(application.OwnerKey))
            {
                return BadRequest();
            }


            string token = Request.Headers["authorization"];
            string userId = JwtTokenGenerator.GetUserKey(token);

            if (application.OwnerKey != userId)
            {
                return BadRequest();
            }

            if (_dbDriver.Service.PamaxieApplicationData.TryCreate(application, out PamaxieApplication createdApplication))
            {
                return Created("", createdApplication);
            }

            return Problem();
        }

        /// <summary>
        /// Updates a <see cref="PamaxieApplication"/> in the database
        /// </summary>
        /// <param name="application">Updated values on <see cref="PamaxieApplication"/></param>
        /// <returns>Updated <see cref="PamaxieApplication"/></returns>
        [Authorize]
        [HttpPut("Update")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PamaxieApplication))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<PamaxieApplication> UpdateTask(PamaxieApplication application)
        {
            if (application == null)
            {
                return BadRequest();
            }

            if (string.IsNullOrEmpty(application.UniqueKey) || string.IsNullOrEmpty(application.OwnerKey))
            {
                return BadRequest();
            }

            if (!ValidateOwnership(application.UniqueKey))
            {
                return Unauthorized();
            }

            return Ok(_dbDriver.Service.PamaxieApplicationData.Update(application));
        }

        /// <summary>
        /// Tries to update a <see cref="PamaxieApplication"/> in the database
        /// </summary>
        /// <param name="application">Updated values on <see cref="PamaxieApplication"/></param>
        /// <returns>Updated <see cref="PamaxieApplication"/></returns>
        [Authorize]
        [HttpPut("TryUpdate")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PamaxieApplication))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<PamaxieApplication> TryUpdateTask(PamaxieApplication application)
        {
            if (application == null)
            {
                return BadRequest();
            }

            if (string.IsNullOrEmpty(application.UniqueKey) || string.IsNullOrEmpty(application.OwnerKey))
            {
                return BadRequest();
            }

            if (!ValidateOwnership(application.UniqueKey))
            {
                return Unauthorized();
            }

            if (_dbDriver.Service.PamaxieApplicationData.TryUpdate(application, out PamaxieApplication updatedApplication))
            {
                return Ok(updatedApplication);
            }

            return Problem();
        }

        /// <summary>
        /// Tries to update a <see cref="PamaxieApplication"/> in the database,
        /// if the <see cref="PamaxieApplication"/> does not exist, then a new one will be created. 
        /// This requires the UniqueKey of the application to be not set. Otherwise you will get an authentication error.
        /// </summary>
        /// <param name="application">The <see cref="PamaxieApplication"/> to be created, or updated values on <see cref="PamaxieApplication"/></param>
        /// <returns>Updated or created <see cref="PamaxieApplication"/></returns>
        [Authorize]
        [HttpPost("UpdateOrCreate")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PamaxieApplication))]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(PamaxieApplication))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<PamaxieApplication> UpdateOrCreateTask(PamaxieApplication application)
        {
            if (application == null)
            {
                return BadRequest();
            }

            if (string.IsNullOrEmpty(application.OwnerKey))
            {
                return BadRequest();
            }

            if (string.IsNullOrEmpty(application.UniqueKey))
            {
                string token = Request.Headers["authorization"];
                string userId = JwtTokenGenerator.GetUserKey(token);

                if (application.OwnerKey != userId)
                {
                    return BadRequest();
                }
            }
            else
            {
                ValidateOwnership(application.UniqueKey);
            }

            if (_dbDriver.Service.PamaxieApplicationData.UpdateOrCreate(application, out PamaxieApplication updatedOrCreatedApplication))
            {
                return Created("", updatedOrCreatedApplication);
            }

            return Ok(updatedOrCreatedApplication);
        }

        /// <summary>
        /// Checks if a <see cref="PamaxieApplication"/> exists in the database
        /// </summary>
        /// <param name="applicationId">Unique UniqueKey of the <see cref="PamaxieApplication"/></param>
        /// <returns><see cref="bool"/> if <see cref="PamaxieApplication"/> exists in the database</returns>
        [Authorize]
        [HttpGet("Exists={key}")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(bool))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<bool> ExistsTask(string applicationId)
        {
            if (string.IsNullOrWhiteSpace(applicationId))
            {
                return BadRequest();
            }

            if (string.IsNullOrEmpty(applicationId))
            {
                return BadRequest();
            }

            if (!ValidateOwnership(applicationId))
            {
                return Unauthorized();
            }

            return Ok(_dbDriver.Service.PamaxieApplicationData.Exists(applicationId));
        }

        /// <summary>
        /// Deletes a <see cref="PamaxieApplication"/> in the database
        /// </summary>
        /// <param name="applicationId">The Id of the application that should be deleted</param>
        /// <returns><see cref="bool"/> if <see cref="PamaxieApplication"/> is deleted</returns>
        [Authorize]
        [HttpDelete("Delete")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(bool))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<bool> DeleteTask(string applicationId)
        {
            if (applicationId == null)
            {
                return BadRequest();
            }

            if (string.IsNullOrWhiteSpace(applicationId))
            {
                return BadRequest();
            }

            if (!ValidateOwnership(applicationId))
            {
                return Unauthorized();
            }

            var application = _dbDriver.Service.PamaxieApplicationData.Get(applicationId);

            if (application == null)
            {
                return NotFound();
            }


            if (_dbDriver.Service.PamaxieApplicationData.Delete(application))
            {
                return Ok(true);
            }

            return Problem();
        }

        /// <summary>
        /// Gets the owner from a <see cref="PamaxieApplication"/>
        /// </summary>
        /// <param name="application">The <see cref="PamaxieApplication"/> to get owner from</param>
        /// <returns>The owner of the <see cref="PamaxieApplication"/></returns>
        [Authorize]
        [HttpGet("GetOwner")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PamaxieUser))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<PamaxieUser> GetOwner(PamaxieApplication application)
        {
            if (application == null)
            {
                return BadRequest();
            }

            if (string.IsNullOrEmpty(application.UniqueKey) || string.IsNullOrEmpty(application.OwnerKey))
            {
                return BadRequest();
            }

            if (!ValidateOwnership(application.UniqueKey))
            {
                return Unauthorized();
            }

            return Ok(_dbDriver.Service.PamaxieApplicationData.GetOwner(application));
        }

        /// <summary>
        /// Enables or disables the <see cref="PamaxieApplication"/> 
        /// </summary>
        /// <param name="applicationId">Id of the application that should be enabled or disabled</param>
        /// <returns>Enabled or disabled <see cref="PamaxieApplication"/></returns>
        [Authorize]
        [HttpPut("EnableOrDisable")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PamaxieApplication))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<PamaxieApplication> EnableOrDisableTask(string applicationId)
        {
            if (applicationId == null)
            {
                return BadRequest();
            }

            if (string.IsNullOrEmpty(applicationId))
            {
                return BadRequest();
            }

            if (!ValidateOwnership(applicationId))
            {
                return Unauthorized();
            }

            var application = _dbDriver.Service.PamaxieApplicationData.Get(applicationId);

            if (application == null)
            {
                return NotFound();
            }

            return Ok(_dbDriver.Service.PamaxieApplicationData.EnableOrDisable(application));
        }

        /// <summary>
        /// Verify if the <see cref="AppAuthCredentials"/> is authorized from a <see cref="PamaxieApplication"/>
        /// </summary>
        /// <param name="application">Application to verify authentication</param>
        /// <returns><see cref="bool"/> if the <see cref="PamaxieApplication"/>is authorized</returns>
        [Authorize]
        [HttpPost("VerifyAuthentication")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(bool))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<bool> VerifyAuthenticationTask(PamaxieApplication application)
        {
            if (application == null)
            {
                return BadRequest();
            }

            if (string.IsNullOrEmpty(application.UniqueKey) || string.IsNullOrEmpty(application.OwnerKey))
            {
                return BadRequest();
            }

            if (!ValidateOwnership(application.UniqueKey))
            {
                return Unauthorized();
            }

            if (!_dbDriver.Service.PamaxieApplicationData.Exists(application.UniqueKey))
            {
                return NotFound();
            }

            if (_dbDriver.Service.PamaxieApplicationData.VerifyAuthentication(application))
            {
                return Ok(true);
            }

            return Unauthorized();
        }

        /// <summary>
        /// Validates if the person making changes to an application is its actual owner
        /// </summary>
        /// <param name="applicationId"></param>
        /// <returns>If the user owns the application. Defaults to false if the application could not be found.</returns>
        private bool ValidateOwnership(string applicationId)
        {
            var application = _dbDriver.Service.PamaxieApplicationData.Get(applicationId);

            if (application == null)
            {
                return false;
            }

            string token = Request.Headers["authorization"];
            string userId = JwtTokenGenerator.GetUserKey(token);
            return application.OwnerKey == userId;
        }
    }
}