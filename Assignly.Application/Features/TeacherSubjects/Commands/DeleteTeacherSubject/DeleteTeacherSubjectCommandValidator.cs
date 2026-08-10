using FluentValidation;

namespace Assignly.Application.Features.TeacherSubjects.Commands.DeleteTeacherSubject;

public sealed class DeleteTeacherSubjectCommandValidator : AbstractValidator<DeleteTeacherSubjectCommand>
{
    public DeleteTeacherSubjectCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
