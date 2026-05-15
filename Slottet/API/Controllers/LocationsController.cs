using Application.DTOs.Location;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

    // GET /api/locations/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<LocationResponseDto>> GetById(int id)
    {
        var location = await _db.Locations.FindAsync(id);

        if (location is null)
            return NotFound();

        return Ok(MapToResponseDto(location));
    }

    // POST /api/locations
    [HttpPost]
    public async Task<ActionResult<LocationResponseDto>> Create(LocationCreateDto dto)
    {
        var location = new Location
        {
            Name    = dto.Name,
            Address = dto.Address,
            ZipCode = dto.ZipCode
        };

        _db.Locations.Add(location);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = location.LocationID }, MapToResponseDto(location));
    }

    // PUT /api/locations/{id}
    [HttpPut("{id}")]
    public async Task<ActionResult<LocationResponseDto>> Update(int id, LocationUpdateDto dto)
    {
        var location = await _db.Locations.FindAsync(id);

        if (location is null)
            return NotFound();

        location.Name    = dto.Name;
        location.Address = dto.Address;
        location.ZipCode = dto.ZipCode;

        await _db.SaveChangesAsync();

        return Ok(MapToResponseDto(location));
    }

    // DELETE /api/locations/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var location = await _db.Locations.FindAsync(id);

        if (location is null)
            return NotFound();

        _db.Locations.Remove(location);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    private static LocationResponseDto MapToResponseDto(Location l) => new()
    {
        LocationID = l.LocationID,
        Name       = l.Name,
        Address    = l.Address,
        ZipCode    = l.ZipCode
    };
}
