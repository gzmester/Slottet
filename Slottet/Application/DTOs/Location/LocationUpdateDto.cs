using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Location;

public class LocationUpdateDto
{
    [Required, MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string Address { get; set; } = string.Empty;

    [Required]
    public int ZipCode { get; set; }
}
