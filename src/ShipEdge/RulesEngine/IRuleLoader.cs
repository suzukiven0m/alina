using CargoShipMonitoring.Shared.Models;

namespace CargoShipMonitoring.ShipEdge.RulesEngine;

public interface IRuleLoader
{
    IReadOnlyList<Rule> LoadDefaultRules();
}
