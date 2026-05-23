using CargoShipMonitoring.Shared.Models;

namespace CargoShipMonitoring.Shared.Events;

public record CommandIssuedEvent : ShipEvent
{
    public Guid CommandId { get; init; } = Guid.NewGuid();
    public string CommandType { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
    public Dictionary<string, string> Parameters { get; init; } = new();
    public CommandStatus Status { get; init; } = CommandStatus.Issued;

    public CommandIssuedEvent()
    {
        EventType = "command.issued";
        Priority = Priority.Operational;
    }
}
