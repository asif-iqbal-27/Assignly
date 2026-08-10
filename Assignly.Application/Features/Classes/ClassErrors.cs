using ErrorOr;

namespace Assignly.Application.Features.Classes;

public static class ClassErrors
{
    public static Error NotFound(Guid id) => Error.NotFound(
        "Class.NotFound",
        $"Class with id '{id}' was not found.");

    public static readonly Error HasDependents = Error.Conflict(
        "Class.HasDependents",
        "Cannot delete a class that still has subjects, enrolled students, or assignments.");
}
