namespace Domain.Entities;

public class PNMedicin
{
    public int PNMedicinID { get; set; }
    public string Type { get; set; } = string.Empty;
    public DateTime Time { get; set; }

    // FK
    public int ResidentID { get; set; }
    public Resident Resident { get; set; } = null!;
}
