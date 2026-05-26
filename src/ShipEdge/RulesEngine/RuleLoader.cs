using CargoShipMonitoring.Shared.Models;

namespace CargoShipMonitoring.ShipEdge.RulesEngine;

public class RuleLoader : IRuleLoader
{
    public IReadOnlyList<Rule> LoadDefaultRules()
    {
        return new List<Rule>
        {
            new Rule
            {
                RuleId = "RULE-001",
                Name = "Engine Overheat",
                Description = "Engine temperature exceeds safe operating range",
                SensorType = SensorType.EngineTemperature,
                Operator = RuleOperator.GreaterThan,
                Threshold = 110,
                Priority = Priority.Operational,
                Actions = new List<ActuatorCommand> { ActuatorCommand.EmitAlert, ActuatorCommand.ReduceEnginePower }
            },
            new Rule
            {
                RuleId = "RULE-002",
                Name = "Fire Detection",
                Description = "Smoke detected in cargo hold",
                SensorType = SensorType.SmokeDetector,
                Location = "HOLD-3",
                Operator = RuleOperator.GreaterThan,
                Threshold = 0.5,
                Priority = Priority.Critical,
                Actions = new List<ActuatorCommand> { ActuatorCommand.TriggerFireSuppression, ActuatorCommand.EmitAlert }
            },
            new Rule
            {
                RuleId = "RULE-003",
                Name = "Bilge High Water",
                Description = "Bilge water level critically high",
                SensorType = SensorType.BilgeLevel,
                Operator = RuleOperator.GreaterThan,
                Threshold = 80,
                Priority = Priority.Critical,
                Actions = new List<ActuatorCommand> { ActuatorCommand.StartBilgePump, ActuatorCommand.EmitAlert }
            },
            new Rule
            {
                RuleId = "RULE-004",
                Name = "Reefer Failure",
                Description = "Reefer container temperature too high for pharmaceuticals",
                SensorType = SensorType.ReeferTemperature,
                Operator = RuleOperator.GreaterThan,
                Threshold = -15,
                Priority = Priority.Critical,
                Actions = new List<ActuatorCommand> { ActuatorCommand.ShutdownReefer, ActuatorCommand.EmitAlert }
            }
        };
    }
}
