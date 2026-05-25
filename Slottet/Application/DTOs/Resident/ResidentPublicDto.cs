namespace Application.DTOs.Resident;

/// <summary>
/// GDPR-venlig DTO til storskærm/offentlig visning.
/// Indeholder ingen personhenførbare oplysninger.
/// </summary>
public class ResidentPublicDto
{
    public int ResidentID { get; set; }
    public int LocationID { get; set; }

    // Risiko og humør vises — ingen navne, rum, statustekster eller betaling
    public string RiskLevel { get; set; } = string.Empty;
    public string Mood { get; set; } = "Neutral";

    // Medicin-tidspunkter og tagningsstatus vises — ingen medicinnavn
    public List<MedicinPublicDto> Medicins { get; set; } = new();
}
