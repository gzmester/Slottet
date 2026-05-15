using Application.DTOs.Authorization;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace API.Controllers;

//[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AuthorizationsController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public AuthorizationsController(ApplicationDbContext db)
    {
        _db = db;
    }

    // GET /api/authorizations
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AuthorizationResponseDto>>> GetAll()
    {
        var authorizations = await _db.Authorizations
            .Select(a => new AuthorizationResponseDto
            {
                AuthorizationID = a.AuthorizationID,
                Role            = a.Role
            })
            .ToListAsync();

        return Ok(authorizations);
    }
}
