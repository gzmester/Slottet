using System.ComponentModel.DataAnnotations;

namespace Slottet.Models;

/// <summary>
/// Repræsenterer medicin ordineret til en beboer
/// </summary>
public class Medication
{
    public int Id { get; set; }

    public int ResidentId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Dosage { get; set; }

    [MaxLength(100)]
    public string? Frequency { get; set; }

    [MaxLength(500)]
    public string? Instructions { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public virtual Resident Resident { get; set; } = null!;
    public virtual ICollection<MedicationLog> Logs { get; set; } = new List<MedicationLog>();
}
