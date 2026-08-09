using Assignly.Application.Core.Abstractions;
using Assignly.Application.Dtos;
using Assignly.Application.Interfaces;
using Assignly.Domain.Entities;
using ErrorOr;
using Microsoft.AspNetCore.Identity;

namespace Assignly.Application.Features.Auth.Login;

public sealed class LoginCommandHandler : ICommandHandler<LoginCommand, AuthResponseDto>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IUserService _userService;

    public LoginCommandHandler(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IUserService userService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _userService = userService;
    }

    public async Task<ErrorOr<AuthResponseDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByNameAsync(request.UserName);
        if (user is null)
        {
            return Error.Unauthorized(description: "Invalid username or password.");
        }

        var signInResult = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: false);
        if (!signInResult.Succeeded)
        {
            return Error.Unauthorized(description: "Invalid username or password.");
        }

        var tokenResult = await _userService.GenerateTokenAsync(user);
        if (tokenResult.IsError)
        {
            return tokenResult.Errors;
        }

        var roleName = user.Role?.ToString() ?? "Student";
        return new AuthResponseDto
        {
            Token = tokenResult.Value,
            UserName = user.UserName ?? string.Empty,
            Role = roleName
        };
    }
}
