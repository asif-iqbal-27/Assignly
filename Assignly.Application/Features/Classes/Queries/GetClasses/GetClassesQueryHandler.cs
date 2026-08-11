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
        var query = _classRepository.Query();
        var orderedQuery = query.OrderBy(c => c.Name);
        var projectedQuery = orderedQuery.Select(c => new ClassDto
        {
            Id = c.Id,
            Name = c.Name,
            Section = c.Section,
            Description = c.Description
        });

        var classes = await projectedQuery.ToListAsync(cancellationToken);

        return classes;
    }
}
