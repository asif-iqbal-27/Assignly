using Assignly.Application.Core.Abstractions;
using Assignly.Application.Dtos;
using Assignly.Domain.Enums;

namespace Assignly.Application.Features.Assignments.Queries.GetAssignmentById;

public sealed record GetAssignmentByIdQuery(
    Guid Id,
    Guid RequestingUserId,
    RoleType RequestingUserRole,
    Guid? RequestingUserClassId) : IQuery<AssignmentDto>;
