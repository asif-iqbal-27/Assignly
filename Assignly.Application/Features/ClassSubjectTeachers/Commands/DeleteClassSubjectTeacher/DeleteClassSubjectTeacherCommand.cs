using Assignly.Application.Core.Abstractions;
using ErrorOr;

namespace Assignly.Application.Features.ClassSubjectTeachers.Commands.DeleteClassSubjectTeacher;

public sealed record DeleteClassSubjectTeacherCommand(Guid Id) : ICommand<Deleted>;
