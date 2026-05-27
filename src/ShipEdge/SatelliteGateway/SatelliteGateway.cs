using CargoShipMonitoring.Shared.Events;
using System.Text;
using System.Text.Json;

namespace CargoShipMonitoring.ShipEdge.SatelliteGateway;

public class SatelliteGateway : ISatelliteGateway
{
    private readonly CircuitBreaker _circuitBreaker;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _shipId;
    private readonly string _cloudEndpoint;
    private readonly ILogger<SatelliteGateway> _logger;

    public SatelliteGateway(
        CircuitBreaker circuitBreaker,
        IHttpClientFactory httpClientFactory,
        string cloudEndpoint,
        string shipId,
        ILogger<SatelliteGateway> logger)
    {
        _circuitBreaker = circuitBreaker;
        _httpClientFactory = httpClientFactory;
        _cloudEndpoint = cloudEndpoint.TrimEnd('/');
        _shipId = shipId;
        _logger = logger;
    }

    public bool CanTransmit() => _circuitBreaker.CanExecute();

    public async Task<bool> TransmitAsync(ShipEvent evt)
    {
        if (!_circuitBreaker.CanExecute())
            return false;

        var httpClient = _httpClientFactory.CreateClient("satellite");

        try
        {
            var json = JsonSerializer.Serialize(evt);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync($"{_cloudEndpoint}/api/events/{_shipId}", content);

            if (response.IsSuccessStatusCode)
            {
                _circuitBreaker.RecordSuccess();
                return true;
            }

            _circuitBreaker.RecordFailure();
            return false;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "[{ShipId}] Satellite transmission failed: {Message}", _shipId, ex.Message);
            _circuitBreaker.RecordFailure();
            return false;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "[{ShipId}] Unexpected error during satellite transmission", _shipId);
            _circuitBreaker.RecordFailure();
            return false;
        }
    }

    public async Task<bool> TransmitBatchAsync(List<ShipEvent> events)
    {
        if (!_circuitBreaker.CanExecute())
            return false;
        if (events.Count == 0)
            return true;

        var httpClient = _httpClientFactory.CreateClient("satellite");

        try
        {
            var json = JsonSerializer.Serialize(events);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync($"{_cloudEndpoint}/api/events/{_shipId}/batch", content);

            if (response.IsSuccessStatusCode)
            {
                _circuitBreaker.RecordSuccess();
                return true;
            }

            _logger.LogError("[{ShipId}] Batch transmission failed with status {StatusCode}", _shipId, response.StatusCode);
            _circuitBreaker.RecordFailure();
            return false;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "[{ShipId}] Batch satellite transmission failed: {Message}", _shipId, ex.Message);
            _circuitBreaker.RecordFailure();
            return false;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "[{ShipId}] Unexpected error during batch satellite transmission", _shipId);
            _circuitBreaker.RecordFailure();
            return false;
        }
    }
}
