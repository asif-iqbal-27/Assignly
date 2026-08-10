using Assignly.Application.Core.Abstractions;
using Assignly.Application.Dtos;

namespace Assignly.Application.Features.Classes.Queries.GetClasses;

public sealed record GetClassesQuery : IQuery<List<ClassDto>>;
