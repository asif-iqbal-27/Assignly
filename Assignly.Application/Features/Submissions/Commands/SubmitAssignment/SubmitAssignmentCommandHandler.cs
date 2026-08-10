using Assignly.Application.Core.Abstractions;
using Assignly.Application.Dtos;
using Assignly.Application.Interfaces;
using Assignly.Domain.Entities;
using Assignly.Domain.Enums;
using ErrorOr;
using Microsoft.EntityFrameworkCore;

namespace Assignly.Application.Features.Submissions.Commands.SubmitAssignment;

public sealed class SubmitAssignmentCommandHandler : ICommandHandler<SubmitAssignmentCommand, SubmissionDto>
{
    private readonly IRepository<Assignment> _assignmentRepository;
    private readonly IRepository<ApplicationUser> _userRepository;
    private readonly IRepository<Submission> _submissionRepository;

    public SubmitAssignmentCommandHandler(
        IRepository<Assignment> assignmentRepository,
        IRepository<ApplicationUser> userRepository,
        IRepository<Submission> submissionRepository)
    {
        _assignmentRepository = assignmentRepository;
        _userRepository = userRepository;
        _submissionRepository = submissionRepository;
    }

    public async Task<ErrorOr<SubmissionDto>> Handle(SubmitAssignmentCommand request, CancellationToken cancellationToken)
    {
        var assignment = await _assignmentRepository.GetByIdAsync(request.AssignmentId, cancellationToken);
        var student = await _userRepository.GetByIdAsync(request.StudentId, cancellationToken);

        // Students may only see/act on Published assignments in their own class — same
        // NotFound-for-everything policy as GetAssignmentById, so ids aren't enumerable.
        if (assignment is null || student is null ||
            assignment.Status != AssignmentStatus.Published ||
            assignment.ClassId != student.ClassId)
        {
            return SubmissionErrors.AssignmentNotFound(request.AssignmentId);
        }

        var alreadySubmitted = await _submissionRepository.Query()
            .AnyAsync(s => s.AssignmentId == request.AssignmentId && s.StudentId == request.StudentId, cancellationToken);

        if (alreadySubmitted)
        {
            return SubmissionErrors.AlreadySubmitted;
        }

        var now = DateTime.UtcNow;
        var isLate = now > assignment.Deadline;

        if (isLate && !assignment.AllowLateSubmission)
        {
            return SubmissionErrors.DeadlinePassed;
        }

        var submission = new Submission
        {
            Id = Guid.NewGuid(),
            AssignmentId = request.AssignmentId,
            StudentId = request.StudentId,
            Content = request.Content,
            SubmittedAt = now,
            IsLate = isLate,
            AttemptNumber = 1,
            Status = isLate ? SubmissionStatus.Late : SubmissionStatus.Submitted
        };

        await _submissionRepository.AddAsync(submission, cancellationToken);
        await _submissionRepository.SaveChangesAsync(cancellationToken);

        return await _submissionRepository.Query()
            .Where(s => s.Id == submission.Id)
            .Select(SubmissionMappings.ToDto)
            .FirstAsync(cancellationToken);
    }
}
