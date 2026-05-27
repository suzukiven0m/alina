using Microsoft.EntityFrameworkCore;

namespace CargoShipMonitoring.FleetCloud.CommandService;

public class CommandDbContext : DbContext
{
    private readonly string _dbPath;

    public DbSet<PendingCommand> Commands => Set<PendingCommand>();

    public CommandDbContext(string dbPath)
    {
        _dbPath = dbPath;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseSqlite($"Data Source={_dbPath}");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PendingCommand>(entity =>
        {
            entity.HasKey(e => e.CommandId);
            entity.HasIndex(e => e.ShipId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.NextRetryAt);
            entity.Property(e => e.CreatedAt).HasConversion(
                v => v.UtcDateTime,
                v => new DateTimeOffset(v, TimeSpan.Zero));
            entity.Property(e => e.LastAttemptAt).HasConversion(
                v => v.HasValue ? v.Value.UtcDateTime : (DateTime?)null,
                v => v.HasValue ? new DateTimeOffset(v.Value, TimeSpan.Zero) : (DateTimeOffset?)null);
            entity.Property(e => e.NextRetryAt).HasConversion(
                v => v.HasValue ? v.Value.UtcDateTime : (DateTime?)null,
                v => v.HasValue ? new DateTimeOffset(v.Value, TimeSpan.Zero) : (DateTimeOffset?)null);
        });
    }
}
