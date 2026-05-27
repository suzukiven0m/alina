using CargoShipMonitoring.FleetCloud.CommandService;
using CargoShipMonitoring.Shared.Models;

namespace CargoShipMonitoring.FleetCloud.Tests;

public class CommandServiceTests : IDisposable
{
    private readonly string _dbPath;

    public CommandServiceTests()
    {
        _dbPath = $"test_commands_{Guid.NewGuid()}.db";
    }

    public void Dispose()
    {
        if (File.Exists(_dbPath))
            File.Delete(_dbPath);
    }

    [Fact]
    public async Task IssueCommand_Creates_PendingCommand()
    {
        var service = new CommandService.CommandService(_dbPath);
        var cmdId = await service.IssueAsync("MSC-001", "set_reefer_temp", "REEFER-01", new Dictionary<string, string> { ["temp"] = "-18" });

        var cmd = await service.GetAsync(cmdId);
        Assert.NotNull(cmd);
        Assert.Equal(CommandStatus.Issued, cmd.Status);
        Assert.Equal(0, cmd.RetryCount);
    }

    [Fact]
    public async Task Retry_Incremented_On_Failure()
    {
        var service = new CommandService.CommandService(_dbPath);
        var cmdId = await service.IssueAsync("MSC-001", "test", "target", new());

        await service.RecordFailureAsync(cmdId);

        var cmd = await service.GetAsync(cmdId);
        Assert.Equal(1, cmd!.RetryCount);
        Assert.Equal(CommandStatus.Issued, cmd.Status);
    }

    [Fact]
    public async Task Max_Retries_Moves_To_DLQ()
    {
        var service = new CommandService.CommandService(_dbPath, maxRetries: 3);
        var cmdId = await service.IssueAsync("MSC-001", "test", "target", new());

        await service.RecordFailureAsync(cmdId);
        await service.RecordFailureAsync(cmdId);
        await service.RecordFailureAsync(cmdId);

        var cmd = await service.GetAsync(cmdId);
        Assert.Equal(CommandStatus.DeadLetter, cmd!.Status);
    }

    [Fact]
    public async Task RecordSuccess_Updates_Status()
    {
        var service = new CommandService.CommandService(_dbPath);
        var cmdId = await service.IssueAsync("MSC-001", "test", "target", new());

        await service.RecordSuccessAsync(cmdId);

        var cmd = await service.GetAsync(cmdId);
        Assert.Equal(CommandStatus.Executed, cmd!.Status);
    }

    [Fact]
    public async Task GetPendingCommands_Filters_By_Status()
    {
        var service = new CommandService.CommandService(_dbPath);
        await service.IssueAsync("MSC-001", "cmd1", "target", new());
        await service.IssueAsync("MSC-001", "cmd2", "target", new());

        var pending = await service.GetPendingForShipAsync("MSC-001");
        Assert.Equal(2, pending.Count);
    }

    [Fact]
    public async Task RecordDelivered_Updates_Status()
    {
        var service = new CommandService.CommandService(_dbPath);
        var cmdId = await service.IssueAsync("MSC-001", "test", "target", new());

        await service.RecordDeliveredAsync(cmdId);

        var cmd = await service.GetAsync(cmdId);
        Assert.Equal(CommandStatus.Delivered, cmd!.Status);
    }

    [Fact]
    public async Task RecordRejected_Updates_Status()
    {
        var service = new CommandService.CommandService(_dbPath);
        var cmdId = await service.IssueAsync("MSC-001", "test", "target", new());

        await service.RecordRejectedAsync(cmdId, "Invalid target");

        var cmd = await service.GetAsync(cmdId);
        Assert.Equal(CommandStatus.Rejected, cmd!.Status);
        Assert.Equal("Invalid target", cmd.FailureReason);
    }

    [Fact]
    public async Task GetPendingCommands_Respects_NextRetryAt()
    {
        var service = new CommandService.CommandService(_dbPath);
        var cmdId = await service.IssueAsync("MSC-001", "test", "target", new());

        // After failure, NextRetryAt is set to the future
        await service.RecordFailureAsync(cmdId);

        var pending = await service.GetPendingForShipAsync("MSC-001");
        Assert.Empty(pending);
    }
}
