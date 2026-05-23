namespace CargoShipMonitoring.ShipEdge;

public class ShipEdgeConfig
{
    public string ShipId { get; set; } = "UNKNOWN";
    public string DbPath { get; set; } = "ship_buffer.db";
    public string CloudEndpoint { get; set; } = "http://localhost:5000";
    public CircuitBreakerConfig CircuitBreaker { get; set; } = new();
}

public class CircuitBreakerConfig
{
    public int FailureThreshold { get; set; } = 5;
    public int TimeoutSeconds { get; set; } = 60;
}
