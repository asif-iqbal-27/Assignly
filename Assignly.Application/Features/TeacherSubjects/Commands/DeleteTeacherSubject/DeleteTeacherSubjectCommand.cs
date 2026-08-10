using Assignly.Application.Core.Abstractions;
using ErrorOr;

namespace Assignly.Application.Features.TeacherSubjects.Commands.DeleteTeacherSubject;

public sealed record DeleteTeacherSubjectCommand(Guid Id) : ICommand<Deleted>;
