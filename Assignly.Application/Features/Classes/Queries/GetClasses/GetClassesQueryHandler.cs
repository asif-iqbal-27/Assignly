using Assignly.Application.Core.Abstractions;
using Assignly.Application.Dtos;
using Assignly.Application.Interfaces;
using Assignly.Domain.Entities;
using ErrorOr;
using Microsoft.EntityFrameworkCore;

namespace Assignly.Application.Features.Classes.Queries.GetClasses;

public sealed class GetClassesQueryHandler : IQueryHandler<GetClassesQuery, List<ClassDto>>
{
    private readonly IRepository<SchoolClass> _classRepository;

    public GetClassesQueryHandler(IRepository<SchoolClass> classRepository)
    {
        _classRepository = classRepository;
    }

    public async Task<ErrorOr<List<ClassDto>>> Handle(GetClassesQuery request, CancellationToken cancellationToken)
    {
        var classes = await _classRepository.Query()
            .OrderBy(c => c.Name)
            .Select(c => new ClassDto
            {
                Id = c.Id,
                Name = c.Name,
                Section = c.Section,
                Description = c.Description
            })
            .ToListAsync(cancellationToken);

        return classes;
    }
}
