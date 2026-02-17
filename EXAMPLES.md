# Eksempler på Udvidelses Muligheder

Dette dokument indeholder konkrete eksempler på funktionalitet man kunne tilføje til Slottet systemet.

## 🎯 Views & Controllers

### 1. Dashboard med Statistik

**HomeController.cs:**
```csharp
public class DashboardViewModel
{
    public int TotalResidents { get; set; }
    public int ActiveMedications { get; set; }
    public int MedicationsDueToday { get; set; }
    public List<RecentActivity> RecentActivities { get; set; }
}

public async Task<IActionResult> Dashboard()
{
    var viewModel = new DashboardViewModel
    {
        TotalResidents = await _residentService.GetCountAsync(),
        ActiveMedications = await _medicationService.GetActiveCountAsync(),
        MedicationsDueToday = await _medicationService.GetDueTodayCountAsync(),
        RecentActivities = await _activityService.GetRecentAsync(10)
    };
    
    return View(viewModel);
}
```

**Dashboard.cshtml:**
```cshtml
@model DashboardViewModel

<div class="row">
    <div class="col-md-3">
        <div class="card bg-primary text-white">
            <div class="card-body">
                <h2>@Model.TotalResidents</h2>
                <p>Total Beboere</p>
            </div>
        </div>
    </div>
    <div class="col-md-3">
        <div class="card bg-success text-white">
            <div class="card-body">
                <h2>@Model.ActiveMedications</h2>
                <p>Aktive Medicin</p>
            </div>
        </div>
    </div>
</div>
```

### 2. Søgefunktion

**ResidentsController.cs:**
```csharp
public async Task<IActionResult> Search(string searchTerm)
{
    if (string.IsNullOrWhiteSpace(searchTerm))
    {
        return RedirectToAction(nameof(Index));
    }
    
    var results = await _residentService.SearchAsync(searchTerm);
    ViewBag.SearchTerm = searchTerm;
    return View("Index", results);
}
```

**Index.cshtml (tilføj søgeboks):**
```cshtml
<form asp-action="Search" method="get" class="mb-3">
    <div class="input-group">
        <input type="text" name="searchTerm" class="form-control" 
               placeholder="Søg efter navn, CPR eller værelse..." 
               value="@ViewBag.SearchTerm" />
        <button type="submit" class="btn btn-primary">🔍 Søg</button>
    </div>
</form>
```

### 3. Export til PDF/Excel

**ResidentsController.cs:**
```csharp
public async Task<IActionResult> ExportToPdf()
{
    var residents = await _residentService.GetAllResidentsAsync();
    var pdf = await _pdfService.GenerateResidentReportAsync(residents);
    
    return File(pdf, "application/pdf", $"beboere_{DateTime.Now:yyyyMMdd}.pdf");
}

public async Task<IActionResult> ExportToExcel()
{
    var residents = await _residentService.GetAllResidentsAsync();
    var excel = await _excelService.GenerateResidentReportAsync(residents);
    
    return File(excel, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
                $"beboere_{DateTime.Now:yyyyMMdd}.xlsx");
}
```

### 4. Filtrering og Sortering

**ResidentsController.cs:**
```csharp
public async Task<IActionResult> Index(string sortOrder, string statusFilter)
{
    ViewBag.NameSortParam = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
    ViewBag.DateSortParam = sortOrder == "date" ? "date_desc" : "date";
    ViewBag.CurrentFilter = statusFilter;
    
    var residents = await _residentService.GetAllResidentsAsync();
    
    // Filter
    if (!string.IsNullOrEmpty(statusFilter))
    {
        residents = statusFilter.ToLower() switch
        {
            "active" => residents.Where(r => r.IsActive),
            "inactive" => residents.Where(r => !r.IsActive),
            _ => residents
        };
    }
    
    // Sort
    residents = sortOrder switch
    {
        "name_desc" => residents.OrderByDescending(r => r.Name),
        "date" => residents.OrderBy(r => r.AdmissionDate),
        "date_desc" => residents.OrderByDescending(r => r.AdmissionDate),
        _ => residents.OrderBy(r => r.Name)
    };
    
    return View(residents.ToList());
}
```

## 💊 Medicin Features

### 1. Medicin Påmindelser

**MedicationsController.cs:**
```csharp
public async Task<IActionResult> DueToday()
{
    var dueMedications = await _medicationService.GetDueTodayAsync();
    return View(dueMedications);
}

public async Task<IActionResult> Upcoming(int hours = 4)
{
    var upcoming = await _medicationService.GetDueInNextHoursAsync(hours);
    return View(upcoming);
}
```

### 2. Medicin Historik med Dato Filter

