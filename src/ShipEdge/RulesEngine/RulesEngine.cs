using CargoShipMonitoring.Shared.Models;
using System.Collections.Immutable;

namespace CargoShipMonitoring.ShipEdge.RulesEngine;

public class RulesEngine : IRulesEngine
{
    private ImmutableList<Rule> _rules = ImmutableList<Rule>.Empty;

    public void LoadRules(IReadOnlyList<Rule> rules)
    {
        _rules = ImmutableList.CreateRange(rules);
    }

    public List<RuleEvaluationResult> Evaluate(SensorReading reading)
    {
        var results = new List<RuleEvaluationResult>();
        var rulesSnapshot = _rules;

        foreach (var rule in rulesSnapshot)
        {
            if (rule.SensorType != reading.SensorType)
                continue;

            if (!string.IsNullOrEmpty(rule.Location) && rule.Location != reading.Location)
                continue;

            if (EvaluateCondition(reading.Value, rule.Operator, rule.Threshold))
            {
                results.Add(new RuleEvaluationResult
                {
                    RuleId = rule.RuleId,
                    RuleName = rule.Name,
                    Triggered = true,
                    Message = rule.Description,
                    Priority = rule.Priority,
                    Reading = reading,
                    Actions = rule.Actions
                });
            }
        }

        return results;
    }

    private static bool EvaluateCondition(double value, RuleOperator op, double threshold)
    {
        return op switch
        {
            RuleOperator.GreaterThan => value > threshold,
            RuleOperator.LessThan => value < threshold,
            RuleOperator.Equals => Math.Abs(value - threshold) < 0.001,
            RuleOperator.GreaterThanOrEqual => value >= threshold,
            RuleOperator.LessThanOrEqual => value <= threshold,
            _ => false
        };
    }
}
