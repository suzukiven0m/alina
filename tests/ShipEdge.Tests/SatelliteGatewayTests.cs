using CargoShipMonitoring.Shared.Events;
using System.Net;
using SatelliteGatewayNs = CargoShipMonitoring.ShipEdge.SatelliteGateway;

namespace CargoShipMonitoring.ShipEdge.Tests;

public class SatelliteGatewayTests
{
    private static SatelliteGatewayNs.SatelliteGateway CreateGateway(SatelliteGatewayNs.CircuitBreaker cb, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        var factory = new HttpClientFactoryStub(statusCode);
        return new SatelliteGatewayNs.SatelliteGateway(cb, factory, "http://localhost:5000", "MSC-001");
    }

    [Fact]
    public void Gateway_Respects_CircuitBreaker_When_Open()
    {
        var cb = new SatelliteGatewayNs.CircuitBreaker(failureThreshold: 1, timeout: TimeSpan.FromHours(1));
        var gateway = CreateGateway(cb);

        cb.RecordFailure();

        var evt = new SensorReadingEvent { ShipId = "MSC-001" };
        var result = gateway.CanTransmit();

        Assert.False(result);
    }

    [Fact]
    public void Gateway_Allows_Transmission_When_Closed()
    {
        var cb = new SatelliteGatewayNs.CircuitBreaker(failureThreshold: 5, timeout: TimeSpan.FromHours(1));
        var gateway = CreateGateway(cb);

        Assert.True(gateway.CanTransmit());
    }

    [Fact]
    public async Task TransmitAsync_Returns_True_On_Success()
    {
        var cb = new SatelliteGatewayNs.CircuitBreaker(failureThreshold: 5, timeout: TimeSpan.FromHours(1));
        var gateway = CreateGateway(cb, HttpStatusCode.OK);

        var evt = new SensorReadingEvent { ShipId = "MSC-001" };
        var result = await gateway.TransmitAsync(evt);

        Assert.True(result);
        Assert.Equal(SatelliteGatewayNs.CircuitBreakerState.Closed, cb.State);
    }

    [Fact]
    public async Task TransmitAsync_Returns_False_On_ServerError()
    {
        var cb = new SatelliteGatewayNs.CircuitBreaker(failureThreshold: 5, timeout: TimeSpan.FromHours(1));
        var gateway = CreateGateway(cb, HttpStatusCode.InternalServerError);

        var evt = new SensorReadingEvent { ShipId = "MSC-001" };
        var result = await gateway.TransmitAsync(evt);

        Assert.False(result);
    }

    [Fact]
    public async Task TransmitAsync_Returns_False_On_NetworkFailure()
    {
        var cb = new SatelliteGatewayNs.CircuitBreaker(failureThreshold: 5, timeout: TimeSpan.FromHours(1));
        var factory = new FailingHttpClientFactoryStub();
        var gateway = new SatelliteGatewayNs.SatelliteGateway(cb, factory, "http://localhost:5000", "MSC-001");

        var evt = new SensorReadingEvent { ShipId = "MSC-001" };
        var result = await gateway.TransmitAsync(evt);

        Assert.False(result);
    }

    [Fact]
    public async Task TransmitBatchAsync_Returns_True_On_Success()
    {
        var cb = new SatelliteGatewayNs.CircuitBreaker(failureThreshold: 5, timeout: TimeSpan.FromHours(1));
        var gateway = CreateGateway(cb, HttpStatusCode.OK);

        var events = new List<ShipEvent>
        {
            new SensorReadingEvent { ShipId = "MSC-001" },
            new SensorReadingEvent { ShipId = "MSC-001" }
        };
        var result = await gateway.TransmitBatchAsync(events);

        Assert.True(result);
    }

    [Fact]
    public async Task TransmitBatchAsync_Returns_False_On_ServerError()
    {
        var cb = new SatelliteGatewayNs.CircuitBreaker(failureThreshold: 5, timeout: TimeSpan.FromHours(1));
        var gateway = CreateGateway(cb, HttpStatusCode.InternalServerError);

        var events = new List<ShipEvent>
        {
            new SensorReadingEvent { ShipId = "MSC-001" }
        };
        var result = await gateway.TransmitBatchAsync(events);

        Assert.False(result);
    }

    [Fact]
    public async Task TransmitAsync_Records_Success_On_CircuitBreaker()
    {
        var cb = new SatelliteGatewayNs.CircuitBreaker(failureThreshold: 1, timeout: TimeSpan.FromHours(1));
        var gateway = CreateGateway(cb, HttpStatusCode.OK);

        // Fail once to open the circuit
        var failingFactory = new FailingHttpClientFactoryStub();
        var failingGateway = new SatelliteGatewayNs.SatelliteGateway(cb, failingFactory, "http://localhost:5000", "MSC-001");
        await failingGateway.TransmitAsync(new SensorReadingEvent { ShipId = "MSC-001" });
        Assert.Equal(SatelliteGatewayNs.CircuitBreakerState.Open, cb.State);

        // Advance time past timeout and succeed
        var timeProvider = new FakeTimeProvider();
        var cb2 = new SatelliteGatewayNs.CircuitBreaker(failureThreshold: 1, timeout: TimeSpan.FromSeconds(1), timeProvider);
        var gateway2 = new SatelliteGatewayNs.SatelliteGateway(cb2, new HttpClientFactoryStub(HttpStatusCode.OK), "http://localhost:5000", "MSC-001");

        // Fail to open
        var failingGateway2 = new SatelliteGatewayNs.SatelliteGateway(cb2, new FailingHttpClientFactoryStub(), "http://localhost:5000", "MSC-001");
        await failingGateway2.TransmitAsync(new SensorReadingEvent { ShipId = "MSC-001" });
        Assert.Equal(SatelliteGatewayNs.CircuitBreakerState.Open, cb2.State);

        // Advance time and succeed
        timeProvider.Advance(TimeSpan.FromSeconds(2));
        var result = await gateway2.TransmitAsync(new SensorReadingEvent { ShipId = "MSC-001" });
        Assert.True(result);
        Assert.Equal(SatelliteGatewayNs.CircuitBreakerState.Closed, cb2.State);
    }

    private class HttpClientFactoryStub : IHttpClientFactory
    {
        private readonly HttpStatusCode _statusCode;

        public HttpClientFactoryStub(HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            _statusCode = statusCode;
        }

        public HttpClient CreateClient(string name)
        {
            var handler = new MockHttpMessageHandler(_statusCode);
            return new HttpClient(handler);
        }
    }

    private class FailingHttpClientFactoryStub : IHttpClientFactory
    {
        public HttpClient CreateClient(string name)
        {
            var handler = new FailingHttpMessageHandler();
            return new HttpClient(handler);
        }
    }

    private class MockHttpMessageHandler : HttpMessageHandler
    {
        private readonly HttpStatusCode _statusCode;

        public MockHttpMessageHandler(HttpStatusCode statusCode)
        {
            _statusCode = statusCode;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpResponseMessage(_statusCode));
        }
    }

    private class FailingHttpMessageHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            throw new HttpRequestException("Simulated network failure");
        }
    }

    private class FakeTimeProvider : TimeProvider
    {
        private DateTimeOffset _now = DateTimeOffset.UtcNow;

        public override DateTimeOffset GetUtcNow() => _now;

        public void Advance(TimeSpan duration)
        {
            _now = _now.Add(duration);
        }
    }
}
