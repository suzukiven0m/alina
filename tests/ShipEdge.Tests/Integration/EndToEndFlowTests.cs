using CargoShipMonitoring.Shared.Events;
using CargoShipMonitoring.Shared.Models;
using CargoShipMonitoring.ShipEdge.RulesEngine;
using PriorityQueueNs = CargoShipMonitoring.ShipEdge.PriorityEventQueue;
using SatelliteGatewayNs = CargoShipMonitoring.ShipEdge.SatelliteGateway;

namespace CargoShipMonitoring.ShipEdge.Tests.Integration;

public class EndToEndFlowTests : IDisposable
{
    private readonly string _dbPath;

    public EndToEndFlowTests()
    {
        _dbPath = $"integration_test_{Guid.NewGuid()}.db";
    }

    public void Dispose()
    {
        if (File.Exists(_dbPath))
            File.Delete(_dbPath);
    }

    [Fact]
    public async Task Critical_Event_Persists_When_Satellite_Down()
    {
        // Arrange
        var queue = new PriorityQueueNs.PriorityEventQueue(1000, _dbPath);
        var rulesEngine = new RulesEngine.RulesEngine();
        rulesEngine.LoadRules(new RuleLoader().LoadDefaultRules());

        var cb = new SatelliteGatewayNs.CircuitBreaker(failureThreshold: 1, timeout: TimeSpan.FromHours(1));
        var gateway = new SatelliteGatewayNs.SatelliteGateway(cb, new TestHttpClientFactory(), "http://invalid", "MSC-TEST");

        // Simulate fire detection sensor
        var fireReading = new SensorReading
        {
            SensorId = "SMOKE-HOLD3",
            SensorType = SensorType.SmokeDetector,
            Location = "HOLD-3",
            Value = 1.5,
            Unit = "ppm"
        };

        // Act
        var ruleResults = rulesEngine.Evaluate(fireReading);
        foreach (var result in ruleResults)
        {
            var alert = new AlertTriggeredEvent
            {
                ShipId = "MSC-TEST",
                Priority = result.Priority,
                RuleId = result.RuleId,
                RuleName = result.RuleName,
                TriggerReading = fireReading,
                Message = result.Message
            };
            queue.Enqueue(alert);
        }

        // Try to transmit with satellite down
        var transmitted = false;
        var evt = queue.Dequeue();
        if (evt != null && gateway.CanTransmit())
        {
            transmitted = await gateway.TransmitAsync(evt);
            if (!transmitted)
            {
                queue.Enqueue(evt);
            }
        }

        await queue.PersistAsync();

        // Assert
        Assert.False(transmitted);
        Assert.Equal(SatelliteGatewayNs.CircuitBreakerState.Open, cb.State);

        // Verify event survived restart
        var restoredQueue = new PriorityQueueNs.PriorityEventQueue(1000, _dbPath);
        await restoredQueue.RestoreAsync();
        Assert.True(restoredQueue.Count > 0);
    }

    [Fact]
    public void Rules_Engine_Fires_Correctly_For_Fire_Detection()
    {
        var engine = new RulesEngine.RulesEngine();
        engine.LoadRules(new RuleLoader().LoadDefaultRules());

        var reading = new SensorReading
        {
            SensorType = SensorType.SmokeDetector,
            Location = "HOLD-3",
            Value = 1.0
        };

        var results = engine.Evaluate(reading);

        Assert.Single(results);
        Assert.Contains(ActuatorCommand.TriggerFireSuppression, results[0].Actions);
        Assert.Equal(Priority.Critical, results[0].Priority);
    }

    [Fact]
    public void Rules_Engine_Fires_Correctly_For_Engine_Overheat()
    {
        var engine = new RulesEngine.RulesEngine();
        engine.LoadRules(new RuleLoader().LoadDefaultRules());

        var reading = new SensorReading
        {
            SensorType = SensorType.EngineTemperature,
            Value = 120
        };

        var results = engine.Evaluate(reading);

        Assert.Single(results);
        Assert.Equal("RULE-001", results[0].RuleId);
    }
}
