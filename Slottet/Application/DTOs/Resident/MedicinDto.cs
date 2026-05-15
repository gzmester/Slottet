namespace Application.DTOs.Resident;

public class MedicinDto
{
    public int MedicinID { get; set; }
    public string Type { get; set; } = string.Empty;
    public DateTime Time { get; set; }
    public bool IsTaken { get; set; }
}
