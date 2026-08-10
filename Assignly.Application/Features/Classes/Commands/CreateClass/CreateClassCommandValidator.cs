using FluentValidation;

namespace Assignly.Application.Features.Classes.Commands.CreateClass;

public sealed class CreateClassCommandValidator : AbstractValidator<CreateClassCommand>
{
    public CreateClassCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Section).MaximumLength(50);
    }
}
