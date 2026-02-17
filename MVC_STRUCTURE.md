# ASP.NET Core MVC Projekt Struktur

Dette projekt følger den klassiske **Model-View-Controller (MVC)** arkitektur pattern.

## 📁 Folder Struktur

```
Slottet/
├── Controllers/          # MVC Controllers der håndterer requests og returnerer views
│   ├── HomeController.cs
│   ├── ResidentsController.cs
│   ├── MedicationsController.cs
│   └── API/             # API Controllers til RESTful endpoints
│       ├── ResidentsApiController.cs
│       └── MedicationsApiController.cs
│
├── Models/              # Data modeller (M i MVC)
│   ├── Resident.cs
│   ├── Medication.cs
│   ├── MedicationLog.cs
│   └── Staff.cs
│
├── Views/               # Razor views (V i MVC)
│   ├── Home/
│   │   ├── Index.cshtml
│   │   ├── About.cshtml
│   │   └── Error.cshtml
│   ├── Residents/
│   │   ├── Index.cshtml
│   │   ├── Details.cshtml
│   │   ├── Create.cshtml
│   │   ├── Edit.cshtml
│   │   └── Delete.cshtml
│   ├── Medications/
│   │   ├── Index.cshtml
│   │   ├── ForResident.cshtml
│   │   ├── Create.cshtml
│   │   └── LogAdministration.cshtml
│   ├── Shared/
│   │   ├── _Layout.cshtml        # Master layout
│   │   └── _ValidationScriptsPartial.cshtml
│   ├── _ViewStart.cshtml          # Global view settings
│   └── _ViewImports.cshtml        # Using statements for views
│
├── Services/            # Business logic layer
│   ├── IResidentService.cs
│   ├── ResidentService.cs
│   ├── IMedicationService.cs
│   └── MedicationService.cs
│
├── Data/                # Database context
│   └── ApplicationDbContext.cs
│
├── wwwroot/             # Static files (CSS, JS, images)
│   ├── css/
│   │   └── site.css
│   ├── js/
│   │   └── site.js
│   ├── lib/             # Client libraries (Bootstrap, jQuery, etc.)
│   └── favicon.ico
│
├── Properties/
│   └── launchSettings.json
│
├── Program.cs           # Application startup
├── appsettings.json     # Configuration
└── Slottet.csproj      # Project file
```

## 🔄 MVC Pattern Forklaring

### Model (M)
**Lokation:** `Models/`

Modeller repræsenterer data og forretningslogik. De indeholder:
- Properties (egenskaber)
- Data annotations (validering)
- Navigation properties (relationer)

**Eksempel:**
```csharp
public class Resident
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; }
    
    public DateTime DateOfBirth { get; set; }
    
    // Navigation property
    public virtual ICollection<Medication> Medications { get; set; }
}
```

### View (V)
**Lokation:** `Views/`

Views er Razor-filer (.cshtml) der kombinerer HTML og C# kode. De er ansvarlige for at vise data til brugeren.

**Eksempel:**
```cshtml
@model Resident

<h1>@Model.Name</h1>
<p>Alder: @CalculateAge(Model.DateOfBirth)</p>
```

**View konventioner:**
- `Index.cshtml` - Liste view
- `Details.cshtml` - Detalje view
- `Create.cshtml` - Oprettelses form
- `Edit.cshtml` - Redigerings form
- `Delete.cshtml` - Sletnings bekræftelse

### Controller (C)
**Lokation:** `Controllers/`

Controllers håndterer HTTP requests og returnerer responses (views eller data). De forbinder Model og View.

**Eksempel:**
```csharp
public class ResidentsController : Controller
{
    // GET: /Residents
    public async Task<IActionResult> Index()
    {
        var residents = await _service.GetAllResidentsAsync();
        return View(residents);
    }
    
    // GET: /Residents/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var resident = await _service.GetResidentByIdAsync(id);
        return View(resident);
    }
    
    // POST: /Residents/Create
    [HttpPost]
    public async Task<IActionResult> Create(Resident resident)
    {
        if (ModelState.IsValid)
        {
            await _service.CreateResidentAsync(resident);
            return RedirectToAction(nameof(Index));
        }
        return View(resident);
    }
}
```

## 🛣️ Routing

ASP.NET Core MVC bruger konvention-baseret routing:

```
/{controller}/{action}/{id?}
```

**Standard route (defineret i Program.cs):**
```csharp
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
```

**Eksempler:**
- `/` → `HomeController.Index()`
- `/Residents` → `ResidentsController.Index()`
- `/Residents/Details/5` → `ResidentsController.Details(5)`
- `/Medications/ForResident/5` → `MedicationsController.ForResident(5)`

## 📦 Dependency Injection

Services registreres i `Program.cs`:

```csharp
// Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Application services
builder.Services.AddScoped<IResidentService, ResidentService>();
builder.Services.AddScoped<IMedicationService, MedicationService>();
```

