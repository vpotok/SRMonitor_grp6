using Microsoft.EntityFrameworkCore;
using SRMCore.Models;

namespace SRMCore.Data;

public class CoreDbContext : DbContext
{
    public CoreDbContext(DbContextOptions<CoreDbContext> options) : base(options) {}

    public DbSet<Company> Companies => Set<Company>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Agent> Agents => Set<Agent>();
    public DbSet<IP> IPs => Set<IP>();
    public DbSet<Log> Logs => Set<Log>();
    public DbSet<Redmine> Redmines => Set<Redmine>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Primary Keys
        modelBuilder.Entity<Company>().HasKey(c => c.ComId);
        modelBuilder.Entity<User>().HasKey(u => u.UserId);
        modelBuilder.Entity<Agent>().HasKey(a => a.AuthToken);
        modelBuilder.Entity<IP>().HasKey(i => new { i.ComId, i.IpAddress });
        modelBuilder.Entity<Log>().HasKey(l => l.LogId);
        modelBuilder.Entity<Redmine>().HasKey(r => new { r.ComId, r.ApiKey });

        // Relationships
        modelBuilder.Entity<User>()
            .HasOne(u => u.Company)
            .WithMany(c => c.Users)
            .HasForeignKey(u => u.ComId);

        modelBuilder.Entity<Agent>()
            .HasOne(a => a.Company)
            .WithMany(c => c.Agents)
            .HasForeignKey(a => a.ComId);

        modelBuilder.Entity<IP>()
            .HasOne(ip => ip.Company)
            .WithMany(c => c.IPs)
            .HasForeignKey(ip => ip.ComId);

        modelBuilder.Entity<Log>()
            .HasOne(l => l.Company)
            .WithMany(c => c.Logs)
            .HasForeignKey(l => l.ComId);

        modelBuilder.Entity<Redmine>()
            .HasOne(r => r.Company)
            .WithOne(c => c.Redmine)
            .HasForeignKey<Redmine>(r => r.ComId);

        // Constraints
        modelBuilder.Entity<Agent>()
            .HasIndex(a => a.AuthToken)
            .IsUnique();

        modelBuilder.Entity<IP>()
            .HasIndex(i => new { i.ComId, i.IpAddress })
            .IsUnique();

        modelBuilder.Entity<Redmine>()
            .HasIndex(r => new { r.ComId, r.ApiKey })
            .IsUnique();

        base.OnModelCreating(modelBuilder);
    }
}
