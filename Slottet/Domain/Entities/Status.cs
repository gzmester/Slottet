namespace Domain.Entities;

public class Status
{
    public int StatusID { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime Time { get; set; }

    // FK
    public int ResidentID { get; set; }
    public Resident Resident { get; set; } = null!;
}
