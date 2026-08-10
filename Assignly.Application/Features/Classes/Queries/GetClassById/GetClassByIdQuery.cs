using Assignly.Application.Core.Abstractions;
using Assignly.Application.Dtos;

namespace Assignly.Application.Features.Classes.Queries.GetClassById;

public sealed record GetClassByIdQuery(Guid Id) : IQuery<ClassDto>;
