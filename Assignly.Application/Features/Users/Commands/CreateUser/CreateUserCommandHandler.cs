using Assignly.Application.Core.Abstractions;
using Assignly.Application.Dtos;
using Assignly.Application.Interfaces;
using Assignly.Domain.Entities;
using Assignly.Domain.Enums;
using ErrorOr;
using Microsoft.AspNetCore.Identity;

namespace Assignly.Application.Features.Users.Commands.CreateUser;

public sealed class CreateUserCommandHandler : ICommandHandler<CreateUserCommand, UserDto>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IRepository<SchoolClass> _classRepository;

    public CreateUserCommandHandler(UserManager<ApplicationUser> userManager, IRepository<SchoolClass> classRepository)
    {
        _userManager = userManager;
        _classRepository = classRepository;
    }

    public async Task<ErrorOr<UserDto>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var classId = request.Role == RoleType.Student ? request.ClassId : null;

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = request.UserName,
            Email = request.Email,
            FullName = request.FullName,
            Role = request.Role,
            ClassId = classId
        };

        var createResult = await _userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
        {
            var description = string.Join(" ", createResult.Errors.Select(e => e.Description));
            return UserErrors.CreateFailed(description);
        }

        await _userManager.AddToRoleAsync(user, request.Role.ToString());

        string? className = null;
        if (classId is not null)
        {
            var schoolClass = await _classRepository.GetByIdAsync(classId.Value, cancellationToken);
            className = schoolClass?.Name;
        }

        return new UserDto
        {
            Id = user.Id,
            UserName = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            FullName = user.FullName,
            Role = request.Role.ToString(),
            ClassId = user.ClassId,
            ClassName = className,
            IsActive = true
        };
    }
}
