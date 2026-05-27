using CargoShipMonitoring.FleetCloud.EventStore;
using CargoShipMonitoring.FleetCloud.ShipRegistry;
using CargoShipMonitoring.FleetCloud.CommandService;
using CargoShipMonitoring.Shared.Events;
using CargoShipMonitoring.Shared.Models;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Text.Json;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Register services
builder.Services.AddSingleton<IShipRegistryService>(_ => new ShipRegistryService("fleet_registry.db"));
builder.Services.AddSingleton<ICommandService>(_ => new CommandService("fleet_commands.db"));
builder.Services.AddSingleton<IEventStoreService>(_ => new EventStoreService("fleet_events.db"));

// Rate limiting
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("events", limiter =>
    {
        limiter.PermitLimit = 120;
        limiter.Window = TimeSpan.FromMinutes(1);
        limiter.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiter.QueueLimit = 10;
    });
    options.AddFixedWindowLimiter("commands", limiter =>
    {
        limiter.PermitLimit = 60;
        limiter.Window = TimeSpan.FromMinutes(1);
        limiter.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiter.QueueLimit = 5;
    });
    options.AddFixedWindowLimiter("fleet", limiter =>
    {
        limiter.PermitLimit = 30;
        limiter.Window = TimeSpan.FromMinutes(1);
        limiter.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiter.QueueLimit = 5;
    });
});

// Telemetry
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing =>
    {
        tracing
            .AddSource("CargoShipMonitoring")
            .SetResourceBuilder(ResourceBuilder.CreateDefault()
                .AddService("FleetCloud", serviceVersion: "1.0.0"))
            .AddAspNetCoreInstrumentation()
            .AddConsoleExporter();
    });

// Health checks
builder.Services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>("database")
    .AddCheck<EventStoreHealthCheck>("eventstore");

builder.Services.AddCors(options =>
{
    options.AddPolicy("LandingPage", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "https://suzukiven0m.github.io")
              .WithHeaders("Content-Type", "Authorization")
              .WithMethods("GET", "POST");
    });
});

builder.Services.AddHostedService<CommandRetryWorker>();

var app = builder.Build();

app.MapHealthChecks("/health");
app.UseCors("LandingPage");
app.UseRateLimiter();

var registry = app.Services.GetRequiredService<IShipRegistryService>();
var commandService = app.Services.GetRequiredService<ICommandService>();
var eventStore = app.Services.GetRequiredService<IEventStoreService>();
var logger = app.Services.GetRequiredService<ILogger<Program>>();

// Receive events from ship
app.MapPost("/api/events/{shipId}", async (string shipId, HttpContext context) =>
{
    if (string.IsNullOrWhiteSpace(shipId))
        return Results.BadRequest("shipId is required");

    using var reader = new StreamReader(context.Request.Body);
    var body = await reader.ReadToEndAsync();
    if (body.Length > 1024 * 1024) // 1MB limit
        return Results.BadRequest("Event payload exceeds 1MB limit");
    if (string.IsNullOrWhiteSpace(body))
        return Results.BadRequest("Event body is required");

    ShipEvent? evt;
    try
    {
        evt = JsonSerializer.Deserialize<ShipEvent>(body, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "Failed to deserialize event from {ShipId}", shipId);
        return Results.BadRequest("Invalid JSON or unknown event type");
    }

    if (evt == null)
        return Results.BadRequest("Event could not be deserialized");

    await registry.UpdateLastSeenAsync(shipId);
    await eventStore.StoreAsync(evt);

    logger.LogInformation("[Cloud] Event from {ShipId}: {EventType} ({Priority})",
        shipId, evt.EventType, evt.Priority);
    return Results.Ok();
}).RequireRateLimiting("events");

// Receive batch events from ship
app.MapPost("/api/events/{shipId}/batch", async (string shipId, HttpContext context) =>
{
    if (string.IsNullOrWhiteSpace(shipId))
        return Results.BadRequest("shipId is required");

    using var reader = new StreamReader(context.Request.Body);
    var body = await reader.ReadToEndAsync();
    if (body.Length > 10 * 1024 * 1024) // 10MB limit for batches
        return Results.BadRequest("Batch payload exceeds 10MB limit");
    if (string.IsNullOrWhiteSpace(body))
        return Results.BadRequest("Batch body is required");

    List<ShipEvent>? events;
    try
    {
        events = JsonSerializer.Deserialize<List<ShipEvent>>(body, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "Failed to deserialize batch from {ShipId}", shipId);
        return Results.BadRequest("Invalid JSON array");
    }

    if (events == null || events.Count == 0)
        return Results.BadRequest("Empty batch");

    await registry.UpdateLastSeenAsync(shipId);
    await eventStore.StoreBatchAsync(events);

    logger.LogInformation("[Cloud] Batch of {Count} events from {ShipId}", events.Count, shipId);
    return Results.Ok();
}).RequireRateLimiting("events");

// Get recent events for a ship
app.MapGet("/api/events/{shipId}", async (string shipId, int? limit) =>
{
    if (string.IsNullOrWhiteSpace(shipId))
        return Results.BadRequest("shipId is required");

    var events = await eventStore.GetEventsForShipAsync(shipId, limit ?? 100);
    return Results.Ok(events);
});

// Get event summary for a ship
app.MapGet("/api/events/{shipId}/summary", async (string shipId) =>
{
    if (string.IsNullOrWhiteSpace(shipId))
        return Results.BadRequest("shipId is required");

    var summary = await eventStore.GetSummaryForShipAsync(shipId);
    return Results.Ok(summary);
});

// Get critical events across fleet
app.MapGet("/api/events/critical", async (int? limit) =>
{
    var events = await eventStore.GetEventsByPriorityAsync("Critical", limit ?? 50);
    return Results.Ok(events);
});

