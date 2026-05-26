using Domain.Entities;
using Domain.Enums;
using Xunit;

namespace Slottet.Tests;

// Rene unit tests for domænentiteter.
// Ingen database eller dependencies — verificerer forretningslogik isoleret.
public class DomainEntityTests
{
    // ----- Resident -----

    [Fact]
    public void Resident_DefaultMood_ErNeutral()
    {
        var beboer = new Resident();
        Assert.Equal(Mood.Neutral, beboer.Mood);
    }

    [Fact]
    public void Resident_KanSaetteRisikoniveau()
    {
        var beboer = new Resident { RiskLevel = RiskLevel.Red };
        Assert.Equal(RiskLevel.Red, beboer.RiskLevel);
    }

    [Theory]
    [InlineData(RiskLevel.Green)]
    [InlineData(RiskLevel.Yellow)]
    [InlineData(RiskLevel.Red)]
    public void RiskLevel_AlleVaerdierErGyldige(RiskLevel niveau)
    {
        var beboer = new Resident { RiskLevel = niveau };
        Assert.Equal(niveau, beboer.RiskLevel);
    }

    [Theory]
    [InlineData(Mood.Happy)]
    [InlineData(Mood.Neutral)]
    [InlineData(Mood.Sad)]
    public void Mood_AlleVaerdierErGyldige(Mood humør)
    {
        var beboer = new Resident { Mood = humør };
        Assert.Equal(humør, beboer.Mood);
    }

    [Fact]
    public void Resident_DefaultNavne_ErTomStrenge()
    {
        var beboer = new Resident();
        Assert.Equal(string.Empty, beboer.FirstName);
        Assert.Equal(string.Empty, beboer.LastName);
    }

    // ----- Employee -----

    [Fact]
    public void Employee_UdenPasswordHash_HasPincodeErFalse()
    {
        var medarbejder = new Employee { PasswordHash = null };
        Assert.False(medarbejder.HasPincode);
    }

    [Fact]
    public void Employee_MedPasswordHash_HasPincodeErTrue()
    {
        var medarbejder = new Employee { PasswordHash = "AQAAAAIAAYagAAAA..." };
        Assert.True(medarbejder.HasPincode);
    }

    [Fact]
    public void Employee_TomPasswordHash_HasPincodeErFalse()
    {
        var medarbejder = new Employee { PasswordHash = string.Empty };
        Assert.False(medarbejder.HasPincode);
    }

    // ----- Location -----

    [Fact]
    public void Location_DefaultLister_ErIkkeNull()
    {
        var lokation = new Location();
        Assert.NotNull(lokation.Residents);
        Assert.NotNull(lokation.Employees);
    }

    // ----- AuditLog -----

    [Fact]
    public void AuditLog_TimeStamp_SaettesAutomatisk()
    {
        var foer = DateTime.UtcNow.AddSeconds(-1);
        var log = new AuditLog();
        var efter = DateTime.UtcNow.AddSeconds(1);

        Assert.InRange(log.TimeStamp, foer, efter);
    }
}
