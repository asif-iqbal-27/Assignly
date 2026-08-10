using Assignly.Application.Core.Abstractions;
using Assignly.Application.Dtos;

namespace Assignly.Application.Features.TeacherSubjects.Commands.CreateTeacherSubject;

public sealed record CreateTeacherSubjectCommand(Guid TeacherId, Guid SubjectId) : ICommand<TeacherSubjectDto>;
