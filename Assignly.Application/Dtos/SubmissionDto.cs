namespace Assignly.Application.Dtos;

public sealed class SubmissionDto
{
    public Guid Id { get; set; }
    public Guid AssignmentId { get; set; }
    public string AssignmentTitle { get; set; } = string.Empty;
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string? Content { get; set; }
    public string? FileUrl { get; set; }
    public DateTime SubmittedAt { get; set; }
    public bool IsLate { get; set; }
    public int AttemptNumber { get; set; }
    public string Status { get; set; } = string.Empty;
    public int? Marks { get; set; }
    public string? Feedback { get; set; }
    public Guid? GradedByTeacherId { get; set; }
    public string? GradedByTeacherName { get; set; }
    public DateTime? GradedAt { get; set; }
}
