namespace Domain.Entities;

public class Location
{
    public int LocationID { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public int ZipCode { get; set; }

    // Navigation
    public ICollection<Resident> Residents { get; set; } = new List<Resident>();
    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
