using Assignly.Application.Core.Abstractions;
using Assignly.Application.Interfaces;
using Assignly.Domain.Entities;
using ErrorOr;
using Microsoft.EntityFrameworkCore;

namespace Assignly.Application.Features.Assignments.Commands.DeleteAssignment;

public sealed class DeleteAssignmentCommandHandler : ICommandHandler<DeleteAssignmentCommand, Deleted>
{
    private readonly IRepository<Assignment> _assignmentRepository;
    private readonly IRepository<ClassSubjectTeacher> _classSubjectTeacherRepository;
    private readonly IRepository<Submission> _submissionRepository;

    public DeleteAssignmentCommandHandler(
        IRepository<Assignment> assignmentRepository,
        IRepository<ClassSubjectTeacher> classSubjectTeacherRepository,
        IRepository<Submission> submissionRepository)
    {
        _assignmentRepository = assignmentRepository;
        _classSubjectTeacherRepository = classSubjectTeacherRepository;
        _submissionRepository = submissionRepository;
    }

    public async Task<ErrorOr<Deleted>> Handle(DeleteAssignmentCommand request, CancellationToken cancellationToken)
    {
        var assignment = await _assignmentRepository.GetByIdAsync(request.Id, cancellationToken);
        if (assignment is null)
        {
            return AssignmentErrors.NotFound(request.Id);
        }

        var isOwner = await _classSubjectTeacherRepository.Query()
            .AnyAsync(t => t.TeacherId == request.TeacherId && t.SubjectId == assignment.SubjectId, cancellationToken);

        if (!isOwner)
        {
            return AssignmentErrors.NotOwner;
        }

        var hasSubmissions = await _submissionRepository.Query()
            .AnyAsync(s => s.AssignmentId == request.Id, cancellationToken);

        if (hasSubmissions)
        {
            return AssignmentErrors.HasSubmissions;
        }

        _assignmentRepository.Remove(assignment);
        await _assignmentRepository.SaveChangesAsync(cancellationToken);

        return Result.Deleted;
    }
}
