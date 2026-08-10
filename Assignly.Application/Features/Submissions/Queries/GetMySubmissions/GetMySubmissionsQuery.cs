using Assignly.Application.Core.Abstractions;
using Assignly.Application.Dtos;

namespace Assignly.Application.Features.Submissions.Queries.GetMySubmissions;

public sealed record GetMySubmissionsQuery(Guid StudentId) : IQuery<List<SubmissionDto>>;
