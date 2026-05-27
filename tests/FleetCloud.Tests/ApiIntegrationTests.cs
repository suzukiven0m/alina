using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CargoShipMonitoring.FleetCloud.CommandService;
using CargoShipMonitoring.FleetCloud.ShipRegistry;
using CargoShipMonitoring.Shared.Models;

namespace CargoShipMonitoring.FleetCloud.Tests;

public class ApiIntegrationTests : IDisposable
{
    private readonly CustomWebApplicationFactory _factory;

    public ApiIntegrationTests()
    {
        _factory = new CustomWebApplicationFactory();
    }

    public void Dispose()
    {
        _factory.Dispose();
    }

    [Fact]
    public async Task HealthCheck_Returns_Ok()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Post_Batch_Event_With_Empty_Body_Returns_BadRequest()
    {
        var client = _factory.CreateClient();

        var content = new StringContent("");
        var response = await client.PostAsync("/api/events/MSC-001/batch", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Post_Event_With_Valid_ShipId_Returns_Ok()
    {
        var client = _factory.CreateClient();

        var payload = new Dictionary<string, object>
        {
            ["$eventType"] = "sensor.reading",
            ["shipId"] = "MSC-001",
            ["eventType"] = "sensor.reading",
            ["reading"] = new Dictionary<string, object>
            {
                ["sensorId"] = "TEMP-01",
                ["sensorType"] = 1, // EngineTemperature = 1
                ["value"] = 95.0,
                ["unit"] = "C"
            }
        };
        var response = await client.PostAsJsonAsync("/api/events/MSC-001", payload);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Post_Batch_Event_Without_ShipId_Returns_BadRequest()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/events/   /batch", new List<object>());

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Register_Ship_And_Get_It_Back()
    {
        var client = _factory.CreateClient();
        var shipId = $"MSC-{Guid.NewGuid()}";

        var registerResponse = await client.PostAsJsonAsync("/api/fleet", new ShipInfo
        {
            ShipId = shipId,
            Name = "Test Ship",
            IMONumber = "1234567"
        });

        Assert.Equal(HttpStatusCode.Created, registerResponse.StatusCode);

        var getResponse = await client.GetAsync($"/api/fleet/{shipId}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var ship = await getResponse.Content.ReadFromJsonAsync<ShipInfo>();
        Assert.NotNull(ship);
        Assert.Equal(shipId, ship.ShipId);
        Assert.Equal("Test Ship", ship.Name);
    }

    [Fact]
    public async Task Get_NonExistent_Ship_Returns_NotFound()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/fleet/MSC-NONEXISTENT");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Issue_Command_And_Get_Pending_Commands()
    {
        var client = _factory.CreateClient();
        var shipId = $"MSC-{Guid.NewGuid()}";

        // Register ship first
        await client.PostAsJsonAsync("/api/fleet", new ShipInfo
        {
            ShipId = shipId,
            Name = "Test Ship"
        });

        // Issue command
        var issueResponse = await client.PostAsJsonAsync($"/api/commands/{shipId}", new
        {
            CommandType = "adjust_course",
            Target = "engine",
            Parameters = new Dictionary<string, string> { { "heading", "270" } }
        });

        Assert.Equal(HttpStatusCode.Accepted, issueResponse.StatusCode);

        // Get pending commands
        var pendingResponse = await client.GetAsync($"/api/commands/{shipId}/pending");
        Assert.Equal(HttpStatusCode.OK, pendingResponse.StatusCode);

        var commands = await pendingResponse.Content.ReadFromJsonAsync<List<PendingCommand>>();
        Assert.NotNull(commands);
        Assert.Single(commands);
        Assert.Equal("adjust_course", commands[0].CommandType);
    }

    [Fact]
    public async Task Post_Large_Event_Returns_BadRequest()
    {
        var client = _factory.CreateClient();

        var largeBody = new string('x', 2 * 1024 * 1024); // 2MB
        var content = new StringContent($"\"{largeBody}\"");
        content.Headers.ContentLength = 2 * 1024 * 1024;

        var response = await client.PostAsync("/api/events/MSC-001", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Invalid_Json_Returns_BadRequest()
    {
        var client = _factory.CreateClient();

        var content = new StringContent("not valid json");
        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

        var response = await client.PostAsync("/api/events/MSC-001", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
