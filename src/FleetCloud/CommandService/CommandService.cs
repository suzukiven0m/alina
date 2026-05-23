using CargoShipMonitoring.Shared.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace CargoShipMonitoring.FleetCloud.CommandService;

public class CommandService : ICommandService
{
    private readonly string _dbPath;
    private readonly int _maxRetries;

    public CommandService(string dbPath, int maxRetries = 10)
    {
        _dbPath = dbPath;
        _maxRetries = maxRetries;

        using var db = new CommandDbContext(_dbPath);
        db.Database.EnsureCreated();
    }

    public async Task<Guid> IssueAsync(string shipId, string commandType, string target, Dictionary<string, string> parameters)
    {
        using var db = new CommandDbContext(_dbPath);

        var cmd = new PendingCommand
        {
            CommandId = Guid.NewGuid(),
            ShipId = shipId,
            CommandType = commandType,
            Target = target,
            ParametersJson = JsonSerializer.Serialize(parameters),
            Status = CommandStatus.Issued,
            NextRetryAt = DateTimeOffset.UtcNow
        };

        db.Commands.Add(cmd);
        await db.SaveChangesAsync();

        return cmd.CommandId;
    }

    public async Task<PendingCommand?> GetAsync(Guid commandId)
    {
        using var db = new CommandDbContext(_dbPath);
        return await db.Commands.FindAsync(commandId);
    }

    public async Task<List<PendingCommand>> GetPendingForShipAsync(string shipId)
    {
        using var db = new CommandDbContext(_dbPath);

        return await db.Commands
            .Where(c => c.ShipId == shipId && c.Status == CommandStatus.Issued)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<PendingCommand>> GetDeadLetterCommandsAsync()
    {
        using var db = new CommandDbContext(_dbPath);

        return await db.Commands
            .Where(c => c.Status == CommandStatus.DeadLetter)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task RecordFailureAsync(Guid commandId, string? reason = null)
    {
        using var db = new CommandDbContext(_dbPath);

        var cmd = await db.Commands.FindAsync(commandId);
        if (cmd == null) return;

        cmd.RetryCount++;
        cmd.LastAttemptAt = DateTimeOffset.UtcNow;
        cmd.FailureReason = reason;

        if (cmd.RetryCount >= _maxRetries)
        {
            cmd.Status = CommandStatus.DeadLetter;
            cmd.NextRetryAt = null;
        }
        else
        {
            var backoffSeconds = Math.Min(300, Math.Pow(2, cmd.RetryCount) * 5);
            cmd.NextRetryAt = DateTimeOffset.UtcNow.AddSeconds(backoffSeconds);
        }

        await db.SaveChangesAsync();
    }

    public async Task RecordSuccessAsync(Guid commandId)
    {
        using var db = new CommandDbContext(_dbPath);

        var cmd = await db.Commands.FindAsync(commandId);
        if (cmd == null) return;

        cmd.Status = CommandStatus.Executed;
        cmd.NextRetryAt = null;
        await db.SaveChangesAsync();
    }

    public async Task RecordRejectedAsync(Guid commandId, string reason)
    {
        using var db = new CommandDbContext(_dbPath);

        var cmd = await db.Commands.FindAsync(commandId);
        if (cmd == null) return;

        cmd.Status = CommandStatus.Rejected;
        cmd.FailureReason = reason;
        cmd.NextRetryAt = null;
        await db.SaveChangesAsync();
    }

}
