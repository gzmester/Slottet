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
    public int PhoneNumber { get; set; }

    [Required]
    public string ShiftType { get; set; } = string.Empty;

    [Required]
    public int PinCode { get; set; }

    [Required]
    public int LocationID { get; set; }

    [Required]
    public int AuthorizationID { get; set; }
}
