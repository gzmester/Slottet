using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<Location> Locations { get; set; }
    public DbSet<Resident> Residents { get; set; }
    public DbSet<Employee> Employees { get; set; }
    public DbSet<Authorization> Authorizations { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Medicin> Medicins { get; set; }
    public DbSet<PNMedicin> PNMedicins { get; set; }
    public DbSet<Status> Statuses { get; set; }
    public DbSet<PhoneNumber> PhoneNumbers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Location
        modelBuilder.Entity<Location>(entity =>
        {
            entity.ToTable("Location");
            entity.HasKey(e => e.LocationID);
            entity.Property(e => e.Name).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Address).HasMaxLength(50).IsRequired();
        });

        // Resident
        modelBuilder.Entity<Resident>(entity =>
        {
            entity.ToTable("Resident");
            entity.HasKey(e => e.ResidentID);
            entity.Property(e => e.FirstName).HasMaxLength(50).IsRequired();
            entity.Property(e => e.LastName).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Room).HasMaxLength(20);
            entity.Property(e => e.Payment).HasMaxLength(200);
            entity.Property(e => e.RiskLevel)
                  .HasConversion<string>()
                  .HasMaxLength(20);

            entity.HasOne(e => e.Location)
                  .WithMany(l => l.Residents)
                  .HasForeignKey(e => e.LocationID);
        });

        // Authorization
        modelBuilder.Entity<Authorization>(entity =>
        {
            entity.ToTable("Authorization");
            entity.HasKey(e => e.AuthorizationID);
        });

        // Role
        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("Role");
            entity.HasKey(e => e.RoleID);
            entity.Property(e => e.Name).HasMaxLength(50);
            entity.Property(e => e.ResponsibilityArea).HasMaxLength(50);
        });

        // Employee – many-to-many with Role via implicit join table
        modelBuilder.Entity<Employee>(entity =>
        {
            entity.ToTable("Employee");
            entity.HasKey(e => e.EmployeeID);
            entity.Property(e => e.FirstName).HasMaxLength(50).IsRequired();
            entity.Property(e => e.LastName).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Email).HasMaxLength(80);
            entity.Property(e => e.ShiftType)
                  .HasConversion<string>()
                  .HasMaxLength(20);

            entity.HasOne(e => e.Location)
                  .WithMany(l => l.Employees)
                  .HasForeignKey(e => e.LocationID);

            entity.HasOne(e => e.Authorization)
                  .WithMany(a => a.Employees)
                  .HasForeignKey(e => e.AuthorizationID);

            entity.HasMany(e => e.Roles)
                  .WithMany(r => r.Employees)
                  .UsingEntity(j => j.ToTable("EmployeeRole"));
        });

        // Medicin
        modelBuilder.Entity<Medicin>(entity =>
        {
            entity.ToTable("Medicin");
            entity.HasKey(e => e.MedicinID);
            entity.HasOne(e => e.Resident)
                  .WithMany(r => r.Medicins)
                  .HasForeignKey(e => e.ResidentID);
        });

        // PNMedicin
        modelBuilder.Entity<PNMedicin>(entity =>
        {
            entity.ToTable("PNMedicin");
            entity.HasKey(e => e.PNMedicinID);
            entity.Property(e => e.Type).HasMaxLength(50);
            entity.HasOne(e => e.Resident)
                  .WithMany(r => r.PNMedicins)
                  .HasForeignKey(e => e.ResidentID);
        });

        // Status
        modelBuilder.Entity<Status>(entity =>
        {
            entity.ToTable("Status");
            entity.HasKey(e => e.StatusID);
            entity.HasOne(e => e.Resident)
                  .WithMany(r => r.Statuses)
                  .HasForeignKey(e => e.ResidentID);
        });

        modelBuilder.Entity<PhoneNumber>(entity =>
        {
            entity.ToTable("PhoneNumber");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Number).HasMaxLength(8).IsRequired();
            entity.Property(e => e.AssignedTo).HasMaxLength(50);
        });

    }
}
