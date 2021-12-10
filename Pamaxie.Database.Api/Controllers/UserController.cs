using System.Collections.Generic;
using System.Net.Mime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Pamaxie.Authentication;
using Pamaxie.Data;
using Pamaxie.Database.Design;

namespace Pamaxie.Api.Controllers
{
    /// <summary>
    /// Api Controller for handling all <see cref="PamaxieUser"/> interactions
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public sealed class UserController : ControllerBase
    {

        private readonly IPamaxieDatabaseDriver _dbDriver;

        /// <summary>
        /// Constructor for <see cref="UserController"/>
        /// </summary>
        /// <param name="dbDriver">Driver for talking to the requested database service</param>
        public UserController(IPamaxieDatabaseDriver dbDriver)
        {
            _dbDriver = dbDriver;
        }

        /// <summary>
        /// Get a <see cref="PamaxieUser"/> from the database by a key
        /// </summary>
        /// <param name="key">Unique UniqueKey of the <see cref="PamaxieUser"/></param>
        /// <returns>A <see cref="PamaxieUser"/> from the database</returns>
        [Authorize]
        [HttpGet("Get={key}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PamaxieUser))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<PamaxieUser> GetTask(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return BadRequest();
            }

            if (!ValidateOwnership(key))
            {
                return Unauthorized();
            }

            if (!_dbDriver.Service.PamaxieApplicationData.Exists(key))
            {
                return NotFound();
            }

            return Ok(_dbDriver.Service.PamaxieUserData.Get(key));
        }

        /// <summary>
        /// Creates a new <see cref="PamaxieUser"/> in the database
        /// </summary>
        /// <param name="user"><see cref="PamaxieUser"/> to be created</param>
        /// <returns>Created <see cref="PamaxieUser"/></returns>
        [Authorize]
        [HttpPost("Create")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(PamaxieUser))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<PamaxieUser> CreateTask(PamaxieUser user)
        {
            return NotFound("This endpoint is not avialable for this data type, please use the Auth Controller to create a user.");
        }

        /// <summary>
        /// Tries to create a new <see cref="PamaxieUser"/> in the database
        /// </summary>
        /// <param name="user"><see cref="PamaxieUser"/> to be created</param>
        /// <returns>Created <see cref="PamaxieUser"/></returns>
        [Authorize]
        [HttpPost("TryCreate")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(PamaxieUser))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<PamaxieUser> TryCreateTask(PamaxieUser user)
        {
            return NotFound("This endpoint is not avialable for this data type");
        }

        /// <summary>
        /// Updates a <see cref="PamaxieUser"/> in the database
        /// </summary>
        /// <param name="user">Updated values on <see cref="PamaxieUser"/></param>
        /// <returns>Updated <see cref="PamaxieUser"/></returns>
        [Authorize]
        [HttpPut("Update")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PamaxieUser))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<PamaxieUser> UpdateTask(PamaxieUser user)
        {
            if (user == null)
            {
                return BadRequest();
            }

            if (string.IsNullOrWhiteSpace(user.UniqueKey))
            {
                return BadRequest();
            }

            if (!ValidateOwnership(user.UniqueKey))
            {
                return Unauthorized();
            }

            if (!_dbDriver.Service.PamaxieUserData.Exists(user.UniqueKey))
            {
                return NotFound("The user you want to update does not exist.");
            }

            return Ok(_dbDriver.Service.PamaxieUserData.Update(user));
        }

