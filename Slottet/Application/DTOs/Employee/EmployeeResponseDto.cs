namespace Application.DTOs.Employee;

public class EmployeeResponseDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public bool HasPincode { get; set; }
    public int LocationID { get; set; }
    public string LocationName { get; set; } = string.Empty;

    /// <summary>Identity-roller fra AspNetRoles (Admin, Vagtansvarlig, Plejepersonale)</summary>
    public List<string> Roles { get; set; } = new();

    /// <summary>Ansvarsomraader fra Role-tabellen (Plejepersonale, Sygeplejerske, Koordinator)</summary>
    public List<string> JobRoles { get; set; } = new();
}
