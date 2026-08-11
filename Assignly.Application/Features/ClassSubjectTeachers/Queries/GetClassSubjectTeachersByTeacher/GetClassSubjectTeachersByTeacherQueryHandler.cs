using Assignly.Application.Core.Abstractions;
using Assignly.Application.Dtos;
using Assignly.Application.Interfaces;
using Assignly.Domain.Entities;
using ErrorOr;
using Microsoft.EntityFrameworkCore;

namespace Assignly.Application.Features.ClassSubjectTeachers.Queries.GetClassSubjectTeachersByTeacher;

public sealed class GetClassSubjectTeachersByTeacherQueryHandler
    : IQueryHandler<GetClassSubjectTeachersByTeacherQuery, List<ClassSubjectTeacherDto>>
{
    private readonly IRepository<ClassSubjectTeacher> _classSubjectTeacherRepository;

    public GetClassSubjectTeachersByTeacherQueryHandler(IRepository<ClassSubjectTeacher> classSubjectTeacherRepository)
    {
        _classSubjectTeacherRepository = classSubjectTeacherRepository;
    }

    public async Task<ErrorOr<List<ClassSubjectTeacherDto>>> Handle(GetClassSubjectTeachersByTeacherQuery request, CancellationToken cancellationToken)
    {
        var query = _classSubjectTeacherRepository.Query();
        var filteredQuery = query.Where(t => t.TeacherId == request.TeacherId);
        var orderedQuery = filteredQuery.OrderBy(t => t.Subject.Name);
        var projectedQuery = orderedQuery.Select(t => new ClassSubjectTeacherDto
        {
            Id = t.Id,
            TeacherId = t.TeacherId,
            TeacherName = t.Teacher.FullName,
            SubjectId = t.SubjectId,
            SubjectName = t.Subject.Name
        });

        var classSubjectTeachers = await projectedQuery.ToListAsync(cancellationToken);

        return classSubjectTeachers;
    }
}
