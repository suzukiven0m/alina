namespace CargoShipMonitoring.Shared.Models;

public record Rule
{
    public string RuleId { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public SensorType SensorType { get; init; }
    public string? Location { get; init; }
    public RuleOperator Operator { get; init; }
    public double Threshold { get; init; }
    public Priority Priority { get; init; }
    public List<ActuatorCommand> Actions { get; init; } = new();
}

public enum RuleOperator
{
    GreaterThan,
    LessThan,
    Equals,
    GreaterThanOrEqual,
    LessThanOrEqual
}
