using Assignly.Application.Interfaces;
using Assignly.Domain.Entities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Assignly.Application.Features.Subjects.Commands.CreateSubject;

public sealed class CreateSubjectCommandValidator : AbstractValidator<CreateSubjectCommand>
{
    public CreateSubjectCommandValidator(IRepository<SchoolClass> classRepository)
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);

        RuleFor(x => x.ClassId)
            .NotEmpty()
            .MustAsync((classId, ct) => classRepository.Query().AnyAsync(c => c.Id == classId, ct))
            .WithMessage("The specified class does not exist.");
    }
}
