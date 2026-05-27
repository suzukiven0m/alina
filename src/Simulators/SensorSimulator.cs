using CargoShipMonitoring.Shared.Events;
using CargoShipMonitoring.Shared.Models;
using System.Text;
using System.Text.Json;

namespace CargoShipMonitoring.Simulators;

public class SensorSimulator : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly string _shipId;
    private readonly Random _random = new();
    private bool _disposed;

    public SensorSimulator(string cloudEndpoint, string shipId)
    {
        _shipId = shipId;
        _httpClient = new HttpClient { BaseAddress = new Uri(cloudEndpoint) };
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _httpClient.Dispose();
            _disposed = true;
        }
    }

    public async Task SimulateNormalReadingsAsync(int count = 5)
    {
        for (int i = 0; i < count; i++)
        {
            var evt = new SensorReadingEvent
            {
                ShipId = _shipId,
                Reading = new SensorReading
                {
                    SensorId = $"SENSOR-{i}",
                    SensorType = SensorType.EngineTemperature,
                    Value = 90 + _random.NextDouble() * 20,
                    Unit = "C",
                    Timestamp = DateTimeOffset.UtcNow
                }
            };

            await SendEventAsync(evt);
            await Task.Delay(100);
        }
    }

    public async Task SimulateCriticalAlertAsync()
    {
        var evt = new SensorReadingEvent
        {
            ShipId = _shipId,
            Priority = Priority.Critical,
            Reading = new SensorReading
            {
                SensorId = "SMOKE-HOLD3",
                SensorType = SensorType.SmokeDetector,
                Value = 2.5,
                Unit = "ppm",
                Location = "HOLD-3",
                Timestamp = DateTimeOffset.UtcNow
            }
        };

        await SendEventAsync(evt);
    }

    private async Task SendEventAsync(ShipEvent evt)
    {
        var json = JsonSerializer.Serialize(evt);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        try
        {
            var response = await _httpClient.PostAsync($"/api/events/{_shipId}", content);
            Console.WriteLine($"[Simulator] Event sent: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Simulator] Failed to send: {ex.Message}");
        }
    }
}
