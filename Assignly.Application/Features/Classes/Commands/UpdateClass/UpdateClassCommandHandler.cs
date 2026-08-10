using Assignly.Application.Core.Abstractions;
using Assignly.Application.Dtos;
using Assignly.Application.Interfaces;
using Assignly.Domain.Entities;
using ErrorOr;

namespace Assignly.Application.Features.Classes.Commands.UpdateClass;

public sealed class UpdateClassCommandHandler : ICommandHandler<UpdateClassCommand, ClassDto>
{
    private readonly IRepository<SchoolClass> _classRepository;

    public UpdateClassCommandHandler(IRepository<SchoolClass> classRepository)
    {
        _classRepository = classRepository;
    }

    public async Task<ErrorOr<ClassDto>> Handle(UpdateClassCommand request, CancellationToken cancellationToken)
    {
        var schoolClass = await _classRepository.GetByIdAsync(request.Id, cancellationToken);
        if (schoolClass is null)
        {
            return ClassErrors.NotFound(request.Id);
        }

        schoolClass.Name = request.Name;
        schoolClass.Section = request.Section;
        schoolClass.Description = request.Description;

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
