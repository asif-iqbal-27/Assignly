using Assignly.Application.Core.Abstractions;
using Assignly.Application.Dtos;

namespace Assignly.Application.Features.ClassSubjectTeachers.Commands.CreateClassSubjectTeacher;

public sealed record CreateClassSubjectTeacherCommand(Guid TeacherId, Guid SubjectId) : ICommand<ClassSubjectTeacherDto>;
