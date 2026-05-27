using CargoShipMonitoring.FleetCloud.CommandService;
using CargoShipMonitoring.FleetCloud.EventStore;
using CargoShipMonitoring.FleetCloud.ShipRegistry;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using CommandServiceClass = CargoShipMonitoring.FleetCloud.CommandService.CommandService;

namespace CargoShipMonitoring.FleetCloud.Tests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _registryDb;
    private readonly string _commandDb;
    private readonly string _eventDb;

    public CustomWebApplicationFactory()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"fleetcloud-tests-{Guid.NewGuid()}");
        Directory.CreateDirectory(tempDir);
        _registryDb = Path.Combine(tempDir, "registry.db");
        _commandDb = Path.Combine(tempDir, "commands.db");
        _eventDb = Path.Combine(tempDir, "events.db");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IShipRegistryService>();
            services.RemoveAll<ICommandService>();
            services.RemoveAll<IEventStoreService>();

            services.AddSingleton<IShipRegistryService>(_ => new ShipRegistryService(_registryDb));
            services.AddSingleton<ICommandService>(_ => new CommandServiceClass(_commandDb));
            services.AddSingleton<IEventStoreService>(_ => new EventStoreService(_eventDb));
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        try
        {
            var dir = Path.GetDirectoryName(_registryDb);
            if (dir != null && Directory.Exists(dir))
                Directory.Delete(dir, true);
        }
        catch { }
    }
}
