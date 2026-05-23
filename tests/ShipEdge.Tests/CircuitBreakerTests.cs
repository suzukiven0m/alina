using CargoShipMonitoring.ShipEdge.SatelliteGateway;
using Microsoft.Extensions.Time.Testing;

namespace CargoShipMonitoring.ShipEdge.Tests;

public class CircuitBreakerTests
{
    [Fact]
    public void Initial_State_Is_Closed()
    {
        var cb = new CircuitBreaker(failureThreshold: 3, timeout: TimeSpan.FromSeconds(1));
        Assert.Equal(CircuitBreakerState.Closed, cb.State);
    }

    [Fact]
    public void After_FailureThreshold_Reaches_Open()
    {
        var cb = new CircuitBreaker(failureThreshold: 3, timeout: TimeSpan.FromMinutes(60));

        cb.RecordFailure();
        cb.RecordFailure();
        cb.RecordFailure();

        Assert.Equal(CircuitBreakerState.Open, cb.State);
    }

    [Fact]
    public void Open_State_AllowsRequest_After_Timeout()
    {
        var timeProvider = new FakeTimeProvider();
        var cb = new CircuitBreaker(failureThreshold: 1, timeout: TimeSpan.FromMinutes(1), timeProvider);

        cb.RecordFailure();
        Assert.Equal(CircuitBreakerState.Open, cb.State);

        timeProvider.Advance(TimeSpan.FromMinutes(2));
        Assert.True(cb.CanExecute());
        Assert.Equal(CircuitBreakerState.HalfOpen, cb.State);
    }

    [Fact]
    public void HalfOpen_Success_Closes_Breaker()
    {
        var timeProvider = new FakeTimeProvider();
        var cb = new CircuitBreaker(failureThreshold: 1, timeout: TimeSpan.FromMinutes(1), timeProvider);

        cb.RecordFailure();
        timeProvider.Advance(TimeSpan.FromMinutes(2));
        cb.CanExecute(); // transitions to HalfOpen
        cb.RecordSuccess();

        Assert.Equal(CircuitBreakerState.Closed, cb.State);
    }

    [Fact]
    public void HalfOpen_Failure_ReOpens_Breaker()
    {
        var timeProvider = new FakeTimeProvider();
        var cb = new CircuitBreaker(failureThreshold: 1, timeout: TimeSpan.FromMinutes(1), timeProvider);

        cb.RecordFailure();
        timeProvider.Advance(TimeSpan.FromMinutes(2));
        cb.CanExecute();
        cb.RecordFailure();

        Assert.Equal(CircuitBreakerState.Open, cb.State);
    }
}
