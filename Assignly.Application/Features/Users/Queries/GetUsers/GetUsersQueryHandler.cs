using Assignly.Application.Core.Abstractions;
using Assignly.Application.Dtos;
using Assignly.Application.Interfaces;
using Assignly.Domain.Entities;
using ErrorOr;
using Microsoft.EntityFrameworkCore;

namespace Assignly.Application.Features.Users.Queries.GetUsers;

public sealed class GetUsersQueryHandler : IQueryHandler<GetUsersQuery, List<UserDto>>
{
    private readonly IRepository<ApplicationUser> _userRepository;

    public GetUsersQueryHandler(IRepository<ApplicationUser> userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<ErrorOr<List<UserDto>>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var query = _userRepository.Query();
        var orderedQuery = query.OrderBy(u => u.UserName);
        var projectedQuery = orderedQuery.Select(u => new UserDto
        {
            Id = u.Id,
            UserName = u.UserName ?? string.Empty,
            Email = u.Email ?? string.Empty,
            FullName = u.FullName,
            Role = u.Role != null ? u.Role.ToString()! : string.Empty,
            ClassId = u.ClassId,
            ClassName = u.Class != null ? u.Class.Name : null,
            IsActive = u.IsActive
        });

        var users = await projectedQuery.ToListAsync(cancellationToken);

        return users;
    }
}
