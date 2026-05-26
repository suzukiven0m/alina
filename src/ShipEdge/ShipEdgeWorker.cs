using CargoShipMonitoring.Shared.Events;
using CargoShipMonitoring.Shared.Models;
using CargoShipMonitoring.ShipEdge.PriorityEventQueue;
using CargoShipMonitoring.ShipEdge.RulesEngine;
using CargoShipMonitoring.ShipEdge.SatelliteGateway;
using CargoShipMonitoring.ShipEdge.TelemetryCollector;
using Microsoft.Extensions.Options;

namespace CargoShipMonitoring.ShipEdge;

public class ShipEdgeWorker : BackgroundService
{
    private readonly string _shipId;
    private readonly IPriorityEventQueue _queue;
    private readonly IRulesEngine _rulesEngine;
    private readonly ISatelliteGateway _satelliteGateway;
    private readonly ISensorReader _sensorReader;
    private readonly ILogger<ShipEdgeWorker> _logger;
    private readonly SemaphoreSlim _persistLock = new(1, 1);

    public ShipEdgeWorker(
        IPriorityEventQueue queue,
        IRulesEngine rulesEngine,
        ISatelliteGateway satelliteGateway,
        ISensorReader sensorReader,
        IOptions<ShipEdgeConfig> config,
        ILogger<ShipEdgeWorker> logger)
    {
        _shipId = config.Value.ShipId;
        _queue = queue;
        _rulesEngine = rulesEngine;
        _satelliteGateway = satelliteGateway;
        _sensorReader = sensorReader;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _queue.RestoreAsync();
        _logger.LogInformation("[{ShipId}] Ship edge started. Queue count: {Count}", _shipId, _queue.Count);

        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(5));

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await Task.WhenAll(CollectTelemetryAsync(), ProcessQueueAsync());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{ShipId}] Worker error", _shipId);
            }
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("[{ShipId}] Ship edge stopping. Persisting queue...", _shipId);
        await PersistWithLockAsync();
        await base.StopAsync(cancellationToken);
    }

    public override void Dispose()
    {
        _persistLock.Dispose();
        base.Dispose();
    }

    private async Task CollectTelemetryAsync()
    {
        var readings = _sensorReader.ReadAllSensors();

        foreach (var reading in readings)
        {
            var sensorEvent = new SensorReadingEvent
            {
                ShipId = _shipId,
                Reading = reading
            };
            _queue.Enqueue(sensorEvent);

            var ruleResults = _rulesEngine.Evaluate(reading);
            foreach (var result in ruleResults)
            {
                var alert = new AlertTriggeredEvent
                {
                    ShipId = _shipId,
                    Priority = result.Priority,
                    RuleId = result.RuleId,
                    RuleName = result.RuleName,
                    TriggerReading = reading,
                    Message = result.Message
                };
                _queue.Enqueue(alert);

                _logger.LogWarning("[{ShipId}] ALERT: {RuleName} - {Message}", _shipId, result.RuleName, result.Message);

                foreach (var action in result.Actions)
                {
                    ExecuteActuator(action, reading);
                }
            }
        }

        await PersistWithLockAsync();
    }

    private void ExecuteActuator(ActuatorCommand command, SensorReading reading)
    {
        switch (command)
        {
            case ActuatorCommand.EmitAlert:
                _logger.LogWarning("[{ShipId}] ALERT triggered for {SensorType}", _shipId, reading.SensorType);
                break;
            case ActuatorCommand.TriggerFireSuppression:
                _logger.LogCritical("[{ShipId}] ACTUATOR: Fire suppression activated for {Location}", _shipId, reading.Location);
                break;
            case ActuatorCommand.StartBilgePump:
                _logger.LogCritical("[{ShipId}] ACTUATOR: Bilge pump started", _shipId);
                break;
            case ActuatorCommand.ReduceEnginePower:
                _logger.LogWarning("[{ShipId}] ACTUATOR: Engine power reduced", _shipId);
                break;
            case ActuatorCommand.ShutdownReefer:
                _logger.LogCritical("[{ShipId}] ACTUATOR: Reefer emergency shutdown", _shipId);
                break;
        }
    }

    private async Task PersistWithLockAsync()
    {
        await _persistLock.WaitAsync();
        try
        {
            await _queue.PersistAsync();
        }
        finally
        {
            _persistLock.Release();
        }
    }

    private async Task ProcessQueueAsync()
    {
        if (!_satelliteGateway.CanTransmit())
            return;

        var stagedEvents = new List<ShipEvent>();
        var criticalEvents = new List<ShipEvent>();

        ShipEvent? evt;
        while ((evt = _queue.Dequeue()) != null && stagedEvents.Count + criticalEvents.Count < 50)
        {
            if (evt.Priority == Priority.Critical)
                criticalEvents.Add(evt);
            else
                stagedEvents.Add(evt);
        }

        var failedCritical = new List<ShipEvent>();

        // Send critical events individually and track failures
        foreach (var critical in criticalEvents)
        {
            if (!await _satelliteGateway.TransmitAsync(critical))
                failedCritical.Add(critical);
        }

        // Send batch — if it fails, all staged events need retry
        var batchFailed = false;
        if (stagedEvents.Count > 0)
        {
            if (!await _satelliteGateway.TransmitBatchAsync(stagedEvents))
                batchFailed = true;
        }

        // Re-enqueue only events that actually failed
        foreach (var failed in failedCritical)
            _queue.Enqueue(failed);

        if (batchFailed)
        {
            foreach (var failed in stagedEvents)
                _queue.Enqueue(failed);
        }

        await PersistWithLockAsync();
    }
}
