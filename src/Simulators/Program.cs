using CargoShipMonitoring.Simulators;

Console.WriteLine("Cargo Ship Monitoring Simulator");
Console.WriteLine("===============================");
Console.WriteLine();
Console.WriteLine("Commands:");
Console.WriteLine("  normal <shipId>    - Send normal telemetry readings");
Console.WriteLine("  critical <shipId>  - Send critical alert");
Console.WriteLine("  connect            - Bring satellite link up");
Console.WriteLine("  disconnect         - Bring satellite link down");
Console.WriteLine("  intermittent       - Start random connection drops");
Console.WriteLine("  exit               - Quit");
Console.WriteLine();

var cloudEndpoint = args.Length > 0 ? args[0] : "http://localhost:5000";
var satLink = new SatelliteLinkSimulator();

while (true)
{
    Console.Write("> ");
    var input = Console.ReadLine()?.Trim() ?? "";
    var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
    if (parts.Length == 0) continue;

    switch (parts[0].ToLower())
    {
        case "normal":
            if (parts.Length > 1)
            {
                var sim = new SensorSimulator(cloudEndpoint, parts[1]);
                await sim.SimulateNormalReadingsAsync();
            }
            break;

        case "critical":
            if (parts.Length > 1)
            {
                var sim = new SensorSimulator(cloudEndpoint, parts[1]);
                await sim.SimulateCriticalAlertAsync();
            }
            break;

        case "connect":
            satLink.SetConnected(true);
            break;

        case "disconnect":
            satLink.SetConnected(false);
            break;

        case "intermittent":
            satLink.SimulateIntermittentConnection();
            break;

        case "exit":
            return;
    }
}
