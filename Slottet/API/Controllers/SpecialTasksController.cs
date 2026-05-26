using Application.DTOs.SpecialTask;
using Application.Interfaces.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SpecialTasksController : ControllerBase
{
    private readonly ISpecialTasksRepository _repo;

    public SpecialTasksController(ISpecialTasksRepository repo)
    {
        _repo = repo;
    }

    // GET /api/specialtasks
    // Returns all responsibility roles with their assigned employees
    [HttpGet]
    public async Task<ActionResult<IEnumerable<SpecialTaskResponseDto>>> GetAll()
    {
        var roles = await _repo.GetAllAsync();

        return Ok(roles.Select(r => new SpecialTaskResponseDto
        {
            SpecialTaskID = r.RoleID,
            Title         = r.ResponsibilityArea,
            Employees     = r.Employees.Select(e => new AssignedEmployeeDto
            {
                Id        = e.Id,
                FirstName = e.FirstName,
                LastName  = e.LastName
            }).ToList()
        }));
    }

    // POST /api/specialtasks
    // Creates a new responsibility role (task)
    [HttpPost]
    public async Task<ActionResult<SpecialTaskResponseDto>> Create(SpecialTaskCreateDto dto)
    {
        var role = new Domain.Entities.Role
        {
            Name               = string.Empty,
            ResponsibilityArea = dto.Title
        };

        if (dto.EmployeeIds.Any())
        {
            var employees = await _repo.GetEmployeesByIdsAsync(dto.EmployeeIds);

            foreach (var emp in employees)
                role.Employees.Add(emp);
        }

        _repo.Add(role);
        await _repo.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAll), new SpecialTaskResponseDto
        {
            SpecialTaskID = role.RoleID,
            Title         = role.Name,
            Employees     = role.Employees.Select(e => new AssignedEmployeeDto
            {
                Id        = e.Id,
                FirstName = e.FirstName,
                LastName  = e.LastName
            }).ToList()
        });
    }

    // PUT /api/specialtasks/{id}/employees
    // Updates which employees are assigned to a task
    [HttpPut("{id}/employees")]
    public async Task<ActionResult<SpecialTaskResponseDto>> UpdateEmployees(int id, SpecialTaskUpdateEmployeesDto dto)
    {
        var role = await _repo.GetByIdWithEmployeesAsync(id);

        if (role is null) return NotFound();

        role.Employees.Clear();

        if (dto.EmployeeIds.Any())
        {
            var employees = await _repo.GetEmployeesByIdsAsync(dto.EmployeeIds);

            foreach (var emp in employees)
                role.Employees.Add(emp);
        }

        await _repo.SaveChangesAsync();

        return Ok(new SpecialTaskResponseDto
        {
            SpecialTaskID = role.RoleID,
            Title         = role.Name,
            Employees     = role.Employees.Select(e => new AssignedEmployeeDto
            {
                Id        = e.Id,
                FirstName = e.FirstName,
                LastName  = e.LastName
            }).ToList()
        });
    }

    // DELETE /api/specialtasks/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var role = await _repo.GetByIdWithEmployeesAsync(id);
        if (role is null) return NotFound();

        _repo.Remove(role);
        await _repo.SaveChangesAsync();
        return NoContent();
    }
}
