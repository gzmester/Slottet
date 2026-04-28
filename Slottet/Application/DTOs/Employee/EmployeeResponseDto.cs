namespace Application.DTOs.Employee;

public class EmployeeResponseDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public int PinCode { get; set; }
    public int LocationID { get; set; }
    public string LocationName { get; set; } = string.Empty;
    public int AuthorizationID { get; set; }
    public List<string> Roles { get; set; } = new();
}
