using Assignly.Application.Core.Abstractions;
using Assignly.Application.Dtos;
using Assignly.Application.Interfaces;
using Assignly.Domain.Entities;
using ErrorOr;
using Microsoft.EntityFrameworkCore;

namespace Assignly.Application.Features.Users.Queries.GetUserById;

public sealed class GetUserByIdQueryHandler : IQueryHandler<GetUserByIdQuery, UserDto>
{
    private readonly IRepository<ApplicationUser> _userRepository;

    public GetUserByIdQueryHandler(IRepository<ApplicationUser> userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<ErrorOr<UserDto>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.Query()
            .Where(u => u.Id == request.Id)
            .Select(u => new UserDto
            {
                Id = u.Id,
                UserName = u.UserName ?? string.Empty,
                Email = u.Email ?? string.Empty,
                FullName = u.FullName,
                Role = u.Role != null ? u.Role.ToString()! : string.Empty,
                ClassId = u.ClassId,
                ClassName = u.Class != null ? u.Class.Name : null,
                IsActive = u.IsActive
            })
            .FirstOrDefaultAsync(cancellationToken);

        return user is null ? UserErrors.NotFound(request.Id) : user;
    }
}
