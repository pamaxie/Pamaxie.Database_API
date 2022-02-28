using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pamaxie.Data;

namespace Pamaxie.Database.Api.Controllers;

/// <summary>
/// Api Controller for handling all <see cref="IPamProject"/> interactions
/// </summary>
[Authorize]
[ApiController]
[Route("[controller]")]
public sealed class ApplicationController : ControllerBase
{

}