namespace Application.DTOs.Resident;

/// <summary>
/// GDPR-venlig DTO til storskærm/offentlig visning.
/// Indeholder ingen personhenførbare oplysninger.
/// </summary>
public class ResidentPublicDto
{
    public int ResidentID { get; set; }
    public int LocationID { get; set; }

    // Risiko og humør vises — ingen navne, rum, statustekster, medicin eller betaling
    public string RiskLevel { get; set; } = string.Empty;
    public string Mood { get; set; } = "Neutral";

    // Kun antal medicin og om nogen er taget — ingen tidspunkter
    public int MedicinCount { get; set; }
    public int MedicinTakenCount { get; set; }
}
