using Assignly.Application.Core.Abstractions;
using Assignly.Application.Dtos;
using Assignly.Application.Interfaces;
using Assignly.Domain.Entities;
using ErrorOr;
using Microsoft.EntityFrameworkCore;

namespace Assignly.Application.Features.Classes.Queries.GetClassById;

public sealed class GetClassByIdQueryHandler : IQueryHandler<GetClassByIdQuery, ClassDto>
{
    private readonly IRepository<SchoolClass> _classRepository;

    public GetClassByIdQueryHandler(IRepository<SchoolClass> classRepository)
    {
        _classRepository = classRepository;
    }

    public async Task<ErrorOr<ClassDto>> Handle(GetClassByIdQuery request, CancellationToken cancellationToken)
    {
        var query = _classRepository.Query();
        var filteredQuery = query.Where(c => c.Id == request.Id);
        var projectedQuery = filteredQuery.Select(c => new ClassDto
        {
            Id = c.Id,
            Name = c.Name,
            Section = c.Section,
            Description = c.Description
        });

        var schoolClass = await projectedQuery.FirstOrDefaultAsync(cancellationToken);

        return schoolClass is null ? ClassErrors.NotFound(request.Id) : schoolClass;
    }
}
