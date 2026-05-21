namespace Application.DTOs.Resident;

public class PNMedicinCreateDto
{
    public string Type { get; set; } = string.Empty;
    public DateTime Time { get; set; }
    public int ResidentId { get; set; }
}