        /// <summary>
        /// Tries to update a <see cref="PamaxieUser"/> in the database
        /// </summary>
        /// <param name="user">Updated values on <see cref="PamaxieUser"/></param>
        /// <returns>Updated <see cref="PamaxieUser"/></returns>
        [Authorize]
        [HttpPut("TryUpdate")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PamaxieUser))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<PamaxieUser> TryUpdateTask(PamaxieUser user)
        {
            if (user == null)
            {
                return BadRequest();
            }

            if (!ValidateOwnership(user.UniqueKey))
            {
                return Unauthorized();
            }

            if (!_dbDriver.Service.PamaxieUserData.Exists(user.UniqueKey))
            {
                return NotFound("The user you want to update does not exist.");
            }

            if (_dbDriver.Service.PamaxieUserData.TryUpdate(user, out var updatedUser))
            {
                return Ok(updatedUser);
            }

            return Problem();
        }

        /// <summary>
        /// Tries to update a <see cref="PamaxieUser"/> in the database,
        /// if the <see cref="PamaxieUser"/> does not exist, then a new one will be created
        /// </summary>
        /// <param name="user">The <see cref="PamaxieUser"/> to be created, or updated values on <see cref="PamaxieUser"/></param>
        /// <returns>Updated or created <see cref="PamaxieUser"/></returns>
        [Authorize]
        [HttpPost("UpdateOrCreate")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PamaxieUser))]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(PamaxieUser))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<PamaxieUser> UpdateOrCreateTask(PamaxieUser user)
        {
            return NotFound("This endpoint is not avialable for this data type");
        }

        /// <summary>
        /// Checks if a <see cref="PamaxieUser"/> exists in the database
        /// TODO: This should probably not exist. Maybe check if this is required.
        /// </summary>
        /// <param name="key">Unique UniqueKey of the <see cref="PamaxieUser"/></param>
        /// <returns><see cref="bool"/> if <see cref="PamaxieUser"/> exists in the database</returns>
        [Authorize]
        [HttpGet("Exists={key}")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(bool))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<bool> ExistsTask(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return BadRequest();
            }

            if (!ValidateOwnership(key))
            {
                return Unauthorized();
            }

            return Ok(_dbDriver.Service.PamaxieUserData.Exists(key));
        }

        /// <summary>
        /// Deletes a <see cref="PamaxieUser"/> in the database
        /// </summary>
        /// <param name="user"><see cref="PamaxieUser"/> to be deleted</param>
        /// <returns><see cref="bool"/> if <see cref="PamaxieUser"/> is deleted</returns>
        [Authorize]
        [HttpDelete("Delete")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(bool))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<bool> DeleteTask(PamaxieUser user)
        {
            if (user == null)
            {
                return BadRequest();
            }

            if (string.IsNullOrWhiteSpace(user.UniqueKey))
            {
                return BadRequest();
            }

            if (!ValidateOwnership(user.UniqueKey))
            {
                return Unauthorized();
            }

            if (!_dbDriver.Service.PamaxieApplicationData.Exists(user.UniqueKey))
            {
                return NotFound();
            }

            if (_dbDriver.Service.PamaxieUserData.Delete(user))
            {
                return Ok(true);
            }

            return Problem();
        }

        /// <summary>
        /// Get all <see cref="PamaxieApplication"/>s the user owns
        /// </summary>
        /// <param name="userId">The user to get the applications from</param>
        /// <returns>A list of all <see cref="PamaxieApplication"/>s the <see cref="PamaxieUser"/> owns</returns>
        [Authorize]
        [HttpGet("GetAllApplications={userId}")]

        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<PamaxieApplication>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<IEnumerable<PamaxieApplication>> GetAllApplicationsTask(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return BadRequest();
            }

            if (ValidateOwnership(userId))
            {
                return Unauthorized();
            }

            if (!_dbDriver.Service.PamaxieUserData.Exists(userId))
            {
                return NotFound();
            }

            var user = _dbDriver.Service.PamaxieUserData.Get(userId);
            return Ok(_dbDriver.Service.PamaxieUserData.GetAllApplications(user));
        }

        /// <summary>
        /// Verifies the <see cref="PamaxieUser"/>'s email address
        /// </summary>
        /// <param name="userId">The user to get the email from and to verify</param>
        /// <returns><see cref="bool"/> if the user got verified</returns>
        [Authorize]
        [HttpPost("VerifyEmail")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(bool))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<bool> VerifyEmailTask(string userId)
        {
            if (userId == null)
            {
                return BadRequest();
            }

            if (ValidateOwnership(userId))
            {
                return Unauthorized();
            }

            var user = _dbDriver.Service.PamaxieUserData.Get(userId);

            if (user == null)
            {
                return BadRequest();
            }

            if (_dbDriver.Service.PamaxieUserData.VerifyEmail(user))
            {
                return Ok(true);
            }

            return Problem();
        }


        /// <summary>
        /// Validates a user owns this resource to modify it
        /// </summary>
        /// <param name="assumedUserId"></param>
        /// <returns></returns>
        private bool ValidateOwnership(string assumedUserId)
        {
            string token = Request.Headers["authorization"];
            string userId = JwtTokenGenerator.GetUserKey(token);
            return assumedUserId == userId;
        }
    }
}