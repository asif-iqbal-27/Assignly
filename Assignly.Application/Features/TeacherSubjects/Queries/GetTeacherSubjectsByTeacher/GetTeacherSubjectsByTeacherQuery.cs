using Assignly.Application.Core.Abstractions;
using Assignly.Application.Dtos;

namespace Assignly.Application.Features.TeacherSubjects.Queries.GetTeacherSubjectsByTeacher;

public sealed record GetTeacherSubjectsByTeacherQuery(Guid TeacherId) : IQuery<List<TeacherSubjectDto>>;
