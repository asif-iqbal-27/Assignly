using Assignly.Application.Interfaces;
using Assignly.Domain.Entities;
using Assignly.Domain.Enums;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Assignly.Application.Features.ClassSubjectTeachers.Commands.CreateClassSubjectTeacher;

public sealed class CreateClassSubjectTeacherCommandValidator : AbstractValidator<CreateClassSubjectTeacherCommand>
{
    public CreateClassSubjectTeacherCommandValidator(
        IRepository<ApplicationUser> userRepository,
        IRepository<Subject> subjectRepository)
    {
        RuleFor(x => x.TeacherId)
            .NotEmpty()
            .MustAsync((teacherId, ct) => userRepository.Query()
                .AnyAsync(u => u.Id == teacherId && u.Role == RoleType.Teacher, ct))
            .WithMessage("The specified user does not exist or is not a Teacher.");

        RuleFor(x => x.SubjectId)
            .NotEmpty()
            .MustAsync((subjectId, ct) => subjectRepository.Query().AnyAsync(s => s.Id == subjectId, ct))
            .WithMessage("The specified subject does not exist.");
    }
}
