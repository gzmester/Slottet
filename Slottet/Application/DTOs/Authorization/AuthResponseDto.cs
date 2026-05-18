using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Authorization;

public class LoginModel
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    /// <summary>Brugerens pinkode (4-6 cifre)</summary>
    [Required]
    public string Pincode { get; set; } = string.Empty;
}

public class SetupPincodeModel
{
    [Required]
    public int EmployeeId { get; set; }

    /// <summary>Ny pinkode (4-6 cifre)</summary>
    [Required, MinLength(4), MaxLength(6)]
    public string Pincode { get; set; } = string.Empty;
}

public class AuthResponseDto
{
    public string Token { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public int EmployeeId { get; set; }

    /// <summary>Sand hvis brugeren ikke har oprettet pinkode endnu</summary>
    public bool RequiresSetup { get; set; }

    /// <summary>Besked til frontend (fx "Opret en pinkode for at fortsætte")</summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>Adgangsrolle fra Authorization-tabellen (CareStaff, Scheduler, Admin, Substitute)</summary>
    public string AuthorizationRole { get; set; } = string.Empty;

    /// <summary>Identity-roller for adgangskontrol (Admin, Vagtansvarlig, Plejepersonale)</summary>
    public List<string> IdentityRoles { get; set; } = new();

    /// <summary>Aktuel vagttype baseret på dags dato (Day, Midday, Night)</summary>
    public string CurrentShiftType { get; set; } = string.Empty;

    public int LocationID { get; set; }
}

public class AssignRoleModel
{
    [Required]
    public int EmployeeId { get; set; }

    /// <summary>Identity-rolle: Admin, Vagtansvarlig eller Plejepersonale</summary>
    [Required]
    public string Role { get; set; } = string.Empty;
}
