namespace Domain.Entities;

public class Role
{
    public int RoleID { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ResponsibilityArea { get; set; } = string.Empty;

    // Navigation: many-to-many with Employee via EmployeeRole join table
    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
