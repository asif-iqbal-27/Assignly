using Assignly.Application.Core.Abstractions;
using Assignly.Application.Dtos;

namespace Assignly.Application.Features.Classes.Commands.CreateClass;

public sealed record CreateClassCommand(string Name, string? Section, string? Description) : ICommand<ClassDto>;
