using ErrorOr;

namespace Assignly.Application.Features.Submissions;

public static class SubmissionErrors
{
    public static Error NotFound(Guid id) => Error.NotFound(
        "Submission.NotFound",
        $"Submission with id '{id}' was not found.");

    public static Error AssignmentNotFound(Guid assignmentId) => Error.NotFound(
        "Submission.AssignmentNotFound",
        $"Assignment with id '{assignmentId}' was not found.");

    public static readonly Error AlreadySubmitted = Error.Conflict(
        "Submission.AlreadySubmitted",
        "You have already submitted this assignment. Use the resubmit endpoint to update it.");

    public static readonly Error DeadlinePassed = Error.Conflict(
        "Submission.DeadlinePassed",
        "The deadline for this assignment has passed and late submissions are not allowed.");

    public static readonly Error ResubmissionNotAllowed = Error.Conflict(
        "Submission.ResubmissionNotAllowed",
        "This assignment does not allow resubmission.");

    public static readonly Error ResubmissionWindowClosed = Error.Conflict(
        "Submission.ResubmissionWindowClosed",
        "The deadline has passed; resubmission is no longer allowed.");

    public static readonly Error NotOwner = Error.Forbidden(
        "Submission.NotOwner",
        "You are not assigned to this subject via a ClassSubjectTeacher.");

    public static Error MarksOutOfRange(int maxMarks) => Error.Validation(
        "Submission.MarksOutOfRange",
        $"Marks must be between 0 and {maxMarks}.");
}
