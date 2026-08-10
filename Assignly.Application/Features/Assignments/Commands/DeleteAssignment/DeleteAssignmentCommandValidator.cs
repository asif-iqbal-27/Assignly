using FluentValidation;

namespace Assignly.Application.Features.Assignments.Commands.DeleteAssignment;

public sealed class DeleteAssignmentCommandValidator : AbstractValidator<DeleteAssignmentCommand>
{
    public DeleteAssignmentCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.TeacherId).NotEmpty();
    }
}
