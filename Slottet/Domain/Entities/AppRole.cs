using Microsoft.AspNetCore.Identity;

namespace Domain.Entities;

/// <summary>
/// Identity-rolle til adgangskontrol: Admin, Vagtansvarlig, Plejepersonale
/// Dette er IKKE det samme som Role-entiteten (ansvarsområder).
/// </summary>
public class AppRole : IdentityRole<int>
{
    public string Description { get; set; } = string.Empty;
}
