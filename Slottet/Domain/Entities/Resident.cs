using Domain.Enums;

namespace Domain.Entities;

public class Resident
{
    public int ResidentID { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Room { get; set; } = string.Empty;
    public RiskLevel RiskLevel { get; set; }
    public string ShoppingDay { get; set; } = string.Empty;
    public string Payment { get; set; } = string.Empty;

    // FKd
    public int LocationID { get; set; }
    public Location Location { get; set; } = null!;

    // Navigation
    public ICollection<Medicin> Medicins { get; set; } = new List<Medicin>();
    public ICollection<PNMedicin> PNMedicins { get; set; } = new List<PNMedicin>();
    public ICollection<Status> Statuses { get; set; } = new List<Status>();
}
