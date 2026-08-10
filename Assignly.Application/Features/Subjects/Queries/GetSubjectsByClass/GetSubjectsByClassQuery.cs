using Assignly.Application.Core.Abstractions;
using Assignly.Application.Dtos;

namespace Assignly.Application.Features.Subjects.Queries.GetSubjectsByClass;

public sealed record GetSubjectsByClassQuery(Guid ClassId) : IQuery<List<SubjectDto>>;
