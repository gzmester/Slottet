using Application.DTOs.SpecialTask;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SpecialTasksController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public SpecialTasksController(ApplicationDbContext db)
    {
        _db = db;
    }

    // GET /api/specialtasks
    // Returns all responsibility roles with their assigned employees
    [HttpGet]
    public async Task<ActionResult<IEnumerable<SpecialTaskResponseDto>>> GetAll()
    {
        var roles = await _db.ResponsibilityRoles
            .Include(r => r.Employees)
            .ToListAsync();

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
            var employees = await _db.Users
                .Where(e => dto.EmployeeIds.Contains(e.Id))
                .ToListAsync();

            foreach (var emp in employees)
                role.Employees.Add(emp);
        }

        _db.ResponsibilityRoles.Add(role);
        await _db.SaveChangesAsync();

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
        var role = await _db.ResponsibilityRoles
            .Include(r => r.Employees)
            .FirstOrDefaultAsync(r => r.RoleID == id);

        if (role is null) return NotFound();

        role.Employees.Clear();

        if (dto.EmployeeIds.Any())
        {
            var employees = await _db.Users
                .Where(e => dto.EmployeeIds.Contains(e.Id))
                .ToListAsync();

            foreach (var emp in employees)
                role.Employees.Add(emp);
        }

        await _db.SaveChangesAsync();

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
        var role = await _db.ResponsibilityRoles.FindAsync(id);
        if (role is null) return NotFound();

        _db.ResponsibilityRoles.Remove(role);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
