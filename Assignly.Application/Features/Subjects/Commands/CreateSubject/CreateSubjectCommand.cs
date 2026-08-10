using Assignly.Application.Core.Abstractions;
using Assignly.Application.Dtos;

namespace Assignly.Application.Features.Subjects.Commands.CreateSubject;

public sealed record CreateSubjectCommand(string Name, Guid ClassId) : ICommand<SubjectDto>;
