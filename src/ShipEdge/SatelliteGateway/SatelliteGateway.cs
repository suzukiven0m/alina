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

    public SatelliteGateway(
        CircuitBreaker circuitBreaker,
        IHttpClientFactory httpClientFactory,
        string cloudEndpoint,
        string shipId)
    {
        _circuitBreaker = circuitBreaker;
        _httpClientFactory = httpClientFactory;
        _cloudEndpoint = cloudEndpoint.TrimEnd('/');
        _shipId = shipId;
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
        catch (HttpRequestException)
        {
            _circuitBreaker.RecordFailure();
            return false;
        }
        catch (Exception)
        {
            _circuitBreaker.RecordFailure();
            return false;
        }
    }

    public async Task<bool> TransmitBatchAsync(List<ShipEvent> events)
    {
        if (!_circuitBreaker.CanExecute() || events.Count == 0)
            return false;

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

            _circuitBreaker.RecordFailure();
            return false;
        }
        catch (HttpRequestException)
        {
            _circuitBreaker.RecordFailure();
            return false;
        }
        catch (Exception)
        {
            _circuitBreaker.RecordFailure();
            return false;
        }
    }
}
