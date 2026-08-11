using Assignly.Application.Core.Abstractions;
using Assignly.Application.Dtos;
using Assignly.Application.Interfaces;
using Assignly.Domain.Entities;
using Assignly.Domain.Enums;
using ErrorOr;
using Microsoft.EntityFrameworkCore;

namespace Assignly.Application.Features.Submissions.Commands.GradeSubmission;

public sealed class GradeSubmissionCommandHandler : ICommandHandler<GradeSubmissionCommand, SubmissionDto>
{
    private readonly IRepository<Submission> _submissionRepository;
    private readonly IRepository<Assignment> _assignmentRepository;
    private readonly IRepository<ClassSubjectTeacher> _classSubjectTeacherRepository;

    public GradeSubmissionCommandHandler(
        IRepository<Submission> submissionRepository,
        IRepository<Assignment> assignmentRepository,
        IRepository<ClassSubjectTeacher> classSubjectTeacherRepository)
    {
        _submissionRepository = submissionRepository;
        _assignmentRepository = assignmentRepository;
        _classSubjectTeacherRepository = classSubjectTeacherRepository;
    }

    public async Task<ErrorOr<SubmissionDto>> Handle(GradeSubmissionCommand request, CancellationToken cancellationToken)
    {
        var submission = await _submissionRepository.GetByIdAsync(request.Id, cancellationToken);
        if (submission is null)
        {
            return SubmissionErrors.NotFound(request.Id);
        }

        var assignment = await _assignmentRepository.GetByIdAsync(submission.AssignmentId, cancellationToken);
        if (assignment is null)
        {
            return SubmissionErrors.AssignmentNotFound(submission.AssignmentId);
        }

        var isOwner = await _classSubjectTeacherRepository.Query()
            .AnyAsync(t => t.TeacherId == request.TeacherId && t.SubjectId == assignment.SubjectId, cancellationToken);

        if (!isOwner)
        {
            return SubmissionErrors.NotOwner;
        }

        if (request.Marks < 0 || request.Marks > assignment.MaxMarks)
        {
            return SubmissionErrors.MarksOutOfRange(assignment.MaxMarks);
        }

        submission.Marks = request.Marks;
        submission.Feedback = request.Feedback;
        submission.Status = SubmissionStatus.Graded;
        submission.GradedByTeacherId = request.TeacherId;
        submission.GradedAt = DateTime.UtcNow;

        await _submissionRepository.SaveChangesAsync(cancellationToken);

        var query = _submissionRepository.Query();
        var filteredQuery = query.Where(s => s.Id == submission.Id);
        var projectedQuery = filteredQuery.Select(SubmissionMappings.ToDto);

        return await projectedQuery.FirstAsync(cancellationToken);
    }
}
