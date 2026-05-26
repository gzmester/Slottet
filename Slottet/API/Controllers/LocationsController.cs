using Application.DTOs.Location;
using Application.Interfaces.Repositories;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

//[Authorize]
[ApiController]
[Route("api/[controller]")]
public class LocationsController : ControllerBase
{
    private readonly ILocationRepository _repo;

    public LocationsController(ILocationRepository repo)
    {
        _repo = repo;
    }

    // GET /api/locations
    [HttpGet]
    public async Task<ActionResult<IEnumerable<LocationResponseDto>>> GetAll()
    {
        var locations = await _repo.GetAllAsync();
        return Ok(locations.Select(MapToResponseDto));
    }

    // GET /api/locations/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<LocationResponseDto>> GetById(int id)
    {
        var location = await _repo.GetByIdAsync(id);

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

        _repo.Add(location);
        await _repo.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = location.LocationID }, MapToResponseDto(location));
    }

    // PUT /api/locations/{id}
    [HttpPut("{id}")]
    public async Task<ActionResult<LocationResponseDto>> Update(int id, LocationUpdateDto dto)
    {
        var location = await _repo.GetByIdAsync(id);

        if (location is null)
            return NotFound();

        location.Name    = dto.Name;
        location.Address = dto.Address;
        location.ZipCode = dto.ZipCode;

        await _repo.SaveChangesAsync();

        return Ok(MapToResponseDto(location));
    }

    // DELETE /api/locations/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var location = await _repo.GetByIdAsync(id);

        if (location is null)
            return NotFound();

        _repo.Remove(location);
        await _repo.SaveChangesAsync();

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
