using FluentValidation;

namespace Assignly.Application.Features.Submissions.Commands.SetSubmissionStatus;

public sealed class SetSubmissionStatusCommandValidator : AbstractValidator<SetSubmissionStatusCommand>
{
    public SetSubmissionStatusCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.TeacherId).NotEmpty();
        RuleFor(x => x.Status).IsInEnum();
    }
}
