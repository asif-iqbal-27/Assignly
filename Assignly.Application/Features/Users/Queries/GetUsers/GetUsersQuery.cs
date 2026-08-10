using Assignly.Application.Core.Abstractions;
using Assignly.Application.Dtos;

namespace Assignly.Application.Features.Users.Queries.GetUsers;

public sealed record GetUsersQuery : IQuery<List<UserDto>>;
