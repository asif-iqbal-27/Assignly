using Assignly.Application.Core.Abstractions;
using Assignly.Application.Dtos;

namespace Assignly.Application.Features.Auth.Login;

public sealed record LoginCommand(string UserName, string Password) : ICommand<AuthResponseDto>;
