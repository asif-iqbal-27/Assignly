using Assignly.Application.Core.Abstractions;
using Assignly.Application.Dtos;
using Assignly.Application.Features.Assignments;
using Assignly.Application.Interfaces;
using Assignly.Domain.Entities;
using Assignly.Domain.Enums;
using ErrorOr;
using Microsoft.EntityFrameworkCore;

namespace Assignly.Application.Features.Submissions.Queries.GetSubmissionsForAssignment;

public sealed class GetSubmissionsForAssignmentQueryHandler : IQueryHandler<GetSubmissionsForAssignmentQuery, List<SubmissionDto>>
{
    private readonly IRepository<Assignment> _assignmentRepository;
    private readonly IRepository<ClassSubjectTeacher> _classSubjectTeacherRepository;
    private readonly IRepository<Submission> _submissionRepository;

    public GetSubmissionsForAssignmentQueryHandler(
        IRepository<Assignment> assignmentRepository,
        IRepository<ClassSubjectTeacher> classSubjectTeacherRepository,
        IRepository<Submission> submissionRepository)
    {
        _assignmentRepository = assignmentRepository;
        _classSubjectTeacherRepository = classSubjectTeacherRepository;
        _submissionRepository = submissionRepository;
    }

    public async Task<ErrorOr<List<SubmissionDto>>> Handle(GetSubmissionsForAssignmentQuery request, CancellationToken cancellationToken)
    {
        var assignment = await _assignmentRepository.GetByIdAsync(request.AssignmentId, cancellationToken);
        if (assignment is null)
        {
            return AssignmentErrors.NotFound(request.AssignmentId);
        }

        if (request.RequestingUserRole == RoleType.Teacher)
        {
            var isOwner = await _classSubjectTeacherRepository.Query()
                .AnyAsync(t => t.TeacherId == request.RequestingUserId && t.SubjectId == assignment.SubjectId, cancellationToken);

            if (!isOwner)
            {
                return SubmissionErrors.NotOwner;
            }
        }

        var latestAttempts = _submissionRepository.Query()
            .Where(s => s.AssignmentId == request.AssignmentId)
            .GroupBy(s => s.StudentId)
            .Select(g => new { StudentId = g.Key, MaxAttempt = g.Max(s => s.AttemptNumber) });

        var submissions = await (
            from s in _submissionRepository.Query()
            join m in latestAttempts
                on new { s.StudentId, s.AttemptNumber } equals new { m.StudentId, AttemptNumber = m.MaxAttempt }
            where s.AssignmentId == request.AssignmentId
            select s)
            .OrderBy(s => s.Student.FullName)
            .Select(SubmissionMappings.ToDto)
            .ToListAsync(cancellationToken);

        return submissions;
    }
}
