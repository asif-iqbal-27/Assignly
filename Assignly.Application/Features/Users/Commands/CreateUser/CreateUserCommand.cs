using Assignly.Application.Core.Abstractions;
using Assignly.Application.Dtos;
using Assignly.Domain.Enums;

namespace Assignly.Application.Features.Users.Commands.CreateUser;

public sealed record CreateUserCommand(
    string UserName,
    string Email,
    string Password,
    string FullName,
    RoleType Role,
    Guid? ClassId) : ICommand<UserDto>;
