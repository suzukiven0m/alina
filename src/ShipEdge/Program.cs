using CargoShipMonitoring.ShipEdge;
using CargoShipMonitoring.ShipEdge.PriorityEventQueue;
using CargoShipMonitoring.ShipEdge.RulesEngine;
using CargoShipMonitoring.ShipEdge.SatelliteGateway;
using CargoShipMonitoring.ShipEdge.TelemetryCollector;
using Microsoft.Extensions.Options;
using SatelliteGatewayClass = CargoShipMonitoring.ShipEdge.SatelliteGateway.SatelliteGateway;
using CircuitBreakerClass = CargoShipMonitoring.ShipEdge.SatelliteGateway.CircuitBreaker;

var builder = Host.CreateApplicationBuilder(args);

// Configuration
builder.Services.Configure<ShipEdgeConfig>(
    builder.Configuration.GetSection(nameof(ShipEdgeConfig)));

// Core services
builder.Services.AddSingleton<IPriorityEventQueue>(sp =>
{
    var config = sp.GetRequiredService<IOptions<ShipEdgeConfig>>().Value;
    return new PriorityEventQueue(maxSize: 50_000, dbPath: config.DbPath);
});
builder.Services.AddSingleton<IRulesEngine, RulesEngine>();
builder.Services.AddSingleton<IRuleLoader, RuleLoader>();
builder.Services.AddSingleton<ISensorReader>(sp =>
{
    var config = sp.GetRequiredService<IOptions<ShipEdgeConfig>>().Value;
    return new RealisticSensorReader(config.ShipId);
});
builder.Services.AddSingleton<SatelliteGatewayClass>(sp =>
{
    var config = sp.GetRequiredService<IOptions<ShipEdgeConfig>>().Value;
    var cbConfig = config.CircuitBreaker;
    var cb = new CircuitBreakerClass(
        failureThreshold: cbConfig.FailureThreshold,
        timeout: TimeSpan.FromSeconds(cbConfig.TimeoutSeconds));
    var factory = sp.GetRequiredService<IHttpClientFactory>();
    return new SatelliteGatewayClass(cb, factory, config.CloudEndpoint, config.ShipId);
});
builder.Services.AddSingleton<ISatelliteGateway>(sp => sp.GetRequiredService<SatelliteGatewayClass>());

// HTTP client with named configuration for satellite
builder.Services.AddHttpClient("satellite", client =>
{
    client.Timeout = TimeSpan.FromSeconds(10);
});

// Hosted service
builder.Services.AddHostedService<ShipEdgeWorker>();

var host = builder.Build();
host.Run();
