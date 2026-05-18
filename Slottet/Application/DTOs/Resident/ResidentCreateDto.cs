using System.ComponentModel.DataAnnotations;
using Domain.Enums;

namespace Application.DTOs.Resident;

public class ResidentCreateDto
{
    [Required, MaxLength(20)]
    public string FirstName { get; set; } = string.Empty;
    [Required, MaxLength(20)]
    public string LastName { get; set; } = string.Empty;
    [Required, MaxLength(5)]
    public string Room { get; set; } = string.Empty;
    [Required]
    public RiskLevel RiskLevel { get; set; }
    [Required]
    public Mood Mood { get; set; }
    [Required, MaxLength(20)]
    public string ShoppingDay { get; set; } = string.Empty;
    [Required, MaxLength(20)]
    public string Payment { get; set; } = string.Empty;
    [Required]
    public int LocationID { get; set; }

}