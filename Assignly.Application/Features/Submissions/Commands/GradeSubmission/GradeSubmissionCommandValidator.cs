using FluentValidation;

namespace Assignly.Application.Features.Submissions.Commands.GradeSubmission;

public sealed class GradeSubmissionCommandValidator : AbstractValidator<GradeSubmissionCommand>
{
    public GradeSubmissionCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.TeacherId).NotEmpty();
        RuleFor(x => x.Marks).GreaterThanOrEqualTo(0);
    }
}
