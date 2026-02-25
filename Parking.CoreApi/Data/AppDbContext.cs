using Microsoft.EntityFrameworkCore;
using Parking.CoreApi.Models;

namespace Parking.CoreApi.Data;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<ParkingPermitApplication> Applications => Set<ParkingPermitApplication>();

    public DbSet<ApplicationDocument> Documents => Set<ApplicationDocument>();

    public DbSet<OutboxEmail> OutboxEmails => Set<OutboxEmail>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ParkingPermitApplication>()
            .HasMany(a => a.Documents)
            .WithOne(d => d.Application!)
            .HasForeignKey(d => d.ApplicationId);

        modelBuilder.Entity<OutboxEmail>()
            .HasIndex(e => new { e.Status, e.CreatedAt });
    }
}