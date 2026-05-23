using CargoShipMonitoring.Shared.Models;

namespace CargoShipMonitoring.FleetCloud.CommandService;

public class PendingCommand
{
    public Guid CommandId { get; set; }
    public string ShipId { get; set; } = string.Empty;
    public string CommandType { get; set; } = string.Empty;
    public string Target { get; set; } = string.Empty;
    public string ParametersJson { get; set; } = "{}";
    public CommandStatus Status { get; set; } = CommandStatus.Issued;
    public int RetryCount { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? LastAttemptAt { get; set; }
    public DateTimeOffset? NextRetryAt { get; set; }
    public string? FailureReason { get; set; }
}
