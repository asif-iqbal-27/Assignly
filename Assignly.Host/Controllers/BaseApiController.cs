using System.Security.Claims;
using ErrorOr;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Assignly.Host.Controllers;

[ApiController]
[Authorize]
public abstract class BaseApiController : ControllerBase
{
    protected Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    protected IActionResult HandleResult<T>(ErrorOr<T> result)
    {
        if (!result.IsError)
        {
            return Ok(result.Value);
        }

        var error = result.FirstError;
        var body = new { errors = result.Errors };

        return error.Type switch
        {
            ErrorType.Unauthorized => StatusCode(StatusCodes.Status401Unauthorized, body),
            ErrorType.Forbidden => StatusCode(StatusCodes.Status403Forbidden, body),
            ErrorType.NotFound => StatusCode(StatusCodes.Status404NotFound, body),
            ErrorType.Conflict => StatusCode(StatusCodes.Status409Conflict, body),
            ErrorType.Validation => StatusCode(StatusCodes.Status400BadRequest, body),
            _ => StatusCode(StatusCodes.Status400BadRequest, body)
        };
    }
}
