using CargoShipMonitoring.Shared.Events;
using CargoShipMonitoring.Shared.Models;
using PriorityQueueNs = CargoShipMonitoring.ShipEdge.PriorityEventQueue;

namespace CargoShipMonitoring.ShipEdge.Tests;

public class EventPersistenceTests : IDisposable
{
    private readonly string _dbPath;

    public EventPersistenceTests()
    {
        _dbPath = Path.Combine(Path.GetTempPath(), $"test_events_{Guid.NewGuid()}.db");
    }

    public void Dispose()
    {
        if (File.Exists(_dbPath))
        {
            File.Delete(_dbPath);
        }
    }

    [Fact]
    public async Task PersistAsync_Saves_Events_To_Database()
    {
        var queue = new PriorityQueueNs.PriorityEventQueue(maxSize: 100, dbPath: _dbPath);
        var evt = new SensorReadingEvent { ShipId = "MSC-001" };
        queue.Enqueue(evt);

        await queue.PersistAsync();

        Assert.True(File.Exists(_dbPath));
        using var db = new PriorityQueueNs.EventBufferDbContext(_dbPath);
        var count = db.Events.Count();
        Assert.Equal(1, count);
    }

    [Fact]
    public async Task RestoreAsync_Loads_Events_From_Database()
    {
        var queue1 = new PriorityQueueNs.PriorityEventQueue(maxSize: 100, dbPath: _dbPath);
        var evt = new SensorReadingEvent { ShipId = "MSC-001" };
        queue1.Enqueue(evt);
        await queue1.PersistAsync();

        var queue2 = new PriorityQueueNs.PriorityEventQueue(maxSize: 100, dbPath: _dbPath);
        await queue2.RestoreAsync();

        Assert.Equal(1, queue2.Count);
        var restored = queue2.Dequeue();
        Assert.NotNull(restored);
        Assert.Equal("MSC-001", restored.ShipId);
    }

    [Fact]
    public async Task RestoreAsync_Removes_Events_From_Database()
    {
        var queue1 = new PriorityQueueNs.PriorityEventQueue(maxSize: 100, dbPath: _dbPath);
        queue1.Enqueue(new SensorReadingEvent { ShipId = "MSC-001" });
        await queue1.PersistAsync();

        var queue2 = new PriorityQueueNs.PriorityEventQueue(maxSize: 100, dbPath: _dbPath);
        await queue2.RestoreAsync();

        using var db = new PriorityQueueNs.EventBufferDbContext(_dbPath);
        Assert.Equal(0, db.Events.Count());
    }

    [Fact]
    public async Task RestoreAsync_Preserves_Priority_Ordering()
    {
        var queue1 = new PriorityQueueNs.PriorityEventQueue(maxSize: 100, dbPath: _dbPath);
        queue1.Enqueue(new SensorReadingEvent { ShipId = "T1", Priority = Priority.Telemetry });
        queue1.Enqueue(new AlertTriggeredEvent { ShipId = "O1", Priority = Priority.Operational });
        queue1.Enqueue(new AlertTriggeredEvent { ShipId = "C1", Priority = Priority.Critical });
        await queue1.PersistAsync();

        var queue2 = new PriorityQueueNs.PriorityEventQueue(maxSize: 100, dbPath: _dbPath);
        await queue2.RestoreAsync();

        var first = queue2.Dequeue();
        var second = queue2.Dequeue();
        var third = queue2.Dequeue();

        Assert.Equal(Priority.Critical, first!.Priority);
        Assert.Equal(Priority.Operational, second!.Priority);
        Assert.Equal(Priority.Telemetry, third!.Priority);
    }

    [Fact]
    public async Task Dequeue_Removes_Event_From_Persistence_After_Persist()
    {
        var queue = new PriorityQueueNs.PriorityEventQueue(maxSize: 100, dbPath: _dbPath);
        queue.Enqueue(new SensorReadingEvent { ShipId = "MSC-001" });
        await queue.PersistAsync();

        // Simulate dequeue then re-persist
        var dequeued = queue.Dequeue();
        Assert.NotNull(dequeued);
        await queue.PersistAsync();

        using var db = new PriorityQueueNs.EventBufferDbContext(_dbPath);
        Assert.Equal(0, db.Events.Count());
    }

    [Fact]
    public async Task PersistAsync_Without_DbPath_Does_Nothing()
    {
        var queue = new PriorityQueueNs.PriorityEventQueue(maxSize: 100);
        queue.Enqueue(new SensorReadingEvent { ShipId = "MSC-001" });

        var exception = await Record.ExceptionAsync(async () => await queue.PersistAsync());
        Assert.Null(exception);
    }

    [Fact]
    public async Task RestoreAsync_Without_DbPath_Does_Nothing()
    {
        var queue = new PriorityQueueNs.PriorityEventQueue(maxSize: 100);

        var exception = await Record.ExceptionAsync(async () => await queue.RestoreAsync());
        Assert.Null(exception);
    }

    [Fact]
    public async Task RestoreAsync_With_Missing_File_Does_Nothing()
    {
        var nonExistentPath = Path.Combine(Path.GetTempPath(), $"nonexistent_{Guid.NewGuid()}.db");
        var queue = new PriorityQueueNs.PriorityEventQueue(maxSize: 100, dbPath: nonExistentPath);

        var exception = await Record.ExceptionAsync(async () => await queue.RestoreAsync());
        Assert.Null(exception);
        Assert.Equal(0, queue.Count);
    }
}
