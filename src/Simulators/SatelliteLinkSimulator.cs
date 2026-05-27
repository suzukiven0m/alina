namespace CargoShipMonitoring.Simulators;

public class SatelliteLinkSimulator : IDisposable
{
    private bool _isConnected = true;
    private Task? _simulationTask;
    private CancellationTokenSource? _cts;

    public bool IsConnected => _isConnected;

    public void SetConnected(bool connected)
    {
        _isConnected = connected;
        Console.WriteLine($"[SatLink] Connection state: {(_isConnected ? "UP" : "DOWN")}");
    }

    public void SimulateIntermittentConnection(CancellationToken cancellationToken = default)
    {
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _simulationTask = Task.Run(async () =>
        {
            while (!_cts.Token.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(5 + Random.Shared.Next(10)), _cts.Token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                _isConnected = !_isConnected;
                Console.WriteLine($"[SatLink] Connection state changed: {(_isConnected ? "UP" : "DOWN")}");
            }
        }, _cts.Token);
    }

    public void Dispose()
    {
        _cts?.Cancel();
        try
        {
            _simulationTask?.Wait(TimeSpan.FromSeconds(5));
        }
        catch (AggregateException) { }
        _cts?.Dispose();
    }

    public async Task<bool> TryTransmitAsync(Func<Task<bool>> transmitAction)
    {
        if (!_isConnected)
        {
            Console.WriteLine("[SatLink] Transmission failed: no connectivity");
            return false;
        }

        // Simulate latency
        await Task.Delay(Random.Shared.Next(100, 500));

        // Simulate random packet loss (10%)
        if (Random.Shared.NextDouble() < 0.1)
        {
            Console.WriteLine("[SatLink] Transmission failed: packet loss");
            return false;
        }

        return await transmitAction();
    }
}
