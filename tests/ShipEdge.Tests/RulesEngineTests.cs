using CargoShipMonitoring.Shared.Events;
using CargoShipMonitoring.Shared.Models;
using CargoShipMonitoring.ShipEdge.RulesEngine;

namespace CargoShipMonitoring.ShipEdge.Tests;

public class RulesEngineTests
{
    private readonly IRulesEngine _engine;

    public RulesEngineTests()
    {
        _engine = new RulesEngine.RulesEngine();
    }

    [Fact]
    public void Evaluate_GreaterThanRule_Fires_When_Threshold_Exceeded()
    {
        var rule = new Rule
        {
            RuleId = "RULE-001",
            Name = "Engine Overheat",
            SensorType = SensorType.EngineTemperature,
            Operator = RuleOperator.GreaterThan,
            Threshold = 110,
            Priority = Priority.Operational,
            Actions = new List<ActuatorCommand> { ActuatorCommand.EmitAlert }
        };
        _engine.LoadRules(new List<Rule> { rule });

        var reading = new SensorReading
        {
            SensorType = SensorType.EngineTemperature,
            Value = 120,
            Timestamp = DateTimeOffset.UtcNow
        };

        var result = _engine.Evaluate(reading);

        Assert.Single(result);
        Assert.Equal("RULE-001", result[0].RuleId);
        Assert.Equal(ActuatorCommand.EmitAlert, result[0].Actions[0]);
    }

    [Fact]
    public void Evaluate_GreaterThanRule_DoesNotFire_When_Below_Threshold()
    {
        var rule = new Rule
        {
            RuleId = "RULE-001",
            SensorType = SensorType.EngineTemperature,
            Operator = RuleOperator.GreaterThan,
            Threshold = 110
        };
        _engine.LoadRules(new List<Rule> { rule });

        var reading = new SensorReading
        {
            SensorType = SensorType.EngineTemperature,
            Value = 100
        };

        var result = _engine.Evaluate(reading);

        Assert.Empty(result);
    }

    [Fact]
    public void Evaluate_Location_Match_Required()
    {
        var rule = new Rule
        {
            RuleId = "RULE-002",
            SensorType = SensorType.SmokeDetector,
            Location = "HOLD-3",
            Operator = RuleOperator.GreaterThan,
            Threshold = 0.5
        };
        _engine.LoadRules(new List<Rule> { rule });

        var matchingReading = new SensorReading
        {
            SensorType = SensorType.SmokeDetector,
            Location = "HOLD-3",
            Value = 1.0
        };

        var nonMatchingReading = new SensorReading
        {
            SensorType = SensorType.SmokeDetector,
            Location = "HOLD-5",
            Value = 1.0
        };

        Assert.Single(_engine.Evaluate(matchingReading));
        Assert.Empty(_engine.Evaluate(nonMatchingReading));
    }

    [Fact]
    public void Evaluate_Multiple_Rules_Can_Fire_For_Single_Reading()
    {
        var rule1 = new Rule
        {
            RuleId = "RULE-001",
            Name = "Engine Overheat",
            SensorType = SensorType.EngineTemperature,
            Operator = RuleOperator.GreaterThan,
            Threshold = 110,
            Priority = Priority.Operational,
            Actions = new List<ActuatorCommand> { ActuatorCommand.EmitAlert }
        };
        var rule2 = new Rule
        {
            RuleId = "RULE-002",
            Name = "Engine Critical",
            SensorType = SensorType.EngineTemperature,
            Operator = RuleOperator.GreaterThan,
            Threshold = 130,
            Priority = Priority.Critical,
            Actions = new List<ActuatorCommand> { ActuatorCommand.EmitAlert, ActuatorCommand.ReduceEnginePower }
        };
        _engine.LoadRules(new List<Rule> { rule1, rule2 });

        var reading = new SensorReading
        {
            SensorType = SensorType.EngineTemperature,
            Value = 140
        };

        var result = _engine.Evaluate(reading);

        Assert.Equal(2, result.Count);
        Assert.Contains(result, r => r.RuleId == "RULE-001");
        Assert.Contains(result, r => r.RuleId == "RULE-002");
    }

    [Fact]
    public void RuleLoader_Loads_Default_Rules_Correctly()
    {
        var loader = new RuleLoader();
        var rules = loader.LoadDefaultRules();

        Assert.Equal(5, rules.Count);
        Assert.Contains(rules, r => r.RuleId == "RULE-001" && r.Name == "Engine Overheat");
        Assert.Contains(rules, r => r.RuleId == "RULE-002" && r.Name == "Fire Detection" && r.Location == "HOLD-3");
        Assert.Contains(rules, r => r.RuleId == "RULE-003" && r.Name == "Bilge High Water");
        Assert.Contains(rules, r => r.RuleId == "RULE-004" && r.Name == "Reefer Failure");
    }
}
