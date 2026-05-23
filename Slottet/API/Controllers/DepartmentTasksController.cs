using Application.DTOs.DepartmentTask;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DepartmentTasksController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public DepartmentTasksController(ApplicationDbContext db)
    {
        _db = db;
    }

    // GET /api/departmenttasks
    [HttpGet]
    public async Task<ActionResult<IEnumerable<DepartmentTaskResponseDto>>> GetAll()
    {
        var tasks = await _db.DepartmentTasks.ToListAsync();
        return Ok(tasks.Select(MapToDto));
    }

    // POST /api/departmenttasks
    [HttpPost]
    public async Task<ActionResult<DepartmentTaskResponseDto>> Create(DepartmentTaskCreateDto dto)
    {
        var task = new DepartmentTask { Name = dto.Name };
        _db.DepartmentTasks.Add(task);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetAll), MapToDto(task));
    }

    // PUT /api/departmenttasks/{id}
    [HttpPut("{id}")]
    public async Task<ActionResult<DepartmentTaskResponseDto>> Update(int id, DepartmentTaskUpdateDto dto)
    {
        var task = await _db.DepartmentTasks.FindAsync(id);
        if (task is null) return NotFound();

        task.Name = dto.Name;
        await _db.SaveChangesAsync();
        return Ok(MapToDto(task));
    }

    // DELETE /api/departmenttasks/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var task = await _db.DepartmentTasks.FindAsync(id);
        if (task is null) return NotFound();

        _db.DepartmentTasks.Remove(task);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    private static DepartmentTaskResponseDto MapToDto(DepartmentTask t) => new()
    {
        Id   = t.DepartmentTaskID,
        Name = t.Name
    };
}
