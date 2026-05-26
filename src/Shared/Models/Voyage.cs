namespace CargoShipMonitoring.Shared.Models;

public record Voyage
{
    public Guid VoyageId { get; init; } = Guid.NewGuid();
    public string ShipId { get; init; } = string.Empty;
    public string DeparturePort { get; init; } = string.Empty;
    public string DestinationPort { get; init; } = string.Empty;
    public DateTimeOffset ETD { get; init; }
    public DateTimeOffset ETA { get; init; }
    public VoyageStatus Status { get; init; } = VoyageStatus.Planned;
    public List<CargoItem> CargoManifest { get; init; } = new();
    public double? CurrentLatitude { get; init; }
    public double? CurrentLongitude { get; init; }
    public double? CurrentSpeed { get; init; }
    public double? CurrentHeading { get; init; }
}

public record CargoItem
{
    public string ContainerId { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string CargoType { get; init; } = string.Empty;
    public double WeightKg { get; init; }
    public bool RequiresReefer { get; init; }
    public double? MinTemperature { get; init; }
    public double? MaxTemperature { get; init; }
}

public enum VoyageStatus
{
    Planned,
    InProgress,
    Completed,
    Cancelled,
    Distressed
}
