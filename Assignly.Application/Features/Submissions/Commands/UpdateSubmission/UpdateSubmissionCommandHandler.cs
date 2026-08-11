using Assignly.Application.Core.Abstractions;
using Assignly.Application.Dtos;
using Assignly.Application.Interfaces;
using Assignly.Domain.Entities;
using Assignly.Domain.Enums;
using ErrorOr;
using Microsoft.EntityFrameworkCore;

namespace Assignly.Application.Features.Submissions.Commands.UpdateSubmission;

public sealed class UpdateSubmissionCommandHandler : ICommandHandler<UpdateSubmissionCommand, SubmissionDto>
{
    private readonly IRepository<Submission> _submissionRepository;
    private readonly IRepository<Assignment> _assignmentRepository;

    public UpdateSubmissionCommandHandler(
        IRepository<Submission> submissionRepository,
        IRepository<Assignment> assignmentRepository)
    {
        _submissionRepository = submissionRepository;
        _assignmentRepository = assignmentRepository;
    }

    public async Task<ErrorOr<SubmissionDto>> Handle(UpdateSubmissionCommand request, CancellationToken cancellationToken)
    {
        var submission = await _submissionRepository.GetByIdAsync(request.Id, cancellationToken);
        if (submission is null || submission.StudentId != request.StudentId)
        {
            return SubmissionErrors.NotFound(request.Id);
        }

        var assignment = await _assignmentRepository.GetByIdAsync(submission.AssignmentId, cancellationToken);
        if (assignment is null)
        {
            return SubmissionErrors.AssignmentNotFound(submission.AssignmentId);
        }

        if (!assignment.AllowResubmission)
        {
            return SubmissionErrors.ResubmissionNotAllowed;
        }

        // AllowLateSubmission never extends this window — resubmission always requires
        // the deadline to still be in the future, independent of that flag.
        if (DateTime.UtcNow > assignment.Deadline)
        {
            return SubmissionErrors.ResubmissionWindowClosed;
        }

        var nextAttemptNumber = await _submissionRepository.Query()
            .Where(s => s.AssignmentId == submission.AssignmentId && s.StudentId == submission.StudentId)
            .MaxAsync(s => s.AttemptNumber, cancellationToken) + 1;

        var newSubmission = new Submission
        {
            Id = Guid.NewGuid(),
            AssignmentId = submission.AssignmentId,
            StudentId = submission.StudentId,
            Content = request.Content,
            SubmittedAt = DateTime.UtcNow,
            IsLate = false,
            AttemptNumber = nextAttemptNumber,
            Status = SubmissionStatus.Submitted
        };

        await _submissionRepository.AddAsync(newSubmission, cancellationToken);
        await _submissionRepository.SaveChangesAsync(cancellationToken);

        var query = _submissionRepository.Query();
        var filteredQuery = query.Where(s => s.Id == newSubmission.Id);
        var projectedQuery = filteredQuery.Select(SubmissionMappings.ToDto);

        return await projectedQuery.FirstAsync(cancellationToken);
    }
}
