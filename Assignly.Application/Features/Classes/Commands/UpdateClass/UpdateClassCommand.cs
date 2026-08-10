using Assignly.Application.Core.Abstractions;
using Assignly.Application.Dtos;

namespace Assignly.Application.Features.Classes.Commands.UpdateClass;

public sealed record UpdateClassCommand(Guid Id, string Name, string? Section, string? Description) : ICommand<ClassDto>;
