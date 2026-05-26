using API.Controllers;
using Application.DTOs.DepartmentTask;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Slottet.Tests;

// Unit tests for DepartmentTasksController.
// Afdelingsopgaver er simple CRUD-operationer uden auth eller komplekse relationer, så testene er ret straightforward.
// Bruger en in-memory database, så ingen rigtig DB-forbindelse er nødvendig.
public class DepartmentTasksControllerTests : IDisposable
{
    private readonly ApplicationDbContext _db;
    private readonly DepartmentTasksController _controller;

    public DepartmentTasksControllerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _db = new ApplicationDbContext(options);
        _controller = new DepartmentTasksController(_db);
    }

    [Fact]
    public async Task GetAll_TomDatabase_ReturnererTomListe()
    {
        var result = await _controller.GetAll();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var opgaver = Assert.IsAssignableFrom<IEnumerable<DepartmentTaskResponseDto>>(ok.Value);
        Assert.Empty(opgaver);
    }

    [Fact]
    public async Task Create_NyOpgave_GemmerOgReturnerer201()
    {
        var dto = new DepartmentTaskCreateDto { Name = "Rengøring" };

        var result = await _controller.Create(dto);

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        var opgave = Assert.IsType<DepartmentTaskResponseDto>(created.Value);
        Assert.Equal("Rengøring", opgave.Name);
        Assert.Equal(1, await _db.DepartmentTasks.CountAsync());
    }

    [Fact]
    public async Task Update_EksisterendeOpgave_OpdatererNavn()
    {
        _db.DepartmentTasks.Add(new DepartmentTask { DepartmentTaskID = 1, Name = "Gammel" });
        await _db.SaveChangesAsync();

        var dto = new DepartmentTaskUpdateDto { Name = "Ny" };
        var result = await _controller.Update(1, dto);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var opgave = Assert.IsType<DepartmentTaskResponseDto>(ok.Value);
        Assert.Equal("Ny", opgave.Name);
    }

    [Fact]
    public async Task Update_IkkeEksisterendeOpgave_Returnerer404()
    {
        var dto = new DepartmentTaskUpdateDto { Name = "X" };
        var result = await _controller.Update(999, dto);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task Delete_EksisterendeOpgave_FjernerfraDatabase()
    {
        _db.DepartmentTasks.Add(new DepartmentTask { DepartmentTaskID = 1, Name = "Slet mig" });
        await _db.SaveChangesAsync();

        var result = await _controller.Delete(1);

        Assert.IsType<NoContentResult>(result);
        Assert.Equal(0, await _db.DepartmentTasks.CountAsync());
    }

    [Fact]
    public async Task Delete_IkkeEksisterendeOpgave_Returnerer404()
    {
        var result = await _controller.Delete(999);

        Assert.IsType<NotFoundResult>(result);
    }

    public void Dispose() => _db.Dispose();
}
