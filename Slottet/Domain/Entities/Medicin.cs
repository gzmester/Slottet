namespace Domain.Entities;

public class Medicin
{
    public int MedicinID { get; set; }
    public DateTime Time { get; set; }

    // FK
    public int ResidentID { get; set; }
    public Resident Resident { get; set; } = null!;
}
