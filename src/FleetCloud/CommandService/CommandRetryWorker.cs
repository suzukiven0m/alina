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
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var commandService = scope.ServiceProvider.GetRequiredService<ICommandService>();
                var pendingCommands = await commandService.GetCommandsReadyForRetryAsync();

                foreach (var cmd in pendingCommands)
                {
                    _logger.LogInformation("Retrying command {CommandId} for ship {ShipId}",
                        cmd.CommandId, cmd.ShipId);
                    await commandService.ResetRetryTimerAsync(cmd.CommandId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in command retry worker");
            }
        }
    }
}