**MedicationsController.cs:**
```csharp
public async Task<IActionResult> History(int medicationId, DateTime? fromDate, DateTime? toDate)
{
    fromDate ??= DateTime.Now.AddMonths(-1);
    toDate ??= DateTime.Now;
    
    var logs = await _medicationService.GetLogsInDateRangeAsync(
        medicationId, fromDate.Value, toDate.Value);
    
    ViewBag.MedicationName = (await _medicationService.GetMedicationByIdAsync(medicationId))?.Name;
    ViewBag.FromDate = fromDate;
    ViewBag.ToDate = toDate;
    
    return View(logs);
}
```

**History.cshtml:**
```cshtml
@model IEnumerable<MedicationLog>

<h1>Medicin Historik: @ViewBag.MedicationName</h1>

<form asp-action="History" method="get" class="mb-3">
    <input type="hidden" name="medicationId" value="@ViewBag.MedicationId" />
    <div class="row">
        <div class="col-md-4">
            <label>Fra dato:</label>
            <input type="date" name="fromDate" value="@ViewBag.FromDate?.ToString("yyyy-MM-dd")" class="form-control" />
        </div>
        <div class="col-md-4">
            <label>Til dato:</label>
            <input type="date" name="toDate" value="@ViewBag.ToDate?.ToString("yyyy-MM-dd")" class="form-control" />
        </div>
        <div class="col-md-4">
            <label>&nbsp;</label>
            <button type="submit" class="btn btn-primary d-block">Filtrer</button>
        </div>
    </div>
</form>

<table class="table">
    <thead>
        <tr>
            <th>Dato & Tid</th>
            <th>Givet af</th>
            <th>Status</th>
            <th>Noter</th>
        </tr>
    </thead>
    <tbody>
        @foreach (var log in Model)
        {
            <tr>
                <td>@log.AdministeredAt.ToString("dd/MM/yyyy HH:mm")</td>
                <td>@log.AdministeredBy.Name</td>
                <td>
                    @if (log.WasSkipped)
                    {
                        <span class="badge bg-warning">Sprunget over</span>
                    }
                    else
                    {
                        <span class="badge bg-success">Givet</span>
                    }
                </td>
                <td>@log.Notes @log.SkipReason</td>
            </tr>
        }
    </tbody>
</table>
```

### 3. Batch Administration

**MedicationsController.cs:**
```csharp
public class BatchAdministrationViewModel
{
    public List<MedicationDueItem> DueMedications { get; set; }
    public int StaffId { get; set; }
}

public class MedicationDueItem
{
    public int MedicationId { get; set; }
    public string MedicationName { get; set; }
    public string ResidentName { get; set; }
    public string Dosage { get; set; }
    public bool Selected { get; set; }
}

[HttpGet]
public async Task<IActionResult> BatchAdminister()
{
    var dueMedications = await _medicationService.GetCurrentDueAsync();
    
    var viewModel = new BatchAdministrationViewModel
    {
        DueMedications = dueMedications.Select(m => new MedicationDueItem
        {
            MedicationId = m.Id,
            MedicationName = m.Name,
            ResidentName = m.Resident.Name,
            Dosage = m.Dosage,
            Selected = false
        }).ToList()
    };
    
    return View(viewModel);
}

[HttpPost]
public async Task<IActionResult> BatchAdminister(List<int> selectedIds, int staffId)
{
    if (selectedIds == null || !selectedIds.Any())
    {
        TempData["Error"] = "Vælg mindst én medicin";
        return RedirectToAction(nameof(BatchAdminister));
    }
    
    await _medicationService.BatchLogAsync(selectedIds, staffId);
    TempData["Success"] = $"{selectedIds.Count} medicin logget succesfuldt!";
    
    return RedirectToAction(nameof(Index));
}
```

### 4. Drug Interaction Check

**MedicationsController.cs:**
```csharp
public async Task<IActionResult> CheckInteractions(int residentId)
{
    var interactions = await _medicationService.CheckDrugInteractionsAsync(residentId);
    
    if (interactions.Any())
    {
        ViewBag.HasInteractions = true;
        ViewBag.WarningMessage = "Der er fundet potentielle interaktioner!";
    }
    
    return View(interactions);
}
```

**Services/IMedicationService.cs:**
```csharp
public interface IMedicationService
{
    // ... existing methods
    
    Task<List<DrugInteraction>> CheckDrugInteractionsAsync(int residentId);
}
```

## 📊 Rapporter

### 1. Månedlig Medicin Rapport

