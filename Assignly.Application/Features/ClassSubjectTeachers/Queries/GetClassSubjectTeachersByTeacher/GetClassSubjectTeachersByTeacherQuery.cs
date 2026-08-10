using Assignly.Application.Core.Abstractions;
using Assignly.Application.Dtos;

namespace Assignly.Application.Features.ClassSubjectTeachers.Queries.GetClassSubjectTeachersByTeacher;

public sealed record GetClassSubjectTeachersByTeacherQuery(Guid TeacherId) : IQuery<List<ClassSubjectTeacherDto>>;
