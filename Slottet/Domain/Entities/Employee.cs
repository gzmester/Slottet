using Domain.Enums;

namespace Domain.Entities;

public class Employee
{
    public int EmployeeID { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int PhoneNumber { get; set; }
    public ShiftType ShiftType { get; set; }
    public int PinCode { get; set; }

    // FKs
    public int LocationID { get; set; }
    public Location Location { get; set; } = null!;

    public int AuthorizationID { get; set; }
    public Authorization Authorization { get; set; } = null!;

    // Many-to-many via linking table
    public ICollection<Role> Roles { get; set; } = new List<Role>();
}