**ReportsController.cs:**
```csharp
public class ReportsController : Controller
{
    private readonly IMedicationService _medicationService;
    private readonly IResidentService _residentService;
    
    public async Task<IActionResult> MonthlyMedication(int? month, int? year)
    {
        month ??= DateTime.Now.Month;
        year ??= DateTime.Now.Year;
        
        var report = await _medicationService.GenerateMonthlyReportAsync(month.Value, year.Value);
        
        ViewBag.Month = month;
        ViewBag.Year = year;
        
        return View(report);
    }
    
    public async Task<IActionResult> ResidentSummary(int residentId)
    {
        var resident = await _residentService.GetResidentByIdAsync(residentId);
        var medications = await _medicationService.GetMedicationsByResidentIdAsync(residentId);
        var logs = await _medicationService.GetAllLogsForResidentAsync(residentId);
        
        var viewModel = new ResidentSummaryViewModel
        {
            Resident = resident,
            ActiveMedications = medications.Where(m => m.IsActive).ToList(),
            InactiveMedications = medications.Where(m => !m.IsActive).ToList(),
            RecentLogs = logs.OrderByDescending(l => l.AdministeredAt).Take(50).ToList(),
            ComplianceRate = CalculateComplianceRate(logs)
        };
        
        return View(viewModel);
    }
}
```

### 2. Compliance Tracking

**Models/ComplianceReport.cs:**
```csharp
public class ComplianceReport
{
    public int ResidentId { get; set; }
    public string ResidentName { get; set; }
    public int ExpectedDoses { get; set; }
    public int AdministeredDoses { get; set; }
    public int SkippedDoses { get; set; }
    public double ComplianceRate => ExpectedDoses > 0 
        ? (double)AdministeredDoses / ExpectedDoses * 100 
        : 0;
}
```

## 🔐 Autentifikation & Authorization

### 1. Login Controller

**AccountController.cs:**
```csharp
public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    
    [HttpGet]
    public IActionResult Login(string returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
    {
        if (ModelState.IsValid)
        {
            var result = await _signInManager.PasswordSignInAsync(
                model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
                
            if (result.Succeeded)
            {
                return RedirectToLocal(returnUrl);
            }
            
            ModelState.AddModelError(string.Empty, "Ugyldigt login forsøg.");
        }
        
        return View(model);
    }
}
```

### 2. Role-Based Authorization

**Controllers:**
```csharp
[Authorize(Roles = "Administrator,Nurse")]
public class MedicationsController : Controller
{
    // Only admins and nurses can access
}

[Authorize(Roles = "Administrator")]
public async Task<IActionResult> Delete(int id)
{
    // Only administrator can delete
}
```

## 🔔 Notifikationer

### 1. Email Notifikationer

**Services/IEmailService.cs:**
```csharp
public interface IEmailService
{
    Task SendMedicationReminder(Medication medication);
    Task SendMissedDoseAlert(MedicationLog log);
}

public class EmailService : IEmailService
{
    public async Task SendMedicationReminder(Medication medication)
    {
        // Send email til personale om kommende medicin
        var subject = $"Påmindelse: Medicin til {medication.Resident.Name}";
        var body = $"Medicin '{medication.Name}' skal gives til {medication.Resident.Name} " +
                   $"i værelse {medication.Resident.RoomNumber}.";
                   
        await SendEmailAsync("staff@slottet.dk", subject, body);
    }
}
```

### 2. Real-time Opdateringer med SignalR

**Hubs/MedicationHub.cs:**
```csharp
public class MedicationHub : Hub
{
    public async Task MedicationLogged(int medicationId, int residentId)
    {
        await Clients.All.SendAsync("MedicationLogged", medicationId, residentId);
    }
    
    public async Task ResidentUpdated(int residentId)
    {
        await Clients.All.SendAsync("ResidentUpdated", residentId);
    }
}
```

**Program.cs:**
```csharp
builder.Services.AddSignalR();

// ...

app.MapHub<MedicationHub>("/medicationHub");
```

**Views (JavaScript):**
```javascript
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/medicationHub")
    .build();

connection.on("MedicationLogged", function (medicationId, residentId) {
    // Reload data eller vis notification
    showNotification(`Medicin logget for beboer ${residentId}`);
    reloadMedicationData(residentId);
});

connection.start();
```

## 📱 Mobile Features

### 1. QR Code Scanning

**Views/Medications/Scan.cshtml:**
```cshtml
<div id="qr-reader" style="width: 500px"></div>

<script src="https://unpkg.com/html5-qrcode"></script>
<script>
    const html5QrCode = new Html5Qrcode("qr-reader");
    
    html5QrCode.start(
        { facingMode: "environment" },
        { fps: 10, qrbox: 250 },
        qrCodeMessage => {
            // QR Code genkendt
            $.get(`/api/medications/verify?qrCode=${qrCodeMessage}`, function(medication) {
                // Vis medicin info og bekræft
                showMedicationConfirmation(medication);
            });
        }
    );
</script>
```

### 2. Progressive Web App (PWA)

**wwwroot/manifest.json:**
```json
{
  "name": "Slottet Plejehjemssystem",
  "short_name": "Slottet",
  "start_url": "/",
  "display": "standalone",
  "background_color": "#ffffff",
  "theme_color": "#0d6efd",
  "icons": [
    {
      "src": "/icons/icon-192.png",
      "sizes": "192x192",
      "type": "image/png"
    }
  ]
}
```

Dette er kun nogle få eksempler - mulighederne er uendelige! 🚀
