using Assignly.Application.Core.Abstractions;
using Assignly.Application.Dtos;

namespace Assignly.Application.Features.Submissions.Commands.GradeSubmission;

public sealed record GradeSubmissionCommand(Guid Id, int Marks, string? Feedback, Guid TeacherId) : ICommand<SubmissionDto>;
