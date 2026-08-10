namespace Assignly.Application.Dtos;

public sealed class ClassDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Section { get; set; }
    public string? Description { get; set; }
}
