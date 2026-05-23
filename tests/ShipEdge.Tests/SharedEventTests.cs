using CargoShipMonitoring.Shared.Events;
using CargoShipMonitoring.Shared.Models;

namespace CargoShipMonitoring.ShipEdge.Tests;

public class SharedEventTests
{
    [Fact]
    public void ShipEvent_Created_WithRequiredFields()
    {
        var evt = new SensorReadingEvent
        {
            EventId = Guid.NewGuid(),
            ShipId = "MSC-001",
            Timestamp = DateTimeOffset.UtcNow
        };

        Assert.NotEqual(Guid.Empty, evt.EventId);
        Assert.Equal("MSC-001", evt.ShipId);
        Assert.Equal("sensor.reading", evt.EventType);
        Assert.Equal(Priority.Telemetry, evt.Priority);
    }

    [Fact]
    public void Priority_Order_Is_Critical_Operational_Telemetry()
    {
        Assert.True(Priority.Critical > Priority.Operational);
        Assert.True(Priority.Operational > Priority.Telemetry);
    }
}
