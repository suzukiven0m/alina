using CargoShipMonitoring.Shared.Events;
using CargoShipMonitoring.Shared.Models;
using PriorityQueueNs = CargoShipMonitoring.ShipEdge.PriorityEventQueue;

namespace CargoShipMonitoring.ShipEdge.Tests;

public class PriorityEventQueueTests
{
    [Fact]
    public void Dequeue_Returns_Critical_Before_Operational()
    {
        var queue = new PriorityQueueNs.PriorityEventQueue();
        var telemetry = new SensorReadingEvent { ShipId = "TEST", Priority = Priority.Telemetry };
        var operational = new AlertTriggeredEvent { ShipId = "TEST", Priority = Priority.Operational };
        var critical = new AlertTriggeredEvent { ShipId = "TEST", Priority = Priority.Critical };

        queue.Enqueue(telemetry);
        queue.Enqueue(operational);
        queue.Enqueue(critical);

        var first = queue.Dequeue();
        var second = queue.Dequeue();
        var third = queue.Dequeue();

        Assert.Equal(Priority.Critical, first!.Priority);
        Assert.Equal(Priority.Operational, second!.Priority);
        Assert.Equal(Priority.Telemetry, third!.Priority);
    }

    [Fact]
    public void Dequeue_Returns_Null_When_Empty()
    {
        var queue = new PriorityQueueNs.PriorityEventQueue();
        Assert.Null(queue.Dequeue());
    }

    [Fact]
    public void Count_Returns_Number_Of_Events()
    {
        var queue = new PriorityQueueNs.PriorityEventQueue();
        Assert.Equal(0, queue.Count);

        queue.Enqueue(new SensorReadingEvent { ShipId = "TEST" });
        Assert.Equal(1, queue.Count);
    }
}
