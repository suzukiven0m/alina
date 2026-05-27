using CargoShipMonitoring.Shared.Models;

namespace CargoShipMonitoring.FleetCloud.CommandService;

public interface ICommandService
{
    Task<Guid> IssueAsync(string shipId, string commandType, string target, Dictionary<string, string> parameters);
    Task<PendingCommand?> GetAsync(Guid commandId);
    Task<List<PendingCommand>> GetPendingForShipAsync(string shipId);
    Task<List<PendingCommand>> GetDeadLetterCommandsAsync();
    Task RecordFailureAsync(Guid commandId, string? reason = null);
    Task RecordSuccessAsync(Guid commandId);
    Task RecordDeliveredAsync(Guid commandId);
    Task RecordRejectedAsync(Guid commandId, string reason);
    Task<List<PendingCommand>> GetCommandsReadyForRetryAsync();
    Task ResetRetryTimerAsync(Guid commandId);
}
