using Microsoft.EntityFrameworkCore;
using SRMCore.Models;

namespace SRMCore.Data;

public class CoreDbContext : DbContext
{
    public CoreDbContext(DbContextOptions<CoreDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<ShellyData> ShellyData => Set<ShellyData>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // User
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasOne(u => u.Customer)
            .WithMany(c => c.Users)
            .HasForeignKey(u => u.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        // ShellyData
        modelBuilder.Entity<ShellyData>()
            .HasOne(d => d.Customer)
            .WithMany(c => c.ShellyData)
            .HasForeignKey(d => d.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        base.OnModelCreating(modelBuilder);
    }
}
