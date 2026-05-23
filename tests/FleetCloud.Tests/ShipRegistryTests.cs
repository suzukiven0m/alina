using CargoShipMonitoring.FleetCloud.ShipRegistry;

namespace CargoShipMonitoring.FleetCloud.Tests;

public class ShipRegistryTests : IDisposable
{
    private readonly string _dbPath;

    public ShipRegistryTests()
    {
        _dbPath = $"test_registry_{Guid.NewGuid()}.db";
    }

    public void Dispose()
    {
        if (File.Exists(_dbPath))
            File.Delete(_dbPath);
    }

    [Fact]
    public async Task RegisterShip_Adds_To_Registry()
    {
        var registry = new ShipRegistryService(_dbPath);
        await registry.RegisterAsync("MSC-001", "MSC Olivia", "IMO1234567");

        var ship = await registry.GetAsync("MSC-001");

        Assert.NotNull(ship);
        Assert.Equal("MSC Olivia", ship.Name);
    }

    [Fact]
    public async Task UpdateLastSeen_Updates_Timestamp()
    {
        var registry = new ShipRegistryService(_dbPath);
        await registry.RegisterAsync("MSC-001", "MSC Olivia", "IMO1234567");

        var before = DateTimeOffset.UtcNow;
        await registry.UpdateLastSeenAsync("MSC-001");
        var ship = await registry.GetAsync("MSC-001");

        Assert.NotNull(ship);
        Assert.True(ship.LastSeen >= before);
    }

    [Fact]
    public async Task Get_Returns_Null_For_Unknown_Ship()
    {
        var registry = new ShipRegistryService(_dbPath);
        var ship = await registry.GetAsync("UNKNOWN");
        Assert.Null(ship);
    }
}
