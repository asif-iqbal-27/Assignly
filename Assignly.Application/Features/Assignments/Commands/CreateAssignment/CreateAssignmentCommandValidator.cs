using Assignly.Application.Interfaces;
using Assignly.Domain.Entities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Assignly.Application.Features.Assignments.Commands.CreateAssignment;

public sealed class CreateAssignmentCommandValidator : AbstractValidator<CreateAssignmentCommand>
{
    public CreateAssignmentCommandValidator(IRepository<Subject> subjectRepository, IRepository<SchoolClass> classRepository)
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).NotEmpty();
        RuleFor(x => x.Deadline).NotEmpty();
        RuleFor(x => x.MaxMarks).GreaterThan(0);
        RuleFor(x => x.TeacherId).NotEmpty();

        RuleFor(x => x.SubjectId)
            .NotEmpty()
            .MustAsync((subjectId, ct) => subjectRepository.Query().AnyAsync(s => s.Id == subjectId, ct))
            .WithMessage("The specified subject does not exist.");

        RuleFor(x => x.ClassId)
            .NotEmpty()
            .MustAsync((classId, ct) => classRepository.Query().AnyAsync(c => c.Id == classId, ct))
            .WithMessage("The specified class does not exist.");

        RuleFor(x => x)
            .MustAsync(async (command, ct) =>
            {
                var subject = await subjectRepository.Query().FirstOrDefaultAsync(s => s.Id == command.SubjectId, ct);
                return subject is null || subject.ClassId == command.ClassId;
            })
            .WithMessage("Assignment.ClassId must match the selected subject's class.")
            .WithName(nameof(CreateAssignmentCommand.ClassId));
    }
}
