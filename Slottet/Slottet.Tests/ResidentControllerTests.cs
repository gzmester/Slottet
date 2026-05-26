using API.Controllers;
using Application.DTOs.Resident;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Slottet.Tests;

// Unit tests for ResidentController.
// Bruger en in-memory database, så ingen rigtig DB-forbindelse er nødvendig.
public class ResidentControllerTests : IDisposable
{
    private readonly ApplicationDbContext _db;
    private readonly ResidentController _controller;

    public ResidentControllerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _db = new ApplicationDbContext(options);
        _controller = new ResidentController(_db);

        SeedData();
    }

    private void SeedData()
    {
        _db.Locations.Add(new Location
        {
            LocationID = 1,
            Name       = "Afdeling A",
            Address    = "Testgade 1",
            ZipCode    = 1234
        });

        _db.Residents.AddRange(
            new Resident
            {
                ResidentID   = 1,
                FirstName    = "Hans",
                LastName     = "Hansen",
                Room         = "101",
                RiskLevel    = RiskLevel.Green,
                Mood         = Mood.Happy,
                LocationID   = 1,
                ShoppingDay  = "Mandag",
                Payment      = "Ja"
            },
            new Resident
            {
                ResidentID   = 2,
                FirstName    = "Pia",
                LastName     = "Poulsen",
                Room         = "102",
                RiskLevel    = RiskLevel.Yellow,
                Mood         = Mood.Neutral,
                LocationID   = 1,
                ShoppingDay  = "Tirsdag",
                Payment      = "Nej"
            }
        );

        _db.SaveChanges();
    }

    [Fact]
    public async Task GetAll_ReturnererAlleBoebere()
    {
        var result = await _controller.GetAll();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var beboere = Assert.IsAssignableFrom<IEnumerable<ResidentResponseDto>>(ok.Value);
        Assert.Equal(2, beboere.Count());
    }

    [Fact]
    public async Task GetById_MedGyldigId_ReturnererKorrektBeboer()
    {
        var result = await _controller.GetById(1);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var beboer = Assert.IsType<ResidentResponseDto>(ok.Value);
        Assert.Equal(1, beboer.ResidentID);
        Assert.Equal("Hans", beboer.FirstName);
    }

    [Fact]
    public async Task GetById_MedIkkeEksisterendeId_Returnerer404()
    {
        var result = await _controller.GetById(999);

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetPublic_IndeholderIngenNavne()
    {
        var result = await _controller.GetPublic(null);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var beboere = Assert.IsAssignableFrom<IEnumerable<ResidentPublicDto>>(ok.Value).ToList();

        // ResidentPublicDto har ikke FirstName/LastName — kun ResidentID, RiskLevel, Mood, Medicins
        Assert.Equal(2, beboere.Count);
        Assert.All(beboere, b => Assert.True(b.ResidentID > 0));
    }

    [Fact]
    public async Task GetPublic_MedLocationId_ReturnererKunMatchendeLokation()
    {
        var result = await _controller.GetPublic(locationId: 1);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var beboere = Assert.IsAssignableFrom<IEnumerable<ResidentPublicDto>>(ok.Value).ToList();

        Assert.Equal(2, beboere.Count);
        Assert.All(beboere, b => Assert.Equal(1, b.LocationID));
    }

    [Fact]
    public async Task GetPublic_MedIkkeEksisterendeLokationId_ReturnererTomListe()
    {
        var result = await _controller.GetPublic(locationId: 999);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var beboere = Assert.IsAssignableFrom<IEnumerable<ResidentPublicDto>>(ok.Value).ToList();

        Assert.Empty(beboere);
    }

    [Fact]
    public async Task GetAll_ReturnererKorrektRisikoniveau()
    {
        var result = await _controller.GetAll();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var beboere = Assert.IsAssignableFrom<IEnumerable<ResidentResponseDto>>(ok.Value).ToList();

        var hans = beboere.Single(b => b.ResidentID == 1);
        Assert.Equal("Green", hans.RiskLevel);
    }

    public void Dispose() => _db.Dispose();
}
