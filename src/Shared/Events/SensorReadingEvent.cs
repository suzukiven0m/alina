using CargoShipMonitoring.Shared.Models;

namespace CargoShipMonitoring.Shared.Events;

public record SensorReadingEvent : ShipEvent
{
    public SensorReading Reading { get; init; } = new();

    public SensorReadingEvent()
    {
        EventType = "sensor.reading";
        Priority = Priority.Telemetry;
    }
}
