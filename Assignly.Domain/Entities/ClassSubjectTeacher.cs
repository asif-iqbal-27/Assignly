namespace Assignly.Domain.Entities;

public class ClassSubjectTeacher
{
    public Guid Id { get; set; }

    public Guid TeacherId { get; set; }
    public ApplicationUser Teacher { get; set; } = null!;

    public Guid SubjectId { get; set; }
    public Subject Subject { get; set; } = null!;
}
