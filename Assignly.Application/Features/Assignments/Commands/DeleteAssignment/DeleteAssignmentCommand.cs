using Assignly.Application.Core.Abstractions;
using ErrorOr;

namespace Assignly.Application.Features.Assignments.Commands.DeleteAssignment;

public sealed record DeleteAssignmentCommand(Guid Id, Guid TeacherId) : ICommand<Deleted>;
