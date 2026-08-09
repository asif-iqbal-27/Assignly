namespace Assignly.Domain.Entities;

public class SchoolClass
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Section { get; set; }
    public string? Description { get; set; }

    public ICollection<Subject> Subjects { get; set; } = new List<Subject>();
    public ICollection<ApplicationUser> Students { get; set; } = new List<ApplicationUser>();
    public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
}
