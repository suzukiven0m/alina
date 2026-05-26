using System.Text.Json;
using CargoShipMonitoring.Shared.Events;
using Microsoft.EntityFrameworkCore;

namespace CargoShipMonitoring.FleetCloud.EventStore;

public interface IEventStoreService
{
    Task StoreAsync(ShipEvent evt);
    Task StoreBatchAsync(List<ShipEvent> events);
    Task<List<StoredEvent>> GetEventsForShipAsync(string shipId, int limit = 100);
    Task<List<StoredEvent>> GetEventsByPriorityAsync(string priority, int limit = 100);
    Task<List<StoredEvent>> GetEventsInTimeRangeAsync(DateTimeOffset from, DateTimeOffset to, int limit = 1000);
    Task<EventSummary> GetSummaryForShipAsync(string shipId);
}

public class EventStoreService : IEventStoreService
{
    private readonly string _dbPath;

    public EventStoreService(string dbPath)
    {
        _dbPath = dbPath;
        using var db = CreateContext();
        db.Database.EnsureCreated();
    }

    public async Task StoreAsync(ShipEvent evt)
    {
        using var db = CreateContext();
        db.Events.Add(new StoredEvent
        {
            Id = evt.EventId,
            ShipId = evt.ShipId,
            EventType = evt.EventType,
            Priority = evt.Priority.ToString(),
            JsonPayload = JsonSerializer.Serialize(evt, evt.GetType()),
            Timestamp = evt.Timestamp.UtcDateTime,
            ReceivedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
    }

    public async Task StoreBatchAsync(List<ShipEvent> events)
    {
        using var db = CreateContext();
        foreach (var evt in events)
        {
            db.Events.Add(new StoredEvent
            {
                Id = evt.EventId,
                ShipId = evt.ShipId,
                EventType = evt.EventType,
                Priority = evt.Priority.ToString(),
                JsonPayload = JsonSerializer.Serialize(evt, evt.GetType()),
                Timestamp = evt.Timestamp.UtcDateTime,
                ReceivedAt = DateTime.UtcNow
            });
        }
        await db.SaveChangesAsync();
    }

    public async Task<List<StoredEvent>> GetEventsForShipAsync(string shipId, int limit = 100)
    {
        using var db = CreateContext();
        return await db.Events
            .Where(e => e.ShipId == shipId)
            .OrderByDescending(e => e.Timestamp)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<List<StoredEvent>> GetEventsByPriorityAsync(string priority, int limit = 100)
    {
        using var db = CreateContext();
        return await db.Events
            .Where(e => e.Priority == priority)
            .OrderByDescending(e => e.Timestamp)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<List<StoredEvent>> GetEventsInTimeRangeAsync(DateTimeOffset from, DateTimeOffset to, int limit = 1000)
    {
        using var db = CreateContext();
        return await db.Events
            .Where(e => e.Timestamp >= from.UtcDateTime && e.Timestamp <= to.UtcDateTime)
            .OrderByDescending(e => e.Timestamp)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<EventSummary> GetSummaryForShipAsync(string shipId)
    {
        using var db = CreateContext();
        var events = await db.Events.Where(e => e.ShipId == shipId).ToListAsync();
        return new EventSummary
        {
            ShipId = shipId,
            TotalEvents = events.Count,
            CriticalEvents = events.Count(e => e.Priority == "Critical"),
            OperationalEvents = events.Count(e => e.Priority == "Operational"),
            TelemetryEvents = events.Count(e => e.Priority == "Telemetry"),
            LastEventAt = events.MaxBy(e => e.Timestamp)?.Timestamp
        };
    }

    private EventStoreDbContext CreateContext() => new(_dbPath);
}

public class EventSummary
{
    public string ShipId { get; set; } = string.Empty;
    public int TotalEvents { get; set; }
    public int CriticalEvents { get; set; }
    public int OperationalEvents { get; set; }
    public int TelemetryEvents { get; set; }
    public DateTime? LastEventAt { get; set; }
}
