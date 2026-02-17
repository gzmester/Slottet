using Microsoft.AspNetCore.Mvc;
using Slottet.Services;
using Slottet.Models;

namespace Slottet.Controllers;

/// <summary>
/// HomeController - Håndterer forsiden og generelle sider
/// MVC Pattern: Controller modtager requests og returnerer Views
/// </summary>
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    // GET: /Home/Index eller /
    public IActionResult Index()
    {
        // Eksempel: Du kan sende data til view via ViewBag
        ViewBag.WelcomeMessage = "Velkommen til Slottet Plejehjemssystem";
        ViewBag.SystemVersion = "1.0.0";
        
        return View();
    }

    // GET: /Home/About
    public IActionResult About()
    {
        return View();
    }

    // GET: /Home/Error
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View();
    }

    /* EKSEMPLER PÅ HVAD MAN KUNNE GØRE:
     * 
     * 1. Vise dashboard statistik:
     *    public IActionResult Dashboard()
     *    {
     *        var stats = new DashboardViewModel {
     *            TotalResidents = _residentService.GetCount(),
     *            ActiveMedications = _medicationService.GetActiveCount()
     *        };
     *        return View(stats);
     *    }
     * 
     * 2. Håndtere forms med POST:
     *    [HttpPost]
     *    public IActionResult Contact(ContactForm form)
     *    {
     *        if (ModelState.IsValid)
     *        {
     *            // Send email, gem i database, etc.
     *            return RedirectToAction("Index");
     *        }
     *        return View(form);
     *    }
     * 
     * 3. Partial views til dynamisk indhold:
     *    public IActionResult GetNotifications()
     *    {
     *        var notifications = _notificationService.GetRecent();
     *        return PartialView("_Notifications", notifications);
     *    }
     */
}
