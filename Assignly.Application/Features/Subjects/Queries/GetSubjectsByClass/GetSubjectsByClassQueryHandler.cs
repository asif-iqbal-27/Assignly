using Assignly.Application.Core.Abstractions;
using Assignly.Application.Dtos;
using Assignly.Application.Interfaces;
using Assignly.Domain.Entities;
using ErrorOr;
using Microsoft.EntityFrameworkCore;

namespace Assignly.Application.Features.Subjects.Queries.GetSubjectsByClass;

public sealed class GetSubjectsByClassQueryHandler : IQueryHandler<GetSubjectsByClassQuery, List<SubjectDto>>
{
    private readonly IRepository<Subject> _subjectRepository;

    public GetSubjectsByClassQueryHandler(IRepository<Subject> subjectRepository)
    {
        _subjectRepository = subjectRepository;
    }

    public async Task<ErrorOr<List<SubjectDto>>> Handle(GetSubjectsByClassQuery request, CancellationToken cancellationToken)
    {
        var subjects = await _subjectRepository.Query()
            .Where(s => s.ClassId == request.ClassId)
            .OrderBy(s => s.Name)
            .Select(s => new SubjectDto
            {
                Id = s.Id,
                Name = s.Name,
                ClassId = s.ClassId,
                ClassName = s.Class.Name
            })
            .ToListAsync(cancellationToken);

        return subjects;
    }
}
