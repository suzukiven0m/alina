using CargoShipMonitoring.Shared.Models;

namespace CargoShipMonitoring.Shared.Events;

public record AlertTriggeredEvent : ShipEvent
{
    public string RuleId { get; init; } = string.Empty;
    public string RuleName { get; init; } = string.Empty;
    public SensorReading TriggerReading { get; init; } = new();
    public string Message { get; init; } = string.Empty;

    public AlertTriggeredEvent()
    {
        EventType = "alert.triggered";
        Priority = Priority.Operational;
    }
}
