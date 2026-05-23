using CargoShipMonitoring.Shared.Models;

namespace CargoShipMonitoring.ShipEdge.TelemetryCollector;

public interface ISensorReader
{
    List<SensorReading> ReadAllSensors();
    SensorReading TriggerEmergency(string sensorId, SensorType type, double value);
}
