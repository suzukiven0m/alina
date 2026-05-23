namespace CargoShipMonitoring.Shared.Models;

public record SensorReading
{
    public string SensorId { get; init; } = string.Empty;
    public SensorType SensorType { get; init; }
    public double Value { get; init; }
    public string Unit { get; init; } = string.Empty;
    public string? Location { get; init; }
    public DateTimeOffset Timestamp { get; init; }
}
