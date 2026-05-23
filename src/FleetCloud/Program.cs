using CargoShipMonitoring.FleetCloud.ShipRegistry;
using CargoShipMonitoring.FleetCloud.CommandService;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Register services
builder.Services.AddSingleton<IShipRegistryService>(_ => new ShipRegistryService("fleet_registry.db"));
builder.Services.AddSingleton<ICommandService>(_ => new CommandService("fleet_commands.db"));
builder.Services.AddHealthChecks();

var app = builder.Build();

app.MapHealthChecks("/health");

var registry = app.Services.GetRequiredService<IShipRegistryService>();
var commandService = app.Services.GetRequiredService<ICommandService>();
var logger = app.Services.GetRequiredService<ILogger<Program>>();

// Receive events from ship
app.MapPost("/api/events/{shipId}", async (string shipId, HttpContext context) =>
{
    if (string.IsNullOrWhiteSpace(shipId))
        return Results.BadRequest("shipId is required");

    if (context.Request.ContentLength > 1024 * 1024) // 1MB limit
        return Results.BadRequest("Event payload exceeds 1MB limit");

    var body = await new StreamReader(context.Request.Body).ReadToEndAsync();
    if (string.IsNullOrWhiteSpace(body))
        return Results.BadRequest("Event body is required");

    JsonElement evt;
    try
    {
        evt = JsonSerializer.Deserialize<JsonElement>(body);
    }
    catch (JsonException)
    {
        return Results.BadRequest("Invalid JSON");
    }

    await registry.UpdateLastSeenAsync(shipId);

    logger.LogInformation("[Cloud] Event from {ShipId}: {Event}", shipId, evt);
    return Results.Ok();
});

// Receive batch events from ship
app.MapPost("/api/events/{shipId}/batch", async (string shipId, HttpContext context) =>
{
    if (string.IsNullOrWhiteSpace(shipId))
        return Results.BadRequest("shipId is required");

    if (context.Request.ContentLength > 10 * 1024 * 1024) // 10MB limit for batches
        return Results.BadRequest("Batch payload exceeds 10MB limit");

    var body = await new StreamReader(context.Request.Body).ReadToEndAsync();
    if (string.IsNullOrWhiteSpace(body))
        return Results.BadRequest("Batch body is required");

    List<JsonElement>? events;
    try
    {
        events = JsonSerializer.Deserialize<List<JsonElement>>(body);
    }
    catch (JsonException)
    {
        return Results.BadRequest("Invalid JSON array");
    }

    await registry.UpdateLastSeenAsync(shipId);

    logger.LogInformation("[Cloud] Batch of {Count} events from {ShipId}", events?.Count ?? 0, shipId);
    return Results.Ok();
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
app.MapPost("/api/fleet", async (ShipInfo ship) =>
{
    if (string.IsNullOrWhiteSpace(ship.ShipId))
        return Results.BadRequest("shipId is required");
    if (string.IsNullOrWhiteSpace(ship.Name))
        return Results.BadRequest("name is required");

    await registry.RegisterAsync(ship.ShipId, ship.Name, ship.IMONumber);
    return Results.Created($"/api/fleet/{ship.ShipId}", ship);
});

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
});

// Get commands for ship
app.MapGet("/api/commands/{shipId}", async (string shipId) =>
{
    if (string.IsNullOrWhiteSpace(shipId))
        return Results.BadRequest("shipId is required");

    var commands = await commandService.GetPendingForShipAsync(shipId);
    return Results.Ok(commands);
});

// Get dead letter queue
app.MapGet("/api/commands/deadletter", async () =>
{
    var commands = await commandService.GetDeadLetterCommandsAsync();
    return Results.Ok(commands);
});

// Ship acknowledges command receipt
app.MapPost("/api/commands/{commandId}/delivered", async (Guid commandId) =>
{
    var cmd = await commandService.GetAsync(commandId);
    if (cmd == null) return Results.NotFound();
    return Results.Ok();
});

// Ship reports command execution
app.MapPost("/api/commands/{commandId}/executed", async (Guid commandId) =>
{
    await commandService.RecordSuccessAsync(commandId);
    return Results.Ok();
});

// Ship reports command failure
app.MapPost("/api/commands/{commandId}/failed", async (Guid commandId, FailureRequest request) =>
{
    await commandService.RecordFailureAsync(commandId, request.Reason);
    return Results.Ok();
});

// Ship polls for pending commands
app.MapGet("/api/commands/{shipId}/pending", async (string shipId) =>
{
    if (string.IsNullOrWhiteSpace(shipId))
        return Results.BadRequest("shipId is required");

    var commands = await commandService.GetPendingForShipAsync(shipId);
    return Results.Ok(commands);
});

app.Run();

public record IssueCommandRequest(string CommandType, string Target, Dictionary<string, string>? Parameters);
public record FailureRequest(string? Reason);
