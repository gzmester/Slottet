// ============================================================
//  EKSEMPEL: API Controller
//
//  En controller er det sted, hvor vores API tager imod requests
//  fra klienten (fx en frontend eller en app) og sender et svar tilbage.
//
//  Man kan tænke på controlleren som et slags mellemled:
//  Den står for at modtage data og sende svar, men den indeholder
//  ikke selve forretningslogikken. I stedet kalder den videre til
//  fx en DbContext eller en service, som klarer arbejdet.
//
//  Typisk flow for en request:
//
//    1. Klienten sender en HTTP request
//    2. Controlleren modtager data (ofte som en DTO)
//    3. DTO’en bliver lavet om til en entity
//    4. Data bliver gemt i databasen via DbContext
//    5. Entity bliver lavet om til en response DTO
//    6. Controlleren sender svaret tilbage til klienten
//
//  De endpoints vi laver her:
//
//    GET    /api/products
//      → Henter alle produkter
//
//    GET    /api/products/{id}
//      → Henter ét specifikt produkt
//
//    POST   /api/products
//      → Opretter et nyt produkt
//
//    PUT    /api/products/{id}
//      → Opdaterer et eksisterende produkt
//
//    DELETE /api/products/{id}
//      → Sletter et produkt
// ============================================================

// det fik chatgpt lige omformuleret bedre end jeg selv kunne :-) 

using API.DTOs;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]  //api/products
public class ProductsController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    // DbContext injiceres automatisk af ASP.NET (Dependency Injection)
    public ProductsController(ApplicationDbContext db)
    {
        _db = db;
    }

    //GET /api/products 
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductResponseDto>>> GetAll()
    {
        var products = await _db.Products.ToListAsync();

        // Map Entity så vi kan bruge vores ResponseDto der bliver altså ikke sendt den rå entity ud fra vores domain, men derimod en DTO som kun indeholder det nødvendige data
        var result = products.Select(p => new ProductResponseDto
        {
            ProductID   = p.ProductID,
            Name        = p.Name,
            Description = p.Description,
            Price       = p.Price,
            Stock       = p.Stock
        });

        return Ok(result);
    }

    //GET /api/products/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<ProductResponseDto>> GetById(int id)
    {
        var product = await _db.Products.FindAsync(id);

        if (product is null)
            return NotFound();  // 404 som så skal håndteres i frontend med en 404 side eller lignende

        return Ok(new ProductResponseDto
        {
            ProductID   = product.ProductID,
            Name        = product.Name,
            Description = product.Description,
            Price       = product.Price,
            Stock       = product.Stock
        });
    }

    //POST /api/products
    [HttpPost]
    public async Task<ActionResult<ProductResponseDto>> Create(ProductCreateDto dto)
    {
        // Map CreateDto Entity præcis som ovenfor
        var product = new Product
        {
            Name        = dto.Name,
            Description = dto.Description,
            Price       = dto.Price,
            Stock       = dto.Stock,
            CreatedAt   = DateTime.UtcNow
        };

        _db.Products.Add(product);
        await _db.SaveChangesAsync();

        // Returner 201 Created med Location header + den nye ressource
        return CreatedAtAction(nameof(GetById), new { id = product.ProductID }, new ProductResponseDto
        {
            ProductID   = product.ProductID,
            Name        = product.Name,
            Description = product.Description,
            Price       = product.Price,
            Stock       = product.Stock
        });
    }

    //PUT /api/products/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, ProductUpdateDto dto)
    {
        var product = await _db.Products.FindAsync(id);

        if (product is null)
            return NotFound();

        // Map UpdateDto Entity (opdater kun de felter der må ændres)
        product.Name        = dto.Name;
        product.Description = dto.Description;
        product.Price       = dto.Price;
        product.Stock       = dto.Stock;

        await _db.SaveChangesAsync();

        return NoContent();  // 204 – opdateret, intet at returnere
    }

    //DELETE /api/products/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var product = await _db.Products.FindAsync(id);

        if (product is null)
            return NotFound();

        _db.Products.Remove(product);
        await _db.SaveChangesAsync();

        return NoContent();  // 204
    }
}
