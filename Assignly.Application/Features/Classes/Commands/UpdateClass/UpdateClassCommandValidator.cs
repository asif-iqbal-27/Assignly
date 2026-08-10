using FluentValidation;

namespace Assignly.Application.Features.Classes.Commands.UpdateClass;

public sealed class UpdateClassCommandValidator : AbstractValidator<UpdateClassCommand>
{
    public UpdateClassCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Section).MaximumLength(50);
    }
}
