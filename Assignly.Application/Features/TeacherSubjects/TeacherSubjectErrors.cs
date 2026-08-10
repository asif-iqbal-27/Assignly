using ErrorOr;

namespace Assignly.Application.Features.TeacherSubjects;

public static class TeacherSubjectErrors
{
    public static Error NotFound(Guid id) => Error.NotFound(
        "TeacherSubject.NotFound",
        $"Teacher-subject assignment with id '{id}' was not found.");

    public static readonly Error AlreadyAssigned = Error.Conflict(
        "TeacherSubject.AlreadyAssigned",
        "This teacher is already assigned to this subject.");
}
