namespace Application.DTOs.Authorization;

public class AuthorizationResponseDto
{
    public int AuthorizationID { get; set; }
    public bool Substitute { get; set; }
    public bool Employee { get; set; }
    public bool Scheduler { get; set; }
    public bool Admin { get; set; }
}
