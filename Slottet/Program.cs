using Microsoft.EntityFrameworkCore;
using Slottet.Data;
using Slottet.Services;

var builder = WebApplication.CreateBuilder(args);

// Add MVC services to the container
builder.Services.AddControllersWithViews()
    .AddRazorRuntimeCompilation(); // Hot reload for views in development

// Database Configuration - MySQL/MariaDB
// Cloud-ready: Connection string fra configuration (miljøvariabel i produktion)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    
    var serverVersion = new MySqlServerVersion(new Version(8, 0, 32));
    options.UseMySql(connectionString, serverVersion);
});

// Register application services
builder.Services.AddScoped<IResidentService, ResidentService>();
builder.Services.AddScoped<IMedicationService, MedicationService>();

// API Controllers for external integrations
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS for API (cloud-ready)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseCors("AllowAll");

// Map MVC routes
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Map API routes
app.MapControllers();

app.Run();
