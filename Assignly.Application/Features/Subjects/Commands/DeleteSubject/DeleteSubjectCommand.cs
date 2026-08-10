using Assignly.Application.Core.Abstractions;
using ErrorOr;

namespace Assignly.Application.Features.Subjects.Commands.DeleteSubject;

public sealed record DeleteSubjectCommand(Guid Id) : ICommand<Deleted>;
