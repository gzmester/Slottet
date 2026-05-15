using Domain.Enums;

namespace Application.DTOs.Authorization;

public class AuthorizationResponseDto
{
    public int AuthorizationID { get; set; }
    public AuthorizationRole Role { get; set; }
}
