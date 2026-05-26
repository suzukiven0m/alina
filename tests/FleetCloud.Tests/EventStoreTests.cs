using CargoShipMonitoring.FleetCloud.EventStore;
using CargoShipMonitoring.Shared.Events;
using CargoShipMonitoring.Shared.Models;

namespace CargoShipMonitoring.FleetCloud.Tests;

public class EventStoreTests : IDisposable
{
    private readonly string _dbPath = $"test_events_{Guid.NewGuid()}.db";
    private readonly EventStoreService _service;

    public EventStoreTests()
    {
        _service = new EventStoreService(_dbPath);
    }

    [Fact]
    public async Task StoreAsync_Persists_SensorReadingEvent()
    {
        var evt = new SensorReadingEvent
        {
            ShipId = "MSC-001",
            Reading = new SensorReading
            {
                SensorId = "TEMP-01",
                SensorType = SensorType.EngineTemperature,
                Value = 120.5,
                Unit = "C"
            }
        };

        await _service.StoreAsync(evt);
        var events = await _service.GetEventsForShipAsync("MSC-001");

        Assert.Single(events);
        Assert.Equal("sensor.reading", events[0].EventType);
        Assert.Contains("120.5", events[0].JsonPayload);
    }

    [Fact]
    public async Task StoreBatchAsync_Persists_MultipleEvents()
    {
        var events = new List<ShipEvent>
        {
            new SensorReadingEvent { ShipId = "MSC-001" },
            new AlertTriggeredEvent { ShipId = "MSC-001", Priority = Priority.Critical }
        };

        await _service.StoreBatchAsync(events);
        var stored = await _service.GetEventsForShipAsync("MSC-001");

        Assert.Equal(2, stored.Count);
    }

    [Fact]
    public async Task GetSummary_Returns_AccurateCounts()
    {
        await _service.StoreAsync(new AlertTriggeredEvent
        {
            ShipId = "MSC-001",
            Priority = Priority.Critical
        });
        await _service.StoreAsync(new SensorReadingEvent
        {
            ShipId = "MSC-001",
            Priority = Priority.Telemetry
        });

        var summary = await _service.GetSummaryForShipAsync("MSC-001");

        Assert.Equal(2, summary.TotalEvents);
        Assert.Equal(1, summary.CriticalEvents);
        Assert.Equal(1, summary.TelemetryEvents);
    }

    [Fact]
    public async Task GetEventsByPriority_Returns_OnlyMatchingPriority()
    {
        await _service.StoreAsync(new AlertTriggeredEvent
        {
            ShipId = "MSC-001",
            Priority = Priority.Critical
        });
        await _service.StoreAsync(new SensorReadingEvent
        {
            ShipId = "MSC-001",
            Priority = Priority.Telemetry
        });

        var critical = await _service.GetEventsByPriorityAsync("Critical", 100);

        Assert.Single(critical);
        Assert.Equal("Critical", critical[0].Priority);
    }

    [Fact]
    public async Task GetEventsInTimeRange_Returns_EventsWithinRange()
    {
        var now = DateTimeOffset.UtcNow;
        await _service.StoreAsync(new SensorReadingEvent
        {
            ShipId = "MSC-001",
            Timestamp = now.AddMinutes(-5)
        });
        await _service.StoreAsync(new SensorReadingEvent
        {
            ShipId = "MSC-001",
            Timestamp = now.AddMinutes(-30)
        });

        var events = await _service.GetEventsInTimeRangeAsync(now.AddMinutes(-10), now);

        Assert.Single(events);
    }

    public void Dispose()
    {
        if (File.Exists(_dbPath))
            File.Delete(_dbPath);
    }
}
