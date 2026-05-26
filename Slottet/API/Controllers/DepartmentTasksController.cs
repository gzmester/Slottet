using Application.DTOs.DepartmentTask;
using Application.Interfaces.Repositories;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DepartmentTasksController : ControllerBase
{
    private readonly IDepartmentTasksRepository _repo;

    public DepartmentTasksController(IDepartmentTasksRepository repo)
    {
        _repo = repo;
    }

    // GET /api/departmenttasks
    [HttpGet]
    public async Task<ActionResult<IEnumerable<DepartmentTaskResponseDto>>> GetAll()
    {
        var tasks = await _repo.GetAllAsync();
        return Ok(tasks.Select(MapToDto));
    }

    // POST /api/departmenttasks
    [HttpPost]
    public async Task<ActionResult<DepartmentTaskResponseDto>> Create(DepartmentTaskCreateDto dto)
    {
        var task = new DepartmentTask { Name = dto.Name };
        _repo.Add(task);
        await _repo.SaveChangesAsync();
        return CreatedAtAction(nameof(GetAll), MapToDto(task));
    }

    // PUT /api/departmenttasks/{id}
    [HttpPut("{id}")]
    public async Task<ActionResult<DepartmentTaskResponseDto>> Update(int id, DepartmentTaskUpdateDto dto)
    {
        var task = await _repo.GetByIdAsync(id);
        if (task is null) return NotFound();

        task.Name = dto.Name;
        await _repo.SaveChangesAsync();
        return Ok(MapToDto(task));
    }

    // DELETE /api/departmenttasks/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var task = await _repo.GetByIdAsync(id);
        if (task is null) return NotFound();

        _repo.Remove(task);
        await _repo.SaveChangesAsync();
        return NoContent();
    }

    private static DepartmentTaskResponseDto MapToDto(DepartmentTask t) => new()
    {
        Id   = t.DepartmentTaskID,
        Name = t.Name
    };
}
