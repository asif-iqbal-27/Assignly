using Assignly.Application.Core.Abstractions;
using ErrorOr;

namespace Assignly.Application.Features.Users.Commands.DeactivateUser;

public sealed record DeactivateUserCommand(Guid Id) : ICommand<Updated>;
