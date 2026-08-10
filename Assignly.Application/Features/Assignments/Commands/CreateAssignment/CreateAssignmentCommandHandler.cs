using Assignly.Application.Core.Abstractions;
using Assignly.Application.Dtos;
using Assignly.Application.Interfaces;
using Assignly.Domain.Entities;
using Assignly.Domain.Enums;
using ErrorOr;
using Microsoft.EntityFrameworkCore;

namespace Assignly.Application.Features.Assignments.Commands.CreateAssignment;

public sealed class CreateAssignmentCommandHandler : ICommandHandler<CreateAssignmentCommand, AssignmentDto>
{
    private readonly IRepository<Assignment> _assignmentRepository;
    private readonly IRepository<TeacherSubjectAssignment> _teacherSubjectAssignmentRepository;

    public CreateAssignmentCommandHandler(
        IRepository<Assignment> assignmentRepository,
        IRepository<TeacherSubjectAssignment> teacherSubjectAssignmentRepository)
    {
        _assignmentRepository = assignmentRepository;
        _teacherSubjectAssignmentRepository = teacherSubjectAssignmentRepository;
    }

    public async Task<ErrorOr<AssignmentDto>> Handle(CreateAssignmentCommand request, CancellationToken cancellationToken)
    {
        var isOwner = await _teacherSubjectAssignmentRepository.Query()
            .AnyAsync(t => t.TeacherId == request.TeacherId && t.SubjectId == request.SubjectId, cancellationToken);

        if (!isOwner)
        {
            return AssignmentErrors.NotOwner;
        }

        var now = DateTime.UtcNow;
        var assignment = new Assignment
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Description = request.Description,
            SubjectId = request.SubjectId,
            ClassId = request.ClassId,
            CreatedByTeacherId = request.TeacherId,
            Deadline = request.Deadline,
            MaxMarks = request.MaxMarks,
            Status = AssignmentStatus.Draft,
            AllowLateSubmission = request.AllowLateSubmission,
            AllowResubmission = request.AllowResubmission,
            CreatedAt = now,
            UpdatedAt = now
        };

        await _assignmentRepository.AddAsync(assignment, cancellationToken);
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
