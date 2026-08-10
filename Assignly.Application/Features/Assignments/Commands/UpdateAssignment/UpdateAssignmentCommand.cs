using Assignly.Application.Core.Abstractions;
using Assignly.Application.Dtos;

namespace Assignly.Application.Features.Assignments.Commands.UpdateAssignment;

public sealed record UpdateAssignmentCommand(
    Guid Id,
    string Title,
    string Description,
    DateTime Deadline,
    int MaxMarks,
    bool AllowLateSubmission,
    bool AllowResubmission,
    Guid TeacherId) : ICommand<AssignmentDto>;
