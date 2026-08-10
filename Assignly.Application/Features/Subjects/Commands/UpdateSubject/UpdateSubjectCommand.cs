using Assignly.Application.Core.Abstractions;
using Assignly.Application.Dtos;

namespace Assignly.Application.Features.Subjects.Commands.UpdateSubject;

public sealed record UpdateSubjectCommand(Guid Id, string Name, Guid ClassId) : ICommand<SubjectDto>;
