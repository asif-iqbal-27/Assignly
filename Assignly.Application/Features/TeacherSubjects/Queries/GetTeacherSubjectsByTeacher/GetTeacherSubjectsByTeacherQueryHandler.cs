using Assignly.Application.Core.Abstractions;
using Assignly.Application.Dtos;
using Assignly.Application.Interfaces;
using Assignly.Domain.Entities;
using ErrorOr;
using Microsoft.EntityFrameworkCore;

namespace Assignly.Application.Features.TeacherSubjects.Queries.GetTeacherSubjectsByTeacher;

public sealed class GetTeacherSubjectsByTeacherQueryHandler
    : IQueryHandler<GetTeacherSubjectsByTeacherQuery, List<TeacherSubjectDto>>
{
    private readonly IRepository<TeacherSubjectAssignment> _teacherSubjectAssignmentRepository;

    public GetTeacherSubjectsByTeacherQueryHandler(IRepository<TeacherSubjectAssignment> teacherSubjectAssignmentRepository)
    {
        _teacherSubjectAssignmentRepository = teacherSubjectAssignmentRepository;
    }

    public async Task<ErrorOr<List<TeacherSubjectDto>>> Handle(GetTeacherSubjectsByTeacherQuery request, CancellationToken cancellationToken)
    {
        var assignments = await _teacherSubjectAssignmentRepository.Query()
            .Where(t => t.TeacherId == request.TeacherId)
            .OrderBy(t => t.Subject.Name)
            .Select(t => new TeacherSubjectDto
            {
                Id = t.Id,
                TeacherId = t.TeacherId,
                TeacherName = t.Teacher.FullName,
                SubjectId = t.SubjectId,
                SubjectName = t.Subject.Name
            })
            .ToListAsync(cancellationToken);

        return assignments;
    }
}
