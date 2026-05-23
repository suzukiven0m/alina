namespace CargoShipMonitoring.ShipEdge.PriorityEventQueue;

public class PersistedEvent
{
    public Guid Id { get; set; }
    public string ShipId { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string JsonPayload { get; set; } = string.Empty;
    public DateTimeOffset Timestamp { get; set; }
}
