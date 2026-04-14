using DotNetEnv;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

// Load .env file for local development
Env.TraversePath().Load();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();
builder.Services.AddControllers();

// Build connection string: prefer env vars (Docker/CI), fall back to appsettings.json
var dbHost = Environment.GetEnvironmentVariable("DB_HOST");
var connectionString = dbHost is not null
    ? $"Server={dbHost};Port={Environment.GetEnvironmentVariable("DB_PORT") ?? "3306"};Database={Environment.GetEnvironmentVariable("DB_NAME")};User={Environment.GetEnvironmentVariable("DB_USER")};Password={Environment.GetEnvironmentVariable("DB_PASSWORD")};"
    : builder.Configuration.GetConnectionString("DefaultConnection")
      ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 36))));

builder.Services.AddCors(options =>
{
    options.AddPolicy("BlazorClient", policy =>
    {
        policy.WithOrigins("http://localhost:5140", "https://localhost:7158", "http://localhost:5000")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Run migrations and optionally seed data on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        logger.LogInformation("Applying database migrations...");
        await db.Database.MigrateAsync();

        // Seed only in Development
        if (app.Environment.IsDevelopment())
        {
            await DataSeeder.SeedAsync(db, logger);
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Database migration/seed fejlede. API starter alligevel.");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCors("BlazorClient");

app.MapControllers();  // aktiver alle [ApiController] klasser

app.Run();
