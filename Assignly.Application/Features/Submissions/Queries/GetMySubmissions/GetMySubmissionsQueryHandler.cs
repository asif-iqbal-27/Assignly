using Assignly.Application.Core.Abstractions;
using Assignly.Application.Dtos;
using Assignly.Application.Interfaces;
using Assignly.Domain.Entities;
using ErrorOr;
using Microsoft.EntityFrameworkCore;

namespace Assignly.Application.Features.Submissions.Queries.GetMySubmissions;

public sealed class GetMySubmissionsQueryHandler : IQueryHandler<GetMySubmissionsQuery, List<SubmissionDto>>
{
    private readonly IRepository<Submission> _submissionRepository;

    public GetMySubmissionsQueryHandler(IRepository<Submission> submissionRepository)
    {
        _submissionRepository = submissionRepository;
    }

    public async Task<ErrorOr<List<SubmissionDto>>> Handle(GetMySubmissionsQuery request, CancellationToken cancellationToken)
    {
        var mySubmissionsQuery = _submissionRepository.Query()
            .Where(s => s.StudentId == request.StudentId);

        var latestSubmissionsQuery = mySubmissionsQuery.Where(s => !mySubmissionsQuery.Any(other =>
            other.AssignmentId == s.AssignmentId && other.AttemptNumber > s.AttemptNumber));

        var orderedQuery = latestSubmissionsQuery.OrderByDescending(s => s.SubmittedAt);
        var projectedQuery = orderedQuery.Select(SubmissionMappings.ToDto);

        var submissions = await projectedQuery.ToListAsync(cancellationToken);
        
        return submissions;
    }
}
