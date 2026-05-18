using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Employee;

public class EmployeeUpdateDto
{
    [Required, MaxLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string LastName { get; set; } = string.Empty;

    [Required, MaxLength(80), EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string PhoneNumber { get; set; } = string.Empty;

    /// <summary>Optional: set new pincode. Leave empty to keep existing.</summary>
    public string? NewPincode { get; set; }

    [Required]
    public int LocationID { get; set; }

    [Required]
    public int AuthorizationID { get; set; }
}
