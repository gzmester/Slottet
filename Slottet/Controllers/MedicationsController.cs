using Microsoft.AspNetCore.Mvc;
using Slottet.Services;
using Slottet.Models;

namespace Slottet.Controllers;

/// <summary>
/// MedicationsController - MVC Controller til medicinering
/// Håndterer views for medicin administration og logging
/// </summary>
public class MedicationsController : Controller
{
    private readonly IMedicationService _medicationService;
    private readonly IResidentService _residentService;
    private readonly ILogger<MedicationsController> _logger;

    public MedicationsController(
        IMedicationService medicationService,
        IResidentService residentService,
        ILogger<MedicationsController> logger)
    {
        _medicationService = medicationService;
        _residentService = residentService;
        _logger = logger;
    }

    // GET: /Medications
    // Oversigt over al medicin (evt. filtreret)
    public IActionResult Index()
    {
        // I praksis ville man hente medicin for alle beboere eller filtrere
        // Dette er bare et eksempel view
        return View();
    }

    // GET: /Medications/ForResident/5
    // Viser medicin for en specifik beboer
    public async Task<IActionResult> ForResident(int id)
    {
        var resident = await _residentService.GetResidentByIdAsync(id);
        if (resident == null)
        {
            return NotFound();
        }

        var medications = await _medicationService.GetMedicationsByResidentIdAsync(id);
        
        ViewBag.ResidentName = resident.Name;
        ViewBag.ResidentId = id;
        
        return View(medications);
    }

    // GET: /Medications/Create?residentId=5
    // Formular til at oprette ny medicin
    public async Task<IActionResult> Create(int residentId)
    {
        var resident = await _residentService.GetResidentByIdAsync(residentId);
        if (resident == null)
        {
            return NotFound();
        }

        var medication = new Medication
        {
            ResidentId = residentId,
            StartDate = DateTime.Now
        };

        ViewBag.ResidentName = resident.Name;
        return View(medication);
    }

    // POST: /Medications/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Medication medication)
    {
        if (ModelState.IsValid)
        {
            try
            {
                await _medicationService.CreateMedicationAsync(medication);
                TempData["Success"] = "Medicin tilføjet succesfuldt!";
                return RedirectToAction(nameof(ForResident), new { id = medication.ResidentId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating medication");
                ModelState.AddModelError("", "Der opstod en fejl");
            }
        }

        var resident = await _residentService.GetResidentByIdAsync(medication.ResidentId);
        ViewBag.ResidentName = resident?.Name;
        return View(medication);
    }

    // GET: /Medications/LogAdministration/5
    // Formular til at logge at medicin er givet
    public async Task<IActionResult> LogAdministration(int id)
    {
        var medication = await _medicationService.GetMedicationByIdAsync(id);
        if (medication == null)
        {
            return NotFound();
        }

        var log = new MedicationLog
        {
            MedicationId = id,
            AdministeredAt = DateTime.Now
        };

        ViewBag.MedicationName = medication.Name;
        ViewBag.ResidentName = medication.Resident.Name;
        
        return View(log);
    }

    // POST: /Medications/LogAdministration
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LogAdministration(MedicationLog log)
    {
        if (ModelState.IsValid)
        {
            try
            {
                // Her skal du sætte AdministeredById til den aktuelle bruger
                // log.AdministeredById = GetCurrentUserId();
                
                await _medicationService.LogMedicationAdministrationAsync(log);
                TempData["Success"] = "Medicinudlevering logget!";
                
                var medication = await _medicationService.GetMedicationByIdAsync(log.MedicationId);
                return RedirectToAction(nameof(ForResident), new { id = medication!.ResidentId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging medication");
                ModelState.AddModelError("", "Der opstod en fejl");
            }
        }

        return View(log);
    }

    /* EKSEMPLER PÅ HVAD MAN KUNNE TILFØJE:
     * 
     * 1. Medicin påmindelser - vis hvad der skal gives i dag:
     *    public async Task<IActionResult> DueToday()
     *    {
     *        var dueMedications = await _medicationService.GetDueTodayAsync();
     *        return View(dueMedications);
     *    }
     * 
     * 2. Medicin historik med dato filter:
     *    public async Task<IActionResult> History(int id, DateTime? fromDate, DateTime? toDate)
     *    {
     *        var history = await _medicationService.GetHistoryAsync(id, fromDate, toDate);
     *        return View(history);
     *    }
     * 
     * 3. Månedlig rapport:
     *    public async Task<IActionResult> MonthlyReport(int residentId, int month, int year)
     *    {
     *        var report = await _medicationService.GenerateMonthlyReportAsync(residentId, month, year);
     *        return View(report);
     *    }
     * 
     * 4. FMK integration (Fælles Medicinkort):
     *    public async Task<IActionResult> SyncWithFMK(int residentId)
     *    {
     *        await _fmkService.SyncMedicationsAsync(residentId);
     *        TempData["Success"] = "Synkroniseret med Fælles Medicinkort";
     *        return RedirectToAction(nameof(ForResident), new { id = residentId });
     *    }
     * 
     * 5. Batch administration - giv medicin til flere beboere på én gang:
     *    [HttpGet]
     *    public async Task<IActionResult> BatchAdminister()
     *    {
     *        var currentDue = await _medicationService.GetCurrentDueAsync();
     *        return View(currentDue);
     *    }
     *    
     *    [HttpPost]
     *    public async Task<IActionResult> BatchAdminister(List<int> medicationIds, int staffId)
     *    {
     *        await _medicationService.BatchLogAsync(medicationIds, staffId);
     *        return RedirectToAction(nameof(Index));
     *    }
     *
     * 6. Medicin konflikter og allergier check:
     *    public async Task<IActionResult> CheckInteractions(int residentId)
     *    {
     *        var interactions = await _medicationService.CheckDrugInteractionsAsync(residentId);
     *        return View(interactions);
     *    }
     */
}
