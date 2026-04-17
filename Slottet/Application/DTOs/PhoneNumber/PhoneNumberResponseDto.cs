namespace Application.DTOs.PhoneNumber;

public class PhoneNumberResponseDto
{
    public int Id { get; set; }
    public string Number { get; set; } = string.Empty;
    public string? AssignedTo { get; set; }
}