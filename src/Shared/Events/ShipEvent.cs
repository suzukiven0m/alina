using CargoShipMonitoring.Shared.Models;
using System.Text.Json.Serialization;

namespace CargoShipMonitoring.Shared.Events;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$eventType")]
[JsonDerivedType(typeof(SensorReadingEvent), "sensor.reading")]
[JsonDerivedType(typeof(AlertTriggeredEvent), "alert.triggered")]
[JsonDerivedType(typeof(CommandIssuedEvent), "command.issued")]
public abstract record ShipEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public string ShipId { get; init; } = string.Empty;
    public string EventType { get; init; } = string.Empty;
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;
    public Priority Priority { get; init; }
}
