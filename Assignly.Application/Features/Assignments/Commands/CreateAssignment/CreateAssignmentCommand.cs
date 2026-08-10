using Assignly.Application.Core.Abstractions;
using Assignly.Application.Dtos;

namespace Assignly.Application.Features.Assignments.Commands.CreateAssignment;

public sealed record CreateAssignmentCommand(
    string Title,
    string Description,
    Guid SubjectId,
    Guid ClassId,
    DateTime Deadline,
    int MaxMarks,
    bool AllowLateSubmission,
    bool AllowResubmission,
    Guid TeacherId) : ICommand<AssignmentDto>;
