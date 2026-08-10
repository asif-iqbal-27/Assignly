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
        var latestAttempts = _submissionRepository.Query()
            .Where(s => s.StudentId == request.StudentId)
            .GroupBy(s => s.AssignmentId)
            .Select(g => new { AssignmentId = g.Key, MaxAttempt = g.Max(s => s.AttemptNumber) });

        var submissions = await (
            from s in _submissionRepository.Query()
            join m in latestAttempts
                on new { s.AssignmentId, s.AttemptNumber } equals new { m.AssignmentId, AttemptNumber = m.MaxAttempt }
            where s.StudentId == request.StudentId
            select s)
            .OrderByDescending(s => s.SubmittedAt)
            .Select(SubmissionMappings.ToDto)
            .ToListAsync(cancellationToken);

        return submissions;
    }
}
