using Assignly.Application.Core.Abstractions;
using Assignly.Application.Dtos;
using Assignly.Application.Interfaces;
using Assignly.Domain.Entities;
using ErrorOr;
using Microsoft.EntityFrameworkCore;

namespace Assignly.Application.Features.Assignments.Commands.UpdateAssignment;

public sealed class UpdateAssignmentCommandHandler : ICommandHandler<UpdateAssignmentCommand, AssignmentDto>
{
    private readonly IRepository<Assignment> _assignmentRepository;
    private readonly IRepository<ClassSubjectTeacher> _classSubjectTeacherRepository;

    public UpdateAssignmentCommandHandler(
        IRepository<Assignment> assignmentRepository,
        IRepository<ClassSubjectTeacher> classSubjectTeacherRepository)
    {
        _assignmentRepository = assignmentRepository;
        _classSubjectTeacherRepository = classSubjectTeacherRepository;
    }

    public async Task<ErrorOr<AssignmentDto>> Handle(UpdateAssignmentCommand request, CancellationToken cancellationToken)
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

        assignment.Title = request.Title;
        assignment.Description = request.Description;
        assignment.Deadline = request.Deadline;
        assignment.MaxMarks = request.MaxMarks;
        assignment.AllowLateSubmission = request.AllowLateSubmission;
        assignment.AllowResubmission = request.AllowResubmission;
        assignment.UpdatedAt = DateTime.UtcNow;

        await _assignmentRepository.SaveChangesAsync(cancellationToken);

        var query = _assignmentRepository.Query();
        var filteredQuery = query.Where(a => a.Id == assignment.Id);
        var projectedQuery = filteredQuery.Select(a => new AssignmentDto
        {
            Id = a.Id,
            Title = a.Title,
            Description = a.Description,
            SubjectId = a.SubjectId,
            SubjectName = a.Subject.Name,
            ClassId = a.ClassId,
            ClassName = a.Class.Name,
            CreatedByTeacherId = a.CreatedByTeacherId,
            CreatedByTeacherName = a.CreatedByTeacher.FullName,
            Deadline = a.Deadline,
            MaxMarks = a.MaxMarks,
            Status = a.Status.ToString(),
            AllowLateSubmission = a.AllowLateSubmission,
            AllowResubmission = a.AllowResubmission,
            CreatedAt = a.CreatedAt,
            UpdatedAt = a.UpdatedAt
        });

        return await projectedQuery.FirstAsync(cancellationToken);
    }
}
