using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Employee;

public class EmployeeCreateDto
{
    [Required, MaxLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string LastName { get; set; } = string.Empty;

    [Required, MaxLength(80), EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required]
    public int LocationID { get; set; }

    /// <summary>Identity-rolle: Admin, Vagtansvarlig eller Plejepersonale</summary>
    [Required]
    public string Role { get; set; } = "Plejepersonale";
}
