namespace Slottet.Models;

/// <summary>
/// Logger hvornår medicin er givet til beboere
/// </summary>
public class MedicationLog
{
    public int Id { get; set; }

    public int MedicationId { get; set; }

    public int AdministeredById { get; set; }

    public DateTime AdministeredAt { get; set; }

    public string? Notes { get; set; }

    public bool WasSkipped { get; set; } = false;

    public string? SkipReason { get; set; }

    // Navigation properties
    public virtual Medication Medication { get; set; } = null!;
    public virtual Staff AdministeredBy { get; set; } = null!;
}
