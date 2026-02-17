using System.ComponentModel.DataAnnotations;

namespace Slottet.Models;

/// <summary>
/// Repræsenterer en beboer på plejehjemmet
/// </summary>
public class Resident
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(11)]
    public string? CPR { get; set; }

    public DateTime DateOfBirth { get; set; }

    [MaxLength(200)]
    public string? RoomNumber { get; set; }

    [MaxLength(500)]
    public string? MedicalNotes { get; set; }

    public DateTime AdmissionDate { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public virtual ICollection<Medication> Medications { get; set; } = new List<Medication>();
}
