using Assignly.Application.Dtos;
using Assignly.Application.Features.Auth.Login;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Assignly.Host.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request, CancellationToken cancellationToken)
    {
        var command = new LoginCommand(request.UserName, request.Password);
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsError)
        {
            return BadRequest(new { errors = result.Errors });
        }

        return Ok(result.Value);
    }
}
