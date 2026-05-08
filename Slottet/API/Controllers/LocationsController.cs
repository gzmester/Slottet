using Application.DTOs.Location;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace API.Controllers;

//[Authorize]
[ApiController]
[Route("api/[controller]")]
public class LocationsController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public LocationsController(ApplicationDbContext db)
    {
        _db = db;
    }

    // GET /api/locations
    [HttpGet]
    public async Task<ActionResult<IEnumerable<LocationResponseDto>>> GetAll()
    {
        var locations = await _db.Locations
            .Select(l => new LocationResponseDto
            {
                LocationID = l.LocationID,
                Name       = l.Name,
                Address    = l.Address,
                ZipCode    = l.ZipCode
            })
            .ToListAsync();

        return Ok(locations);
    }
}
