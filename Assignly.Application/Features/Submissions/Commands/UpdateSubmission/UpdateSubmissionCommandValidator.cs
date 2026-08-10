using FluentValidation;

namespace Assignly.Application.Features.Submissions.Commands.UpdateSubmission;

public sealed class UpdateSubmissionCommandValidator : AbstractValidator<UpdateSubmissionCommand>
{
    public UpdateSubmissionCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Content).NotEmpty();
        RuleFor(x => x.StudentId).NotEmpty();
    }
}
