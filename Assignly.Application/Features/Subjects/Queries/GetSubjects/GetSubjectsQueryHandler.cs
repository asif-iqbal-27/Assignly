using Assignly.Application.Core.Abstractions;
using Assignly.Application.Dtos;
using Assignly.Application.Interfaces;
using Assignly.Domain.Entities;
using ErrorOr;
using Microsoft.EntityFrameworkCore;

namespace Assignly.Application.Features.Subjects.Queries.GetSubjects;

public sealed class GetSubjectsQueryHandler : IQueryHandler<GetSubjectsQuery, List<SubjectDto>>
{
    private readonly IRepository<Subject> _subjectRepository;

    public GetSubjectsQueryHandler(IRepository<Subject> subjectRepository)
    {
        _subjectRepository = subjectRepository;
    }

    public async Task<ErrorOr<List<SubjectDto>>> Handle(GetSubjectsQuery request, CancellationToken cancellationToken)
    {
        var query = _subjectRepository.Query();
        var orderedQuery = query.OrderBy(s => s.Name);
        var projectedQuery = orderedQuery.Select(s => new SubjectDto
        {
            Id = s.Id,
            Name = s.Name,
            ClassId = s.ClassId,
            ClassName = s.Class.Name
        });

        var subjects = await projectedQuery.ToListAsync(cancellationToken);

        return subjects;
    }
}
