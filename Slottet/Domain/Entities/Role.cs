using Microsoft.AspNetCore.Identity;

namespace Domain.Entities;

public class Role : IdentityRole<int>
{
    public int RoleID { get; set; }
    public string ResponsibilityArea { get; set; } = string.Empty;

    // Navigation
    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
