using Assignly.Application.Core.Abstractions;
using Assignly.Application.Dtos;
using Assignly.Domain.Enums;

namespace Assignly.Application.Features.Assignments.Queries.GetAssignments;

public sealed record GetAssignmentsQuery(
    Guid RequestingUserId,
    RoleType RequestingUserRole,
    Guid? RequestingUserClassId) : IQuery<List<AssignmentDto>>;
