using Assignly.Application.Core.Abstractions;
using Assignly.Application.Dtos;
using Assignly.Application.Interfaces;
using Assignly.Domain.Entities;
using ErrorOr;
using Microsoft.EntityFrameworkCore;

namespace Assignly.Application.Features.Submissions.Commands.SetSubmissionStatus;

public sealed class SetSubmissionStatusCommandHandler : ICommandHandler<SetSubmissionStatusCommand, SubmissionDto>
{
    private readonly IRepository<Submission> _submissionRepository;
    private readonly IRepository<Assignment> _assignmentRepository;
    private readonly IRepository<ClassSubjectTeacher> _classSubjectTeacherRepository;

    public SetSubmissionStatusCommandHandler(
        IRepository<Submission> submissionRepository,
        IRepository<Assignment> assignmentRepository,
        IRepository<ClassSubjectTeacher> classSubjectTeacherRepository)
    {
        _submissionRepository = submissionRepository;
        _assignmentRepository = assignmentRepository;
        _classSubjectTeacherRepository = classSubjectTeacherRepository;
    }

    public async Task<ErrorOr<SubmissionDto>> Handle(SetSubmissionStatusCommand request, CancellationToken cancellationToken)
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

        submission.Status = request.Status;

        await _submissionRepository.SaveChangesAsync(cancellationToken);

        return await _submissionRepository.Query()
            .Where(s => s.Id == submission.Id)
            .Select(SubmissionMappings.ToDto)
            .FirstAsync(cancellationToken);
    }
}
