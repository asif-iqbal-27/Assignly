using ErrorOr;

namespace Assignly.Application.Features.Assignments;

public static class AssignmentErrors
{
    public static Error NotFound(Guid id) => Error.NotFound(
        "Assignment.NotFound",
        $"Assignment with id '{id}' was not found.");

    public static readonly Error NotOwner = Error.Forbidden(
        "Assignment.NotOwner",
        "You are not assigned to this subject via a TeacherSubjectAssignment.");

    public static readonly Error HasSubmissions = Error.Conflict(
        "Assignment.HasSubmissions",
        "Cannot delete an assignment that already has submissions.");

    public static readonly Error AlreadyPublished = Error.Conflict(
        "Assignment.AlreadyPublished",
        "This assignment has already been published.");
}
