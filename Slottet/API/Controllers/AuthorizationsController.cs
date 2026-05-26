using Application.DTOs.Authorization;
using Application.Interfaces.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

//[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AuthorizationsController : ControllerBase
{
    private readonly IAuthorizationRepository _repo;

    public AuthorizationsController(IAuthorizationRepository repo)
    {
        _repo = repo;
    }

    // GET /api/authorizations
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AuthorizationResponseDto>>> GetAll()
    {
        var authorizations = await _repo.GetAllAsync();

        return Ok(authorizations.Select(a => new AuthorizationResponseDto
        {
            AuthorizationID = a.AuthorizationID,
            Role            = a.Role
        }));
    }
}
