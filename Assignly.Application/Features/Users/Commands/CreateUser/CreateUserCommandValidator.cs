using Assignly.Application.Interfaces;
using Assignly.Domain.Entities;
using Assignly.Domain.Enums;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Assignly.Application.Features.Users.Commands.CreateUser;

public sealed class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator(IRepository<SchoolClass> classRepository)
    {
        RuleFor(x => x.UserName).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(4);
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Role).IsInEnum();

        RuleFor(x => x.ClassId)
            .NotNull()
            .WithMessage("ClassId is required when Role is Student.")
            .When(x => x.Role == RoleType.Student);

        RuleFor(x => x.ClassId)
            .Null()
            .WithMessage("ClassId must not be set unless Role is Student.")
            .When(x => x.Role != RoleType.Student);

        RuleFor(x => x.ClassId)
            .MustAsync((classId, ct) => classRepository.Query().AnyAsync(c => c.Id == classId!.Value, ct))
            .WithMessage("The specified class does not exist.")
            .When(x => x.Role == RoleType.Student && x.ClassId.HasValue);
    }
}
