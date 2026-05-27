using CargoShipMonitoring.Shared.Events;
using CargoShipMonitoring.Shared.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using System.Text.Json;

namespace CargoShipMonitoring.ShipEdge.PriorityEventQueue;

public class PriorityEventQueue : IPriorityEventQueue
{
    private readonly ConcurrentQueue<ShipEvent> _criticalQueue = new();
    private readonly ConcurrentQueue<ShipEvent> _operationalQueue = new();
    private readonly ConcurrentQueue<ShipEvent> _telemetryQueue = new();
    private int _count;
    private readonly string? _dbPath;
    private ConcurrentDictionary<Guid, ShipEvent> _pendingInserts = new();
    private ConcurrentDictionary<Guid, byte> _pendingDeletes = new();
    private int _dbInitialized;
    private static readonly ConcurrentDictionary<string, bool> _walInitialized = new();
    private readonly object _enqueueLock = new();

    public int Count => Interlocked.CompareExchange(ref _count, 0, 0);
    public int MaxSize { get; }

    public PriorityEventQueue(int maxSize = 10_000)
    {
        MaxSize = maxSize;
    }

    public PriorityEventQueue(int maxSize, string dbPath)
    {
        MaxSize = maxSize;
        _dbPath = dbPath;
    }

    public void Enqueue(ShipEvent evt)
    {
        lock (_enqueueLock)
        {
            // Priority-aware eviction: drop telemetry first, then operational. Never drop critical.
            while (_count >= MaxSize)
            {
                if (_telemetryQueue.TryDequeue(out _))
                {
                    _count--;
                    continue;
                }
                if (_operationalQueue.TryDequeue(out _))
                {
                    _count--;
                    continue;
                }
                break; // Only critical events left
            }

            var queue = evt.Priority switch
            {
                Priority.Critical => _criticalQueue,
                Priority.Operational => _operationalQueue,
                _ => _telemetryQueue
            };

            queue.Enqueue(evt);
            _count++;
        }

        _pendingInserts[evt.EventId] = evt;
        _pendingDeletes.TryRemove(evt.EventId, out _);
    }

    public ShipEvent? Dequeue()
    {
        lock (_enqueueLock)
        {
            if (_criticalQueue.TryDequeue(out var critical))
            {
                _count--;
                _pendingDeletes[critical.EventId] = 1;
                _pendingInserts.TryRemove(critical.EventId, out _);
                return critical;
            }
            if (_operationalQueue.TryDequeue(out var operational))
            {
                _count--;
                _pendingDeletes[operational.EventId] = 1;
                _pendingInserts.TryRemove(operational.EventId, out _);
                return operational;
            }
            if (_telemetryQueue.TryDequeue(out var telemetry))
            {
                _count--;
                _pendingDeletes[telemetry.EventId] = 1;
                _pendingInserts.TryRemove(telemetry.EventId, out _);
                return telemetry;
            }

            return null;
        }
    }

    public async Task PersistAsync()
    {
        if (_dbPath == null)
            return;

        using var db = new EventBufferDbContext(_dbPath);
        await EnsureDbInitializedAsync(db);

        // Snapshot dictionaries to avoid racing with Enqueue/Dequeue
        var inserts = Interlocked.Exchange(ref _pendingInserts, new ConcurrentDictionary<Guid, ShipEvent>());
        var deletes = Interlocked.Exchange(ref _pendingDeletes, new ConcurrentDictionary<Guid, byte>());

        // Only delete events that were dequeued since last persist
        foreach (var id in deletes.Keys)
        {
            var existing = await db.Events.FindAsync(id);
            if (existing != null)
                db.Events.Remove(existing);
        }

        // Only insert events that were enqueued since last persist
        foreach (var evt in inserts.Values)
        {
            var existing = await db.Events.FindAsync(evt.EventId);
            if (existing == null)
            {
                db.Events.Add(new PersistedEvent
                {
                    Id = evt.EventId,
                    ShipId = evt.ShipId,
                    EventType = evt.EventType,
                    Priority = evt.Priority.ToString(),
                    JsonPayload = JsonSerializer.Serialize(evt, evt.GetType()),
                    Timestamp = evt.Timestamp
                });
            }
        }

        await db.SaveChangesAsync();
    }

    public async Task RestoreAsync()
    {
        if (_dbPath == null)
            return;

        if (!File.Exists(_dbPath))
            return;

        using var db = new EventBufferDbContext(_dbPath);
        await EnsureDbInitializedAsync(db);

        var allPersisted = await db.Events.ToListAsync();
        var persisted = allPersisted.OrderBy(e => e.Timestamp).ToList();

        foreach (var p in persisted)
        {
            ShipEvent? evt = p.Priority switch
            {
                "Critical" => JsonSerializer.Deserialize<AlertTriggeredEvent>(p.JsonPayload),
                "Operational" => DeserializeOperational(p.EventType, p.JsonPayload),
                _ => JsonSerializer.Deserialize<SensorReadingEvent>(p.JsonPayload)
            };

            if (evt != null)
            {
                var queue = evt.Priority switch
                {
                    Priority.Critical => _criticalQueue,
                    Priority.Operational => _operationalQueue,
                    _ => _telemetryQueue
                };
                queue.Enqueue(evt);
                Interlocked.Increment(ref _count);
            }
        }

        // Remove persisted events after successful restore
        db.Events.RemoveRange(db.Events);
        await db.SaveChangesAsync();

        _pendingInserts.Clear();
        _pendingDeletes.Clear();
    }

    private async Task EnsureDbInitializedAsync(EventBufferDbContext db)
    {
        if (Interlocked.CompareExchange(ref _dbInitialized, 1, 0) == 0)
        {
            await db.Database.EnsureCreatedAsync();
            if (_dbPath != null)
                EnsureWalMode(_dbPath);
        }
    }

    private static void EnsureWalMode(string dbPath)
    {
        if (_walInitialized.ContainsKey(dbPath)) return;
        using var connection = new SqliteConnection($"Data Source={dbPath}");
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "PRAGMA journal_mode=WAL;";
        command.ExecuteNonQuery();
        _walInitialized[dbPath] = true;
    }

    private static ShipEvent? DeserializeOperational(string eventType, string json)
    {
        return eventType switch
        {
            "alert.triggered" => JsonSerializer.Deserialize<AlertTriggeredEvent>(json),
            "command.issued" => JsonSerializer.Deserialize<CommandIssuedEvent>(json),
            _ => null
        };
    }
}
