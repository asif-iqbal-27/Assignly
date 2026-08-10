using FluentValidation;

namespace Assignly.Application.Features.Assignments.Commands.PublishAssignment;

public sealed class PublishAssignmentCommandValidator : AbstractValidator<PublishAssignmentCommand>
{
    public PublishAssignmentCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.TeacherId).NotEmpty();
    }
}
