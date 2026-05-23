namespace CargoShipMonitoring.FleetCloud.ShipRegistry;

public class ShipInfo
{
    public string ShipId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? IMONumber { get; set; }
    public DateTimeOffset RegisteredAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset LastSeen { get; set; } = DateTimeOffset.MinValue;
    public string Status { get; set; } = "Offline";
    public string? LastKnownPosition { get; set; }
}
