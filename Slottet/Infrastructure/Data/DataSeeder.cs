using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Data;

public static class DataSeeder
{

    // blev lige genereret af claude så vi kan seede noget data til databasen. 
    public static async Task SeedAsync(ApplicationDbContext db, ILogger logger)
    {
        // Only seed when database is empty
        if (await db.Locations.AnyAsync())
        {
            logger.LogInformation("Database already contains data – skipping seed.");
            return;
        }

        logger.LogInformation("Seeding database with initial data...");

        // --- Locations ---
        var location1 = new Location
        {
            Name = "Slottet Afdeling A",
            Address = "Slottsvej 1",
            ZipCode = 8000
        };
        var location2 = new Location
        {
            Name = "Slottet Afdeling B",
            Address = "Slottsvej 2",
            ZipCode = 8000
        };
        db.Locations.AddRange(location1, location2);
        await db.SaveChangesAsync();

        // --- Authorizations ---
        var authAdmin = new Authorization
        {
            Substitute = false,
            Employee = true,
            Scheduler = true,
            Admin = true
        };
        var authEmployee = new Authorization
        {
            Substitute = false,
            Employee = true,
            Scheduler = false,
            Admin = false
        };
        var authSubstitute = new Authorization
        {
            Substitute = true,
            Employee = false,
            Scheduler = false,
            Admin = false
        };
        db.Authorizations.AddRange(authAdmin, authEmployee, authSubstitute);
        await db.SaveChangesAsync();

        // --- Roles ---
        var roleCaregiver = new Role
        {
            Name = "Plejepersonale",
            ResponsibilityArea = "Daglig pleje og medicin"
        };
        var roleNurse = new Role
        {
            Name = "Sygeplejerske",
            ResponsibilityArea = "Medicinsk behandling og vurdering"
        };
        var roleCoordinator = new Role
        {
            Name = "Koordinator",
            ResponsibilityArea = "Planlægning og vagtskema"
        };
        db.Roles.AddRange(roleCaregiver, roleNurse, roleCoordinator);
        await db.SaveChangesAsync();

        // --- Employees ---
        var emp1 = new Employee
        {
            FirstName = "Mette",
            LastName = "Hansen",
            Email = "mette.hansen@slottet.dk",
            PhoneNumber = "12345678",
            PinCode = 123456,
            LocationID = location1.LocationID,
            AuthorizationID = authAdmin.AuthorizationID,
            Roles = new List<Role> { roleCoordinator, roleNurse }
        };
        var emp2 = new Employee
        {
            FirstName = "Lars",
            LastName = "Jensen",
            Email = "lars.jensen@slottet.dk",
            PhoneNumber = "87654321",
            PinCode = 567890,
            LocationID = location1.LocationID,
            AuthorizationID = authEmployee.AuthorizationID,
            Roles = new List<Role> { roleCaregiver }
        };
        var emp3 = new Employee
        {
            FirstName = "Sofie",
            LastName = "Nielsen",
            Email = "sofie.nielsen@slottet.dk",
            PhoneNumber = "11223344",
            PinCode = 901234,
            LocationID = location2.LocationID,
            AuthorizationID = authEmployee.AuthorizationID,
            Roles = new List<Role> { roleCaregiver, roleNurse }
        };
        db.Employees.AddRange(emp1, emp2, emp3);
        await db.SaveChangesAsync();

        // --- Shifts ---
        db.Shifts.AddRange(
            new Shift { ShiftType = ShiftType.Day,    Date = DateTime.Today, EmployeeID = emp1.Id },
            new Shift { ShiftType = ShiftType.Night,  Date = DateTime.Today, EmployeeID = emp2.Id },
            new Shift { ShiftType = ShiftType.Midday, Date = DateTime.Today, EmployeeID = emp3.Id }
        );
        await db.SaveChangesAsync();

        // --- Residents ---
        var res1 = new Resident
        {
            FirstName = "Poul",
            LastName = "Mortensen",
            Room = "101",
            RiskLevel = RiskLevel.Green,
            ShoppingDay = "Mandag",
            Payment = "Folkepension",
            LocationID = location1.LocationID
        };
        var res2 = new Resident
        {
            FirstName = "Inger",
            LastName = "Christensen",
            Room = "102",
            RiskLevel = RiskLevel.Yellow,
            ShoppingDay = "Torsdag",
            Payment = "Privat betaling",
            LocationID = location1.LocationID
        };
        var res3 = new Resident
        {
            FirstName = "Bjarne",
            LastName = "Pedersen",
            Room = "201",
            RiskLevel = RiskLevel.Red,
            ShoppingDay = "Fredag",
            Payment = "Kommunal støtte",
            LocationID = location2.LocationID
        };
        var res4 = new Resident
        {
            FirstName = "Marianne",
            LastName = "Andersen",
            Room = "103",
            RiskLevel = RiskLevel.Green,
            ShoppingDay = "Onsdag",
            Payment = "Folkepension",
            LocationID = location1.LocationID
        };
        var res5 = new Resident
        {
            FirstName = "Erik",
            LastName = "Larsen",
            Room = "202",
            RiskLevel = RiskLevel.Yellow,
            ShoppingDay = "Tirsdag",
            Payment = "Privat betaling",
            LocationID = location2.LocationID
        };
        var res6 = new Resident
        {
            FirstName = "Gertrud",
            LastName = "Sørensen",
            Room = "203",
            RiskLevel = RiskLevel.Red,
            ShoppingDay = "Mandag",
            Payment = "Kommunal støtte",
            LocationID = location2.LocationID
        };
        var res7 = new Resident
        {
            FirstName = "Knud",
            LastName = "Thomsen",
            Room = "104",
            RiskLevel = RiskLevel.Green,
            ShoppingDay = "Lørdag",
            Payment = "Folkepension",
            LocationID = location1.LocationID
        };
        db.Residents.AddRange(res1, res2, res3, res4, res5, res6, res7);
        await db.SaveChangesAsync();

        // --- Medicin ---
        db.Medicins.AddRange(
            new Medicin { ResidentID = res1.ResidentID, Time = DateTime.Today.AddHours(8) },
            new Medicin { ResidentID = res1.ResidentID, Time = DateTime.Today.AddHours(20) },
            new Medicin { ResidentID = res2.ResidentID, Time = DateTime.Today.AddHours(8) }
        );
        await db.SaveChangesAsync();

        // --- PNMedicin ---
        db.PNMedicins.AddRange(
            new PNMedicin { ResidentID = res2.ResidentID, Type = "Smertestillende", Time = DateTime.Today.AddHours(14) },
            new PNMedicin { ResidentID = res3.ResidentID, Type = "Beroligende", Time = DateTime.Today.AddHours(22) }
        );
        await db.SaveChangesAsync();

        // --- Status ---
        db.Statuses.AddRange(
            new Status { ResidentID = res1.ResidentID, Description = "Har det godt, spist morgenmad", Time = DateTime.Now.AddHours(-2) },
            new Status { ResidentID = res2.ResidentID, Description = "Lidt urolig, fik PN medicin", Time = DateTime.Now.AddHours(-1) },
            new Status { ResidentID = res3.ResidentID, Description = "Sengeliggende, behøver ekstra tilsyn", Time = DateTime.Now }
        );
        await db.SaveChangesAsync();

        logger.LogInformation("Seeding completed.");
    }
}
