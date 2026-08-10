using Assignly.Application.Core.Abstractions;
using ErrorOr;

namespace Assignly.Application.Features.Classes.Commands.DeleteClass;

public sealed record DeleteClassCommand(Guid Id) : ICommand<Deleted>;
