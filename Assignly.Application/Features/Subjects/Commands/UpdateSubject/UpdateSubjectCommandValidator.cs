using Assignly.Application.Interfaces;
using Assignly.Domain.Entities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Assignly.Application.Features.Subjects.Commands.UpdateSubject;

public sealed class UpdateSubjectCommandValidator : AbstractValidator<UpdateSubjectCommand>
{
    public UpdateSubjectCommandValidator(IRepository<SchoolClass> classRepository)
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);

        RuleFor(x => x.ClassId)
            .NotEmpty()
            .MustAsync((classId, ct) => classRepository.Query().AnyAsync(c => c.Id == classId, ct))
            .WithMessage("The specified class does not exist.");
    }
}
