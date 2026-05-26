using CargoShipMonitoring.Shared.Models;

namespace CargoShipMonitoring.ShipEdge.TelemetryCollector;

public class RealisticSensorReader : ISensorReader
{
    private readonly string _shipId;
    private double _engineTemp = 95.0;
    private double _bilgeLevel = 15.0;
    private double _smokeBaseline = 0.02;
    private double _reeferTemp = -20.0;
    private double _rpm = 800;
    private bool _bilgePumpActive = false;

    public RealisticSensorReader(string shipId)
    {
        _shipId = shipId;
    }

    public List<SensorReading> ReadAllSensors()
    {
        SimulateEngine();
        SimulateBilge();
        SimulateSmoke();
        SimulateReefer();

        return new List<SensorReading>
        {
            new()
            {
                SensorId = "ENGINE-TEMP-01",
                SensorType = SensorType.EngineTemperature,
                Value = Math.Round(_engineTemp, 2),
                Unit = "°C",
                Timestamp = DateTimeOffset.UtcNow
            },
            new()
            {
                SensorId = "BILGE-01",
                SensorType = SensorType.BilgeLevel,
                Value = Math.Round(_bilgeLevel, 2),
                Unit = "%",
                Timestamp = DateTimeOffset.UtcNow
            },
            new()
            {
                SensorId = "SMOKE-HOLD3",
                SensorType = SensorType.SmokeDetector,
                Value = Math.Round(_smokeBaseline, 3),
                Location = "HOLD-3",
                Unit = "ppm",
                Timestamp = DateTimeOffset.UtcNow
            },
            new()
            {
                SensorId = "REEFER-01",
                SensorType = SensorType.ReeferTemperature,
                Value = Math.Round(_reeferTemp, 2),
                Unit = "°C",
                Timestamp = DateTimeOffset.UtcNow
            },
            new()
            {
                SensorId = "ENGINE-RPM-01",
                SensorType = SensorType.EngineRpm,
                Value = Math.Round(_rpm, 0),
                Unit = "RPM",
                Timestamp = DateTimeOffset.UtcNow
            }
        };
    }

    private void SimulateEngine()
    {
        var targetTemp = 85 + (_rpm / 1000.0) * 25;
        var noise = (Random.Shared.NextDouble() - 0.5) * 2;
        _engineTemp += (targetTemp - _engineTemp) * 0.1 + noise;
        _rpm += (Random.Shared.NextDouble() - 0.5) * 20;
        _rpm = Math.Clamp(_rpm, 400, 1200);
    }

    private void SimulateBilge()
    {
        if (_bilgePumpActive)
        {
            _bilgeLevel -= 5;
            if (_bilgeLevel < 10) _bilgePumpActive = false;
        }
        else
        {
            _bilgeLevel += Random.Shared.NextDouble() * 2;
            if (_bilgeLevel > 75) _bilgePumpActive = true;
        }
        _bilgeLevel = Math.Clamp(_bilgeLevel, 0, 100);
    }

    private void SimulateSmoke()
    {
        _smokeBaseline = 0.02 + (Random.Shared.NextDouble() - 0.5) * 0.01;
        if (Random.Shared.NextDouble() < 0.01)
            _smokeBaseline += Random.Shared.NextDouble() * 2;
    }

    private void SimulateReefer()
    {
        var setpoint = -20;
        var drift = (Random.Shared.NextDouble() - 0.5) * 0.5;
        _reeferTemp += (setpoint - _reeferTemp) * 0.05 + drift;
    }

    public SensorReading TriggerEmergency(string sensorId, SensorType type, double value)
    {
        return new SensorReading
        {
            SensorId = sensorId,
            SensorType = type,
            Value = value,
            Unit = type switch
            {
                SensorType.EngineTemperature => "°C",
                SensorType.BilgeLevel => "%",
                SensorType.SmokeDetector => "ppm",
                SensorType.ReeferTemperature => "°C",
                SensorType.EngineRpm => "RPM",
                _ => ""
            },
            Timestamp = DateTimeOffset.UtcNow
        };
    }
}
