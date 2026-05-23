namespace Application.DTOs.SpecialTask;

public class SpecialTaskResponseDto
{
    public int SpecialTaskID { get; set; }
    public string Title { get; set; } = string.Empty;
    public List<AssignedEmployeeDto> Employees { get; set; } = new();
}

public class AssignedEmployeeDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
}
