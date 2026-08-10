using Assignly.Application.Core.Abstractions;
using Assignly.Application.Dtos;

namespace Assignly.Application.Features.Assignments.Commands.PublishAssignment;

public sealed record PublishAssignmentCommand(Guid Id, Guid TeacherId) : ICommand<AssignmentDto>;
