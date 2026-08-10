using Assignly.Application.Core.Abstractions;
using Assignly.Application.Dtos;
using Assignly.Application.Interfaces;
using Assignly.Domain.Entities;
using Assignly.Domain.Enums;
using ErrorOr;
using Microsoft.EntityFrameworkCore;

namespace Assignly.Application.Features.Assignments.Queries.GetAssignments;

public sealed class GetAssignmentsQueryHandler : IQueryHandler<GetAssignmentsQuery, List<AssignmentDto>>
{
    private readonly IRepository<Assignment> _assignmentRepository;
    private readonly IRepository<ClassSubjectTeacher> _classSubjectTeacherRepository;

    public GetAssignmentsQueryHandler(
        IRepository<Assignment> assignmentRepository,
        IRepository<ClassSubjectTeacher> classSubjectTeacherRepository)
    {
        _assignmentRepository = assignmentRepository;
        _classSubjectTeacherRepository = classSubjectTeacherRepository;
    }

    public async Task<ErrorOr<List<AssignmentDto>>> Handle(GetAssignmentsQuery request, CancellationToken cancellationToken)
    {
        var query = _assignmentRepository.Query();

        switch (request.RequestingUserRole)
        {
            case RoleType.Admin:
                break;

            case RoleType.Teacher:
                var ownedSubjectIds = await _classSubjectTeacherRepository.Query()
                    .Where(t => t.TeacherId == request.RequestingUserId)
                    .Select(t => t.SubjectId)
                    .ToListAsync(cancellationToken);
                query = query.Where(a => ownedSubjectIds.Contains(a.SubjectId));
                break;

            case RoleType.Student:
                if (request.RequestingUserClassId is null)
                {
                    return new List<AssignmentDto>();
                }

                query = query.Where(a =>
                    a.ClassId == request.RequestingUserClassId.Value &&
                    a.Status == AssignmentStatus.Published);
                break;

            default:
                return new List<AssignmentDto>();
        }

        var assignments = await query
            .OrderByDescending(a => a.CreatedAt)
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
            .ToListAsync(cancellationToken);

        return assignments;
    }
}
