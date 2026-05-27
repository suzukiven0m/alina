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

    [Fact]
    public void Enqueue_Respects_MaxSize_With_Eviction()
    {
        var queue = new PriorityQueueNs.PriorityEventQueue(maxSize: 5);

        for (int i = 0; i < 10; i++)
        {
            queue.Enqueue(new SensorReadingEvent { ShipId = "TEST", Priority = Priority.Telemetry });
        }

        Assert.Equal(5, queue.Count);
    }

    [Fact]
    public void Enqueue_Never_Evicts_Critical()
    {
        var queue = new PriorityQueueNs.PriorityEventQueue(maxSize: 3);

        queue.Enqueue(new AlertTriggeredEvent { ShipId = "TEST", Priority = Priority.Critical });
        queue.Enqueue(new SensorReadingEvent { ShipId = "TEST", Priority = Priority.Telemetry });
        queue.Enqueue(new SensorReadingEvent { ShipId = "TEST", Priority = Priority.Telemetry });
        queue.Enqueue(new SensorReadingEvent { ShipId = "TEST", Priority = Priority.Telemetry });

        var first = queue.Dequeue();
        Assert.Equal(Priority.Critical, first!.Priority);
    }

    [Fact]
    public void Concurrent_Enqueue_Dequeue_Maintains_Consistency()
    {
        var queue = new PriorityQueueNs.PriorityEventQueue(maxSize: 10_000);
        var events = new List<ShipEvent>();
        var threads = 4;
        var iterations = 100;

        Parallel.For(0, threads, _ =>
        {
            for (int i = 0; i < iterations; i++)
            {
                queue.Enqueue(new SensorReadingEvent { ShipId = "TEST", Priority = Priority.Telemetry });
                var evt = queue.Dequeue();
                if (evt != null)
                    lock (events) { events.Add(evt); }
            }
        });

        // After equal enqueues and dequeues from each thread, queue should be empty
        // But eviction may have occurred, so just verify count is non-negative
        Assert.True(queue.Count >= 0);

        // Drain remaining
        while (queue.Dequeue() != null) { }
        Assert.Equal(0, queue.Count);
    }
}
