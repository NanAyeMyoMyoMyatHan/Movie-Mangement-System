using Microsoft.AspNetCore.Mvc;
using Common;

namespace DotNetProject.WebApi.Features.Shared
{
    [ApiController]
    public abstract class BaseController : ControllerBase
    {
        protected IActionResult ToActionResult<T>(Result<T> result)
        {
            if (result.IsSuccess)
                return Ok(result);

            if (result.IsValidationError || result.IsDataError)
                return BadRequest(result);

            if (result.IsNotFound)
                return NotFound(result);

            if (result.IsDuplicateRecord || result.IsInvalidData)
                return Conflict(result);

            if (result.IsSystemError)
                return StatusCode(500, result);

            return BadRequest(result);
        }
    }
}
