using Assignly.Application.Core.Abstractions;
using Assignly.Domain.Entities;
using ErrorOr;
using Microsoft.AspNetCore.Identity;

namespace Assignly.Application.Features.Users.Commands.DeactivateUser;

public sealed class DeactivateUserCommandHandler : ICommandHandler<DeactivateUserCommand, Updated>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public DeactivateUserCommandHandler(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<ErrorOr<Updated>> Handle(DeactivateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.Id.ToString());
        if (user is null)
        {
            return UserErrors.NotFound(request.Id);
        }

        user.IsActive = false;
        await _userManager.UpdateAsync(user);

        return Result.Updated;
    }
}
