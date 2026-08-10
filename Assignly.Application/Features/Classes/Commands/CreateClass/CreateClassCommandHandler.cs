using Assignly.Application.Core.Abstractions;
using Assignly.Application.Dtos;
using Assignly.Application.Interfaces;
using Assignly.Domain.Entities;
using ErrorOr;

namespace Assignly.Application.Features.Classes.Commands.CreateClass;

public sealed class CreateClassCommandHandler : ICommandHandler<CreateClassCommand, ClassDto>
{
    private readonly IRepository<SchoolClass> _classRepository;

    public CreateClassCommandHandler(IRepository<SchoolClass> classRepository)
    {
        _classRepository = classRepository;
    }

    public async Task<ErrorOr<ClassDto>> Handle(CreateClassCommand request, CancellationToken cancellationToken)
    {
        var schoolClass = new SchoolClass
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Section = request.Section,
            Description = request.Description
        };

        await _classRepository.AddAsync(schoolClass, cancellationToken);
        await _classRepository.SaveChangesAsync(cancellationToken);

        return new ClassDto
        {
            Id = schoolClass.Id,
            Name = schoolClass.Name,
            Section = schoolClass.Section,
            Description = schoolClass.Description
        };
    }
}
