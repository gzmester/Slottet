namespace Domain.Entities;

public class Authorization
{
    public int AuthorizationID { get; set; }
    public bool Substitute { get; set; }
    public bool Employee { get; set; }
    public bool Scheduler { get; set; }
    public bool Admin { get; set; }

    // Navigation
    public ICollection<Domain.Entities.Employee> Employees { get; set; } = new List<Domain.Entities.Employee>();
}