Og injiceres i controllers:

```csharp
public class ResidentsController : Controller
{
    private readonly IResidentService _service;
    
    public ResidentsController(IResidentService service)
    {
        _service = service;
    }
}
```

## 🎨 Razor Syntax

### Model Binding
```cshtml
@model Resident

<h1>@Model.Name</h1>
```

### Tag Helpers
```cshtml
<a asp-controller="Residents" asp-action="Details" asp-route-id="@Model.Id">
    Detaljer
</a>
```

### Forms
```cshtml
<form asp-action="Create" method="post">
    <input asp-for="Name" class="form-control" />
    <span asp-validation-for="Name" class="text-danger"></span>
    <button type="submit">Gem</button>
</form>
```

### Conditional Rendering
```cshtml
@if (Model.IsActive)
{
    <span class="badge bg-success">Aktiv</span>
}
else
{
    <span class="badge bg-secondary">Inaktiv</span>
}
```

### Loops
```cshtml
@foreach (var resident in Model)
{
    <div>@resident.Name</div>
}
```

## 📊 Data Flow

```
1. Browser sender HTTP request
   ↓
2. Routing dirigerer til controller action
   ↓
3. Controller kalder service
   ↓
4. Service kommunikerer med database via EF Core
   ↓
5. Data returneres til controller som model
   ↓
6. Controller vælger et view og sender model til det
   ↓
7. Razor Engine renderer view til HTML
   ↓
8. HTML sendes tilbage til browser
```

## 🔧 Forskelle fra klassisk ASP.NET MVC

### ASP.NET MVC (gammel)
- `Global.asax` for routing
- `Web.config` for configuration
- `.aspx` views
- Built-in bundling

### ASP.NET Core MVC (moderne)
- `Program.cs` for startup ✅
- `appsettings.json` for configuration ✅
- `.cshtml` Razor views ✅
- Middleware pipeline ✅
- Built-in dependency injection ✅
- Cross-platform (Windows, Linux, Mac) ✅
- Cloud-ready ✅

## 🎯 Best Practices i Dette Projekt

### 1. **Separation of Concerns**
- Controllers er tynde - kun koordinering
- Business logic i Services
- Data access i Repositories/Services

### 2. **Dependency Injection**
- Alle dependencies injiceres via constructor
- Interfaces bruges for loose coupling

### 3. **Async/Await**
- Alle database operationer er async
- Forbedrer skalerbarhed

### 4. **ViewBag vs ViewData vs Model**
- **Model** (foretrukket): Strongly typed data
- **ViewBag**: Dynamic data til layout/mindre data
- **TempData**: Data der overføres mellem requests

### 5. **Validation**
- Server-side: Data Annotations + ModelState
- Client-side: jQuery Validation (unobtrusive)

## 📝 Eksempler på Udvidelser

### 1. ViewModels
Opret `ViewModels/` folder for at kombinere data fra flere modeller:

```csharp
public class ResidentDetailsViewModel
{
    public Resident Resident { get; set; }
    public List<Medication> ActiveMedications { get; set; }
    public int TotalMedicationLogs { get; set; }
}
```

### 2. Partial Views
Genbrugelige view komponenter:

```cshtml
@* Views/Shared/_ResidentCard.cshtml *@
@model Resident

<div class="card">
    <div class="card-body">
        <h5>@Model.Name</h5>
        <p>@Model.RoomNumber</p>
    </div>
</div>

@* Brug i anden view: *@
@await Html.PartialAsync("_ResidentCard", resident)
```

### 3. Areas
For at organisere store applikationer:

```
Areas/
├── Admin/
│   ├── Controllers/
│   ├── Views/
│   └── Models/
└── Reports/
    ├── Controllers/
    ├── Views/
    └── Models/
```

### 4. Custom Tag Helpers
```csharp
public class StatusBadgeTagHelper : TagHelper
{
    public bool IsActive { get; set; }
    
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "span";
        output.Attributes.SetAttribute("class", 
            IsActive ? "badge bg-success" : "badge bg-secondary");
        output.Content.SetContent(IsActive ? "Aktiv" : "Inaktiv");
    }
}
```

## 🚀 Næste Skridt

1. **Autentifikation**: Tilføj ASP.NET Core Identity
2. **Authorization**: Roles og policies
3. **API Integration**: Udvid API controllers
4. **Client-side Framework**: React/Vue/Angular
5. **Real-time**: SignalR for live updates
6. **Testing**: Unit tests og integration tests

## 📚 Læs Mere

- [ASP.NET Core MVC Dokumentation](https://docs.microsoft.com/aspnet/core/mvc/)
- [Razor Syntax Reference](https://docs.microsoft.com/aspnet/core/mvc/views/razor)
- [Entity Framework Core](https://docs.microsoft.com/ef/core/)
