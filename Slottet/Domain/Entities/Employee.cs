using Microsoft.AspNetCore.Identity;

namespace Domain.Entities;

public class Employee : IdentityUser<int>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    /// <summary>Whether the employee has set up their pincode (PasswordHash) yet</summary>
    public bool HasPincode => !string.IsNullOrEmpty(PasswordHash);

    // FKs
    public int LocationID { get; set; }
    public Location Location { get; set; } = null!;

    public int AuthorizationID { get; set; }
    public Authorization Authorization { get; set; } = null!;

    // Many-to-many: Ansvarsområder (job descriptions) via EmployeeRole join table
    public ICollection<Role> Roles { get; set; } = new List<Role>();

    // One-to-many: Vagter
    public ICollection<Shift> Shifts { get; set; } = new List<Shift>();
}
