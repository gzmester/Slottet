using Microsoft.EntityFrameworkCore;
using Slottet.Models;

namespace Slottet.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // DbSets - tabeller i databasen
    public DbSet<Resident> Residents { get; set; }
    public DbSet<Medication> Medications { get; set; }
    public DbSet<MedicationLog> MedicationLogs { get; set; }
    public DbSet<Staff> Staff { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Konfigurer relationer
        modelBuilder.Entity<Resident>()
            .HasMany(r => r.Medications)
            .WithOne(m => m.Resident)
            .HasForeignKey(m => m.ResidentId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<MedicationLog>()
            .HasOne(ml => ml.Medication)
            .WithMany(m => m.Logs)
            .HasForeignKey(ml => ml.MedicationId);

        modelBuilder.Entity<MedicationLog>()
            .HasOne(ml => ml.AdministeredBy)
            .WithMany()
            .HasForeignKey(ml => ml.AdministeredById);

        // Seed data (eksempel)
        modelBuilder.Entity<Staff>().HasData(
            new Staff 
            { 
                Id = 1, 
                Name = "Admin User", 
                Email = "admin@slottet.dk",
                Role = "Administrator",
                CreatedAt = DateTime.UtcNow
            }
        );
    }
}
