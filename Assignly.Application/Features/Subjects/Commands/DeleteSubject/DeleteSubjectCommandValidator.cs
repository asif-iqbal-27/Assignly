using FluentValidation;

namespace Assignly.Application.Features.Subjects.Commands.DeleteSubject;

public sealed class DeleteSubjectCommandValidator : AbstractValidator<DeleteSubjectCommand>
{
    public DeleteSubjectCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
