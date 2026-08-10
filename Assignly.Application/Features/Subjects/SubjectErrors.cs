using ErrorOr;

namespace Assignly.Application.Features.Subjects;

public static class SubjectErrors
{
    public static Error NotFound(Guid id) => Error.NotFound(
        "Subject.NotFound",
        $"Subject with id '{id}' was not found.");

    public static Error ClassNotFound(Guid classId) => Error.NotFound(
        "Subject.ClassNotFound",
        $"Class with id '{classId}' was not found.");

    public static readonly Error HasDependents = Error.Conflict(
        "Subject.HasDependents",
        "Cannot delete a subject that still has teacher assignments or assignments.");
}
