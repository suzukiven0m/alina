namespace CargoShipMonitoring.ShipEdge.SatelliteGateway;

public enum CircuitBreakerState
{
    Closed,
    Open,
    HalfOpen
}

public class CircuitBreaker
{
    private readonly int _failureThreshold;
    private readonly TimeSpan _timeout;
    private readonly TimeProvider _timeProvider;
    private int _failureCount;
    private DateTimeOffset _lastFailureTime;
    private CircuitBreakerState _state = CircuitBreakerState.Closed;
    private readonly object _lock = new();

    public CircuitBreakerState State
    {
        get
        {
            lock (_lock)
            {
                return _state;
            }
        }
    }

    public CircuitBreaker(int failureThreshold, TimeSpan timeout, TimeProvider? timeProvider = null)
    {
        _failureThreshold = failureThreshold;
        _timeout = timeout;
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    public bool CanExecute()
    {
        lock (_lock)
        {
            if (_state == CircuitBreakerState.Closed)
                return true;

            if (_state == CircuitBreakerState.Open)
            {
                if (_timeProvider.GetUtcNow() - _lastFailureTime >= _timeout)
                {
                    _state = CircuitBreakerState.HalfOpen;
                    return true;
                }
                return false;
            }

            return _state == CircuitBreakerState.HalfOpen;
        }
    }

    public void RecordFailure()
    {
        var newCount = Interlocked.Increment(ref _failureCount);
        lock (_lock)
        {
            _lastFailureTime = _timeProvider.GetUtcNow();
            if (newCount >= _failureThreshold)
                _state = CircuitBreakerState.Open;
        }
    }

    public void RecordSuccess()
    {
        Interlocked.Exchange(ref _failureCount, 0);
        lock (_lock)
        {
            _state = CircuitBreakerState.Closed;
        }
    }
}
