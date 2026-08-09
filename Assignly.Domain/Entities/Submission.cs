using Assignly.Domain.Enums;

namespace Assignly.Domain.Entities;

public class Submission
{
    public Guid Id { get; set; }

    public Guid AssignmentId { get; set; }
    public Assignment Assignment { get; set; } = null!;

    public Guid StudentId { get; set; }
    public ApplicationUser Student { get; set; } = null!;

    public string? Content { get; set; }
    public string? FileUrl { get; set; }

    public DateTime SubmittedAt { get; set; }
    public bool IsLate { get; set; }
    public int AttemptNumber { get; set; } = 1;
    public SubmissionStatus Status { get; set; } = SubmissionStatus.Submitted;

    public int? Marks { get; set; }
    public string? Feedback { get; set; }
    public Guid? GradedByTeacherId { get; set; }
    public ApplicationUser? GradedByTeacher { get; set; }
    public DateTime? GradedAt { get; set; }
}
