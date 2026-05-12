namespace Application.DTOs.Resident;

public class UpdateResidentRequestDto
{
    public int ResidentID { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Room { get; set; } = string.Empty;
    public string RiskLevel { get; set; } = string.Empty;
    public string Mood { get; set; } = "Neutral";
    public string ShoppingDay { get; set; } = string.Empty;
    public string Payment { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}
