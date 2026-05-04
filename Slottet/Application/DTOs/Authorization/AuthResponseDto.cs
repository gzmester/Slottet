using System;

namespace Application.DTOs.Authorization;

public class LoginModel
{
    public int Pincode { get; set; }
}

public class AuthResponseDto
{
    public string Token { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
}
