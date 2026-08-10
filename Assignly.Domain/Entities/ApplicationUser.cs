using Assignly.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace Assignly.Domain.Entities;

public class ApplicationUser : IdentityUser<Guid>
{
    public string FullName { get; set; } = string.Empty;
    public RoleType? Role { get; set; }
    public bool IsActive { get; set; } = true;

    public Guid? ClassId { get; set; }
    public SchoolClass? Class { get; set; }
}
