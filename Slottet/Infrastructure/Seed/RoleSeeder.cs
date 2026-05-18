using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Seed;

/// <summary>
/// Seeder Identity-roller (adgangskontrol): Admin, Vagtansvarlig, Plejepersonale
/// Disse er IKKE det samme som Role-entiteten (ansvarsområder).
/// </summary>
public static class RoleSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<AppRole>>();

        var roles = new[]
        {
            new AppRole { Name = "Admin", Description = "Fuld adgang til alt" },
            new AppRole { Name = "Vagtansvarlig", Description = "Kan administrere vagter og ansatte" },
            new AppRole { Name = "Plejepersonale", Description = "Kan se og redigere beboerdata" }
        };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role.Name))
            {
                var result = await roleManager.CreateAsync(role);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    throw new Exception($"Kunne ikke oprette rolle '{role.Name}': {errors}");
                }
            }
        }
    }
}
