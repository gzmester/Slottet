using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Resident;

public class MedicinCreateDto
{
    [Required, MaxLength(50)]
    public string Type { get; set; } = string.Empty;
    [Required]
    public int ResidentId { get; set; }
}