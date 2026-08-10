using System.Security.Claims;
using Assignly.Domain.Enums;
using ErrorOr;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Assignly.Host.Controllers;

[ApiController]
[Authorize]
public abstract class BaseApiController : ControllerBase
{
    protected Guid? UserIdOrNull =>
        Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

    protected Guid CurrentUserId =>
        UserIdOrNull ?? throw new UnauthorizedAccessException("Missing or invalid user id claim.");

    protected RoleType CurrentUserRole =>
        Enum.TryParse<RoleType>(User.FindFirstValue(ClaimTypes.Role), out var role)
            ? role
            : throw new UnauthorizedAccessException("Missing or invalid role claim.");

    protected Guid? CurrentUserClassId =>
        Guid.TryParse(User.FindFirstValue("class_id"), out var classId) ? classId : null;

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
            ErrorType.Unexpected => StatusCode(StatusCodes.Status500InternalServerError, body),
            ErrorType.Failure => StatusCode(StatusCodes.Status500InternalServerError, body),
            _ => StatusCode(StatusCodes.Status400BadRequest, body)
        };
    }
}
