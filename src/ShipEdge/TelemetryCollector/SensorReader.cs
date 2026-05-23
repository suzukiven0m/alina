using CargoShipMonitoring.Shared.Models;

namespace CargoShipMonitoring.ShipEdge.TelemetryCollector;

public class SensorReader : ISensorReader
{
    private readonly string _shipId;
    public SensorReader(string shipId)
    {
        _shipId = shipId;
    }

    public List<SensorReading> ReadAllSensors()
    {
        var r = Random.Shared;
        return new List<SensorReading>
        {
            new SensorReading
            {
                SensorId = "ENGINE-TEMP-01",
                SensorType = SensorType.EngineTemperature,
                Value = 90 + r.NextDouble() * 30,
                Unit = "C",
                Timestamp = DateTimeOffset.UtcNow
            },
            new SensorReading
            {
                SensorId = "BILGE-01",
                SensorType = SensorType.BilgeLevel,
                Value = r.NextDouble() * 100,
                Unit = "%",
                Timestamp = DateTimeOffset.UtcNow
            },
            new SensorReading
            {
                SensorId = "SMOKE-HOLD3",
                SensorType = SensorType.SmokeDetector,
                Value = r.NextDouble() * 0.2,
                Unit = "ppm",
                Location = "HOLD-3",
                Timestamp = DateTimeOffset.UtcNow
            },
            new SensorReading
            {
                SensorId = "REEFER-01",
                SensorType = SensorType.ReeferTemperature,
                Value = -20 + r.NextDouble() * 10,
                Unit = "C",
                Timestamp = DateTimeOffset.UtcNow
            }
        };
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
                SensorType.EngineTemperature => "C",
                SensorType.BilgeLevel => "%",
                SensorType.SmokeDetector => "ppm",
                SensorType.ReeferTemperature => "C",
                _ => ""
            },
            Timestamp = DateTimeOffset.UtcNow
        };
    }
}
