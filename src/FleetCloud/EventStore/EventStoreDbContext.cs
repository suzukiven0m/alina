using CargoShipMonitoring.Shared.Events;
using Microsoft.EntityFrameworkCore;

namespace CargoShipMonitoring.FleetCloud.EventStore;

public class EventStoreDbContext : DbContext
{
    private readonly string _dbPath;

    public DbSet<StoredEvent> Events => Set<StoredEvent>();

    public EventStoreDbContext(string dbPath)
    {
        _dbPath = dbPath;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseSqlite($"Data Source={_dbPath}");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<StoredEvent>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ShipId);
            entity.HasIndex(e => e.EventType);
            entity.HasIndex(e => e.Timestamp);
            entity.HasIndex(e => e.Priority);
        });
    }
}

public class StoredEvent
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public string ShipId { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string JsonPayload { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public DateTime ReceivedAt { get; set; }
}
