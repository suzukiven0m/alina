namespace CargoShipMonitoring.Shared.Models;

public enum CommandStatus
{
    Issued,
    Delivered,
    Executed,
    Rejected,
    Failed,
    DeadLetter
}
