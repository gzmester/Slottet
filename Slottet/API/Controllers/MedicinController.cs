using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MedicinController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public MedicinController(ApplicationDbContext db)
    {
        _db = db;
    }

    // PATCH /api/medicin/{id}/taken?isTaken=true
    [HttpPatch("{id}/taken")]
    public async Task<IActionResult> SetTaken(int id, [FromQuery] bool isTaken)
    {
        var medicin = await _db.Medicins.FindAsync(id);

        if (medicin is null)
            return NotFound();

        medicin.IsTaken = isTaken;
        await _db.SaveChangesAsync();

        return NoContent();
    }
}
