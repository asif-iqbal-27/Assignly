using Assignly.Application.Core.Abstractions;
using Assignly.Application.Dtos;
using Assignly.Application.Interfaces;
using Assignly.Domain.Entities;
using Assignly.Domain.Enums;
using ErrorOr;
using Microsoft.EntityFrameworkCore;

namespace Assignly.Application.Features.Assignments.Queries.GetAssignmentById;

public sealed class GetAssignmentByIdQueryHandler : IQueryHandler<GetAssignmentByIdQuery, AssignmentDto>
{
    private readonly IRepository<Assignment> _assignmentRepository;
    private readonly IRepository<TeacherSubjectAssignment> _teacherSubjectAssignmentRepository;

    public GetAssignmentByIdQueryHandler(
        IRepository<Assignment> assignmentRepository,
        IRepository<TeacherSubjectAssignment> teacherSubjectAssignmentRepository)
    {
        _assignmentRepository = assignmentRepository;
        _teacherSubjectAssignmentRepository = teacherSubjectAssignmentRepository;
    }

    public async Task<ErrorOr<AssignmentDto>> Handle(GetAssignmentByIdQuery request, CancellationToken cancellationToken)
    {
        var assignment = await _assignmentRepository.Query()
            .Where(a => a.Id == request.Id)
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
            .FirstOrDefaultAsync(cancellationToken);

        if (assignment is null)
        {
            return AssignmentErrors.NotFound(request.Id);
        }

        var visible = request.RequestingUserRole switch
        {
            RoleType.Admin => true,
            RoleType.Teacher => await _teacherSubjectAssignmentRepository.Query()
                .AnyAsync(t => t.TeacherId == request.RequestingUserId && t.SubjectId == assignment.SubjectId, cancellationToken),
            RoleType.Student => assignment.ClassId == request.RequestingUserClassId &&
                                 assignment.Status == nameof(AssignmentStatus.Published),
            _ => false
        };

        if (!visible)
        {
            return AssignmentErrors.NotFound(request.Id);
        }

        return assignment;
    }
}
