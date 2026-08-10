using Assignly.Application.Core.Abstractions;
using Assignly.Application.Dtos;
using Assignly.Application.Interfaces;
using Assignly.Domain.Entities;
using Assignly.Domain.Enums;
using ErrorOr;
using Microsoft.EntityFrameworkCore;

namespace Assignly.Application.Features.Assignments.Commands.PublishAssignment;

public sealed class PublishAssignmentCommandHandler : ICommandHandler<PublishAssignmentCommand, AssignmentDto>
{
    private readonly IRepository<Assignment> _assignmentRepository;
    private readonly IRepository<TeacherSubjectAssignment> _teacherSubjectAssignmentRepository;

    public PublishAssignmentCommandHandler(
        IRepository<Assignment> assignmentRepository,
        IRepository<TeacherSubjectAssignment> teacherSubjectAssignmentRepository)
    {
        _assignmentRepository = assignmentRepository;
        _teacherSubjectAssignmentRepository = teacherSubjectAssignmentRepository;
    }

    public async Task<ErrorOr<AssignmentDto>> Handle(PublishAssignmentCommand request, CancellationToken cancellationToken)
    {
        var assignment = await _assignmentRepository.GetByIdAsync(request.Id, cancellationToken);
        if (assignment is null)
        {
            return AssignmentErrors.NotFound(request.Id);
        }

        var isOwner = await _teacherSubjectAssignmentRepository.Query()
            .AnyAsync(t => t.TeacherId == request.TeacherId && t.SubjectId == assignment.SubjectId, cancellationToken);

        if (!isOwner)
        {
            return AssignmentErrors.NotOwner;
        }

        if (assignment.Status == AssignmentStatus.Published)
        {
            return AssignmentErrors.AlreadyPublished;
        }

        assignment.Status = AssignmentStatus.Published;
        assignment.UpdatedAt = DateTime.UtcNow;

        await _assignmentRepository.SaveChangesAsync(cancellationToken);

        return await _assignmentRepository.Query()
            .Where(a => a.Id == assignment.Id)
            .Select(a => new AssignmentDto
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
            })
            .FirstAsync(cancellationToken);
    }
}
