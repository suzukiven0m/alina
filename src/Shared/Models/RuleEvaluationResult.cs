namespace CargoShipMonitoring.Shared.Models;

public record RuleEvaluationResult
{
    public string RuleId { get; init; } = string.Empty;
    public string RuleName { get; init; } = string.Empty;
    public bool Triggered { get; init; }
    public string Message { get; init; } = string.Empty;
    public Priority Priority { get; init; }
    public SensorReading Reading { get; init; } = new();
    public List<string> Actions { get; init; } = new();
}
