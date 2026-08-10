using Assignly.Application.Core.Abstractions;
using Assignly.Application.Dtos;
using Assignly.Domain.Enums;

namespace Assignly.Application.Features.Submissions.Queries.GetSubmissionsForAssignment;

public sealed record GetSubmissionsForAssignmentQuery(
    Guid AssignmentId,
    Guid RequestingUserId,
    RoleType RequestingUserRole) : IQuery<List<SubmissionDto>>;
