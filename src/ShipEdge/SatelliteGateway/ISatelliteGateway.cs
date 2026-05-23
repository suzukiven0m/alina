using CargoShipMonitoring.Shared.Events;

namespace CargoShipMonitoring.ShipEdge.SatelliteGateway;

public interface ISatelliteGateway
{
    bool CanTransmit();
    Task<bool> TransmitAsync(ShipEvent evt);
    Task<bool> TransmitBatchAsync(List<ShipEvent> events);
}