// Get fleet status
app.MapGet("/api/fleet", async () =>
{
    var ships = await registry.GetAllAsync();
    return Results.Ok(ships);
});

// Get specific ship
app.MapGet("/api/fleet/{shipId}", async (string shipId) =>
{
    if (string.IsNullOrWhiteSpace(shipId))
        return Results.BadRequest("shipId is required");

    var ship = await registry.GetAsync(shipId);
    return ship != null ? Results.Ok(ship) : Results.NotFound();
});

// Register ship
app.MapPost("/api/fleet", async (RegisterShipRequest ship) =>
{
    if (string.IsNullOrWhiteSpace(ship.ShipId))
        return Results.BadRequest("shipId is required");
    if (string.IsNullOrWhiteSpace(ship.Name))
        return Results.BadRequest("name is required");
    if (!string.IsNullOrWhiteSpace(ship.IMONumber) && !IMONumber.IsValid(ship.IMONumber))
        return Results.BadRequest("Invalid IMO number");

    await registry.RegisterAsync(ship.ShipId, ship.Name, ship.IMONumber);
    return Results.Created($"/api/fleet/{ship.ShipId}", ship);
}).RequireRateLimiting("fleet");

// Update ship position
app.MapPost("/api/fleet/{shipId}/position", async (string shipId, PositionUpdateRequest request) =>
{
    if (string.IsNullOrWhiteSpace(shipId))
        return Results.BadRequest("shipId is required");
    if (request.Latitude is < -90 or > 90)
        return Results.BadRequest("Latitude must be between -90 and 90");
    if (request.Longitude is < -180 or > 180)
        return Results.BadRequest("Longitude must be between -180 and 180");

    await registry.UpdatePositionAsync(shipId, request.Latitude, request.Longitude, request.Speed);
    return Results.Ok();
}).RequireRateLimiting("fleet");

// Issue command to ship
app.MapPost("/api/commands/{shipId}", async (string shipId, IssueCommandRequest request) =>
{
    if (string.IsNullOrWhiteSpace(shipId))
        return Results.BadRequest("shipId is required");
    if (string.IsNullOrWhiteSpace(request.CommandType))
        return Results.BadRequest("CommandType is required");
    if (string.IsNullOrWhiteSpace(request.Target))
        return Results.BadRequest("Target is required");

    var cmdId = await commandService.IssueAsync(shipId, request.CommandType, request.Target, request.Parameters ?? new());
    return Results.Accepted($"/api/commands/{cmdId}", new { CommandId = cmdId });
}).RequireRateLimiting("commands");

// Get commands for ship
app.MapGet("/api/commands/{shipId}", async (string shipId) =>
{
    if (string.IsNullOrWhiteSpace(shipId))
        return Results.BadRequest("shipId is required");

    var commands = await commandService.GetPendingForShipAsync(shipId);
    return Results.Ok(commands);
}).RequireRateLimiting("commands");

// Get dead letter queue
app.MapGet("/api/commands/deadletter", async () =>
{
    var commands = await commandService.GetDeadLetterCommandsAsync();
    return Results.Ok(commands);
}).RequireRateLimiting("commands");

// Ship acknowledges command receipt
app.MapPost("/api/commands/{commandId}/delivered", async (Guid commandId) =>
{
    var cmd = await commandService.GetAsync(commandId);
    if (cmd == null) return Results.NotFound();
    await commandService.RecordDeliveredAsync(commandId);
    return Results.Ok();
}).RequireRateLimiting("commands");

// Ship reports command execution
app.MapPost("/api/commands/{commandId}/executed", async (Guid commandId, ShipCommandReport report) =>
{
    var cmd = await commandService.GetAsync(commandId);
    if (cmd == null) return Results.NotFound();
    if (cmd.ShipId != report.ShipId)
        return Results.Forbid();
    await commandService.RecordSuccessAsync(commandId);
    return Results.Ok();
}).RequireRateLimiting("commands");

// Ship reports command failure
app.MapPost("/api/commands/{commandId}/failed", async (Guid commandId, ShipCommandReport report) =>
{
    var cmd = await commandService.GetAsync(commandId);
    if (cmd == null) return Results.NotFound();
    if (cmd.ShipId != report.ShipId)
        return Results.Forbid();
    await commandService.RecordFailureAsync(commandId, report.Reason);
    return Results.Ok();
}).RequireRateLimiting("commands");

// Ship polls for pending commands
app.MapGet("/api/commands/{shipId}/pending", async (string shipId) =>
{
    if (string.IsNullOrWhiteSpace(shipId))
        return Results.BadRequest("shipId is required");

    var commands = await commandService.GetPendingForShipAsync(shipId);
    return Results.Ok(commands);
}).RequireRateLimiting("commands");

app.Run();

public record RegisterShipRequest(string ShipId, string Name, string? IMONumber);
public record IssueCommandRequest(string CommandType, string Target, Dictionary<string, string>? Parameters);
public record ShipCommandReport(string ShipId, string? Reason);
public record PositionUpdateRequest(double Latitude, double Longitude, double? Speed);

public class DatabaseHealthCheck : IHealthCheck
{
    private readonly IShipRegistryService _registry;

    public DatabaseHealthCheck(IShipRegistryService registry)
    {
        _registry = registry;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _registry.GetAllAsync();
            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Database connectivity failed", ex);
        }
    }
}

public class EventStoreHealthCheck : IHealthCheck
{
    private readonly IEventStoreService _eventStore;

    public EventStoreHealthCheck(IEventStoreService eventStore)
    {
        _eventStore = eventStore;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _eventStore.GetEventsForShipAsync("health-check", 1);
            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Event store connectivity failed", ex);
        }
    }
}
