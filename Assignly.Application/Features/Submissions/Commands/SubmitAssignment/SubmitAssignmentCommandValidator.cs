using FluentValidation;

namespace Assignly.Application.Features.Submissions.Commands.SubmitAssignment;

public sealed class SubmitAssignmentCommandValidator : AbstractValidator<SubmitAssignmentCommand>
{
    public SubmitAssignmentCommandValidator()
    {
        RuleFor(x => x.AssignmentId).NotEmpty();
        RuleFor(x => x.Content).NotEmpty();
        RuleFor(x => x.StudentId).NotEmpty();
    }
}
