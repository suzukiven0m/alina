using CargoShipMonitoring.Shared.Models;

namespace CargoShipMonitoring.ShipEdge.RulesEngine;

public interface IRulesEngine
{
    void LoadRules(IReadOnlyList<Rule> rules);
    List<RuleEvaluationResult> Evaluate(SensorReading reading);
}
