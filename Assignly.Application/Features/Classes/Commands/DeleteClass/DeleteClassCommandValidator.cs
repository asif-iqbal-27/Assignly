using FluentValidation;

namespace Assignly.Application.Features.Classes.Commands.DeleteClass;

public sealed class DeleteClassCommandValidator : AbstractValidator<DeleteClassCommand>
{
    public DeleteClassCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
