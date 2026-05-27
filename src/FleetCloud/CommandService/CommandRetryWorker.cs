namespace CargoShipMonitoring.FleetCloud.CommandService;

public class CommandRetryWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CommandRetryWorker> _logger;

    public CommandRetryWorker(IServiceProvider serviceProvider, ILogger<CommandRetryWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(30));

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await ProcessRetriesAsync(stoppingToken);
        }
    }

    public async Task ProcessRetriesAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var commandService = scope.ServiceProvider.GetRequiredService<ICommandService>();
            var readyCommands = await commandService.GetCommandsReadyForRetryAsync();

            if (readyCommands.Count > 0)
            {
                _logger.LogWarning(
                    "{Count} commands are ready for retry but have not been polled by their ships",
                    readyCommands.Count);
            }

            // In a poll-based model, commands become visible automatically when
            // NextRetryAt <= now. This worker monitors for stale commands.
            foreach (var cmd in readyCommands)
            {
                var age = DateTimeOffset.UtcNow - cmd.CreatedAt;
                if (age > TimeSpan.FromHours(24))
                {
                    _logger.LogError(
                        "Command {CommandId} for ship {ShipId} has been retrying for {Hours:F1} hours. Moving to dead letter.",
                        cmd.CommandId, cmd.ShipId, age.TotalHours);
                    await commandService.RecordFailureAsync(cmd.CommandId, "Expired after 24h of retries");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in command retry worker");
        }
    }
}
