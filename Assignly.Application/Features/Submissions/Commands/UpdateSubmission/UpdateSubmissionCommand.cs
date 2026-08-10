using Assignly.Application.Core.Abstractions;
using Assignly.Application.Dtos;

namespace Assignly.Application.Features.Submissions.Commands.UpdateSubmission;

public sealed record UpdateSubmissionCommand(Guid Id, string? Content, Guid StudentId) : ICommand<SubmissionDto>;
