using ErrorOr;

namespace Assignly.Application.Features.ClassSubjectTeachers;

public static class ClassSubjectTeacherErrors
{
    public static Error NotFound(Guid id) => Error.NotFound(
        "ClassSubjectTeacher.NotFound",
        $"Class-subject-teacher assignment with id '{id}' was not found.");

    public static readonly Error AlreadyAssigned = Error.Conflict(
        "ClassSubjectTeacher.AlreadyAssigned",
        "This teacher is already assigned to this subject.");
}
