using Assignly.Application.Core.Abstractions;
using Assignly.Application.Dtos;

namespace Assignly.Application.Features.Submissions.Commands.SubmitAssignment;

public sealed record SubmitAssignmentCommand(Guid AssignmentId, string? Content, Guid StudentId) : ICommand<SubmissionDto>;
