using Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace Domain.Entities;

public class Employee : IdentityUser<int>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
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
