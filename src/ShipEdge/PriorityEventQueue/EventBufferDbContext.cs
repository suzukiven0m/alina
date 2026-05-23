using Microsoft.EntityFrameworkCore;

namespace CargoShipMonitoring.ShipEdge.PriorityEventQueue;

public class EventBufferDbContext : DbContext
{
    private readonly string _dbPath;

    public DbSet<PersistedEvent> Events => Set<PersistedEvent>();

    public EventBufferDbContext(string dbPath)
    {
        _dbPath = dbPath;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseSqlite($"Data Source={_dbPath}");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PersistedEvent>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ShipId).IsRequired();
            entity.Property(e => e.EventType).IsRequired();
            entity.Property(e => e.Priority).IsRequired();
            entity.Property(e => e.JsonPayload).IsRequired();
            entity.Property(e => e.Timestamp).IsRequired();
        });
    }
}
