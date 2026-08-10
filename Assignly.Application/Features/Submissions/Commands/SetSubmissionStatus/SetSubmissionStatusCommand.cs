using Assignly.Application.Core.Abstractions;
using Assignly.Application.Dtos;
using Assignly.Domain.Enums;

namespace Assignly.Application.Features.Submissions.Commands.SetSubmissionStatus;

public sealed record SetSubmissionStatusCommand(Guid Id, SubmissionStatus Status, Guid TeacherId) : ICommand<SubmissionDto>;
