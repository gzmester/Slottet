using Microsoft.AspNetCore.Mvc;
using Slottet.Services;
using Slottet.Models;

namespace Slottet.Controllers;

/// <summary>
/// ResidentsController - MVC Controller til at håndtere beboer-relaterede views
/// Dette er IKKE en API controller - den returnerer HTML Views med Razor
/// </summary>
public class ResidentsController : Controller
{
    private readonly IResidentService _residentService;
    private readonly ILogger<ResidentsController> _logger;

    public ResidentsController(IResidentService residentService, ILogger<ResidentsController> logger)
    {
        _residentService = residentService;
        _logger = logger;
    }

    // GET: /Residents
    // Viser liste over alle beboere
    public async Task<IActionResult> Index()
    {
        try
        {
            var residents = await _residentService.GetAllResidentsAsync();
            return View(residents);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading residents");
            TempData["Error"] = "Der opstod en fejl ved indlæsning af beboere";
            return View(new List<Resident>());
        }
    }

    // GET: /Residents/Details/5
    // Viser detaljer for en specifik beboer
    public async Task<IActionResult> Details(int id)
    {
        var resident = await _residentService.GetResidentByIdAsync(id);
        
        if (resident == null)
        {
            return NotFound();
        }

        return View(resident);
    }

    // GET: /Residents/Create
    // Viser formular til at oprette ny beboer
    public IActionResult Create()
    {
        return View();
    }

    // POST: /Residents/Create
    // Håndterer formular submission for ny beboer
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Resident resident)
    {
        if (ModelState.IsValid)
        {
            try
            {
                await _residentService.CreateResidentAsync(resident);
                TempData["Success"] = "Beboer oprettet succesfuldt!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating resident");
                ModelState.AddModelError("", "Der opstod en fejl ved oprettelse af beboer");
            }
        }

        return View(resident);
    }

    // GET: /Residents/Edit/5
    // Viser formular til at redigere beboer
    public async Task<IActionResult> Edit(int id)
    {
        var resident = await _residentService.GetResidentByIdAsync(id);
        
        if (resident == null)
        {
            return NotFound();
        }

        return View(resident);
    }

    // POST: /Residents/Edit/5
    // Håndterer formular submission for redigering
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Resident resident)
    {
        if (id != resident.Id)
        {
            return BadRequest();
        }

        if (ModelState.IsValid)
        {
            try
            {
                await _residentService.UpdateResidentAsync(id, resident);
                TempData["Success"] = "Beboer opdateret succesfuldt!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating resident");
                ModelState.AddModelError("", "Der opstod en fejl ved opdatering af beboer");
            }
        }

        return View(resident);
    }

    // GET: /Residents/Delete/5
    // Viser bekræftelsesside for sletning
    public async Task<IActionResult> Delete(int id)
    {
        var resident = await _residentService.GetResidentByIdAsync(id);
        
        if (resident == null)
        {
            return NotFound();
        }

        return View(resident);
    }

    // POST: /Residents/Delete/5
    // Bekræfter og udfører sletning
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            await _residentService.DeleteResidentAsync(id);
            TempData["Success"] = "Beboer slettet succesfuldt!";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting resident");
            TempData["Error"] = "Der opstod en fejl ved sletning af beboer";
            return RedirectToAction(nameof(Index));
        }
    }

    /* EKSEMPLER PÅ HVAD MAN KUNNE TILFØJE:
     * 
     * 1. Søgefunktion:
     *    public async Task<IActionResult> Search(string searchTerm)
     *    {
     *        var results = await _residentService.SearchAsync(searchTerm);
     *        return View("Index", results);
     *    }
     * 
     * 2. Filtrering:
     *    public async Task<IActionResult> ActiveOnly()
     *    {
     *        var activeResidents = await _residentService.GetActiveResidentsAsync();
     *        return View("Index", activeResidents);
     *    }
     * 
     * 3. Export til PDF/Excel:
     *    public async Task<IActionResult> ExportToPdf()
     *    {
     *        var residents = await _residentService.GetAllResidentsAsync();
     *        var pdf = _pdfService.GenerateResidentReport(residents);
     *        return File(pdf, "application/pdf", "residents.pdf");
     *    }
     * 
     * 4. Bulk operations:
     *    [HttpPost]
     *    public async Task<IActionResult> DeactivateMultiple(List<int> ids)
     *    {
     *        await _residentService.DeactivateMultipleAsync(ids);
     *        return RedirectToAction(nameof(Index));
     *    }
     * 
     * 5. AJAX partial view:
     *    public async Task<IActionResult> GetResidentCard(int id)
     *    {
     *        var resident = await _residentService.GetResidentByIdAsync(id);
     *        return PartialView("_ResidentCard", resident);
     *    }
     */
}
