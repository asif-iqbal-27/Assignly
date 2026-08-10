using FluentValidation;

namespace Assignly.Application.Features.ClassSubjectTeachers.Commands.DeleteClassSubjectTeacher;

public sealed class DeleteClassSubjectTeacherCommandValidator : AbstractValidator<DeleteClassSubjectTeacherCommand>
{
    public DeleteClassSubjectTeacherCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
