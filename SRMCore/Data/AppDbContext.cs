using Microsoft.EntityFrameworkCore;
using SRMCore.Models;

namespace SRMCore.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Device> Devices { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Seed initial data for Users
        modelBuilder.Entity<User>().HasData(
            new User { Id = 1, Username = "admin", PasswordHash = "admin123", Role = "admin" },
            new User { Id = 2, Username = "user", PasswordHash = "user123", Role = "user" }
        );

        // Seed initial data for Devices
        modelBuilder.Entity<Device>().HasData(
            new Device { Id = 1, Name = "Device1", Type = "Sensor", IsActive = true }
        );
    }
}