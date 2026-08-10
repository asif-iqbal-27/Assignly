using Assignly.Application.Core.Abstractions;
using Assignly.Application.Dtos;

namespace Assignly.Application.Features.Subjects.Queries.GetSubjects;

public sealed record GetSubjectsQuery : IQuery<List<SubjectDto>>;
