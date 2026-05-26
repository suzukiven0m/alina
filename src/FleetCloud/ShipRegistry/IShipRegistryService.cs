namespace CargoShipMonitoring.FleetCloud.ShipRegistry;

public interface IShipRegistryService
{
    Task RegisterAsync(string shipId, string name, string? imoNumber = null);
    Task<ShipInfo?> GetAsync(string shipId);
    Task UpdateLastSeenAsync(string shipId);
    Task UpdateStatusAsync(string shipId, string status);
    Task UpdatePositionAsync(string shipId, double latitude, double longitude, double? speed = null);
    Task<List<ShipInfo>> GetAllAsync();
}
