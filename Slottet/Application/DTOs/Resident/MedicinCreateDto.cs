using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Resident;

public class MedicinCreateDto
{
    //dto til at oprette medicin, bruges når man opretter en medicin for en beboer
    [Required, MaxLength(50)]
    public string Type { get; set; } = string.Empty;
    [Required]
    public int ResidentId { get; set; }
}