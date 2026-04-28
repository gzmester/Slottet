using Domain.Enums;

namespace Domain.Entities;

public class Shift
{
    public int ShiftID { get; set; }
    public ShiftType ShiftType { get; set; }
    public DateTime Date { get; set; }

    // FK
    public int EmployeeID { get; set; }
    public Employee Employee { get; set; } = null!;
}
