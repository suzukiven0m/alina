using CargoShipMonitoring.Shared.Events;

namespace CargoShipMonitoring.ShipEdge.PriorityEventQueue;

public interface IPriorityEventQueue
{
    int Count { get; }
    int MaxSize { get; }
    void Enqueue(ShipEvent evt);
    ShipEvent? Dequeue();
    Task PersistAsync();
    Task RestoreAsync();
}
