using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.PhoneNumber;

public class PhoneNumberCreateDto
{
    [Required, MaxLength(8)]
    public string Number { get; set; } = string.Empty;
}