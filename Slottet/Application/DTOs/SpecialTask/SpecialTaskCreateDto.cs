namespace Application.DTOs.SpecialTask;

public class SpecialTaskCreateDto
{
    public string Title { get; set; } = string.Empty;
    public List<int> EmployeeIds { get; set; } = new();
}
