using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace CargoShipMonitoring.Shared.Telemetry;

public static class TelemetryConfig
{
    public static readonly ActivitySource Source = new("CargoShipMonitoring");
    public static readonly Meter Meter = new("CargoShipMonitoring");
}
