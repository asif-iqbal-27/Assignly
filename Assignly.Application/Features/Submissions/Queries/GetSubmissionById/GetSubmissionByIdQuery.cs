using Assignly.Application.Core.Abstractions;
using Assignly.Application.Dtos;
using Assignly.Domain.Enums;

namespace Assignly.Application.Features.Submissions.Queries.GetSubmissionById;

public sealed record GetSubmissionByIdQuery(
    Guid Id,
    Guid RequestingUserId,
    RoleType RequestingUserRole) : IQuery<SubmissionDto>;
