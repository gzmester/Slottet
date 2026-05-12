using Domain.Enums;

namespace Domain.Entities;

public class Authorization
{
    public int AuthorizationID { get; set; }
    public AuthorizationRole Role { get; set; }

    // Navigation
    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
}