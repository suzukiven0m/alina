# Cargo Ship Monitoring Platform — Top-Tier Interview Readiness Roadmap

**Current State:** 3/10 — Solid course project with critical bugs and shallow implementation.
**Target State:** 8.5/10 — Production-credible distributed system that impresses FAANG-caliber recruiters.
**Estimated Effort:** 60–80 hours of focused work.
**Target Audience:** Technical recruiters and hiring managers at top-tier tech companies.

---

## Executive Summary

This roadmap transforms a tutorial-level .NET project into a portfolio piece that demonstrates production engineering judgment. The strategy is:

1. **Fix the lies first** — Bugs that make the system fundamentally broken must go.
2. **Make the data flow real** — Events must be traceable from sensor to query.
3. **Add visual proof** — Recruiters are visual; show them a product, not a README.
4. **Inject domain authenticity** — Maritime specifics separate this from generic IoT.
5. **Add production concerns** — Observability, CI/CD, auth, and performance.

Each phase builds on the previous. Do not skip phases.

---

## Phase 1: Critical Bug Fixes (4–6 hours)

**Goal:** Make the core system actually work. These bugs would fail any code review.

### 1.1 Fix SatelliteGateway Missing Return Values

**File:** `src/ShipEdge/SatelliteGateway/SatelliteGateway.cs`
**Problem:** `TransmitAsync` and `TransmitBatchAsync` return `Task<bool>` but never return `true` on success. The method falls through to the end, returning `default(bool)` = `false`. The worker then re-enqueues successfully transmitted events forever.

**Before (lines 29–48):**
```csharp
public async Task<bool> TransmitAsync(ShipEvent evt)
{
    if (!_circuitBreaker.CanExecute())
        return false;

    var httpClient = _httpClientFactory.CreateClient("satellite");
    try
    {
        var json = JsonSerializer.Serialize(evt);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");
        var res = await httpClient.PostAsync($"{_cloudEndpoint}/api/events/{_shipId}", content);
        if (res.IsSuccessStatusCode)
            _circuitBreaker.RecordSuccess();
        _circuitBreaker.RecordFailure();
    }
    catch (HttpRequestException)
    {
        _circuitBreaker.RecordFailure();
    }
    catch (Exception)
    {
        _circuitBreaker.RecordFailure();
    }
}
```

**After:**
```csharp
public async Task<bool> TransmitAsync(ShipEvent evt)
{
    if (!_circuitBreaker.CanExecute())
        return false;

    var httpClient = _httpClientFactory.CreateClient("satellite");
    try
    {
        var json = JsonSerializer.Serialize(evt);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");
        var res = await httpClient.PostAsync($"{_cloudEndpoint}/api/events/{_shipId}", content);
        if (res.IsSuccessStatusCode)
        {
            _circuitBreaker.RecordSuccess();
            return true;
        }
        _circuitBreaker.RecordFailure();
        return false;
    }
    catch (HttpRequestException)
    {
        _circuitBreaker.RecordFailure();
        return false;
    }
    catch (Exception ex) when (ex is not OperationCanceledException)
    {
        _circuitBreaker.RecordFailure();
        return false;
    }
}
```

Apply the same fix to `TransmitBatchAsync` (lines 50–69).

**Tests to update:** `SatelliteGatewayTests.cs` — the existing tests should already pass after this fix, but add a test that asserts `false` is returned on non-success status codes.

### 1.2 Fix OperationCanceledException Swallowing

**File:** `src/ShipEdge/SatelliteGateway/SatelliteGateway.cs`
**Problem:** `catch (Exception)` swallows `OperationCanceledException`, which breaks graceful shutdown when `stoppingToken` is triggered.

**Fix:** Change both catch blocks to:
```csharp
catch (Exception ex) when (ex is not OperationCanceledException)
```

This preserves the cancellation signal while still handling network and serialization errors.

### 1.3 Fix Naming Inconsistencies

**Files:**
- `src/ShipEdge/RulesEngine/RulesEngine.cs` — `str` → `string`, `val` → `value`, `msg` → `message`, `desc` → `description`, `ret` → `return`
- `src/ShipEdge/RulesEngine/RuleLoader.cs` — `str` → `string`, `desc` → `description`
- `src/Shared/Models/Rule.cs` — `desc` → `description`
- `src/Shared/Models/RuleEvaluationResult.cs` — `msg` → `message`

**Why this matters:** Inconsistent naming (`str` vs `string`, `msg` vs `Message`) signals copy-paste coding and lack of attention to detail. Recruiters scan for this.

### 1.4 Dispose IDisposable Resources

**File:** `src/ShipEdge/ShipEdgeWorker.cs`
**Problem:** `SemaphoreSlim _persistLock` is never disposed. `StreamReader` in `FleetCloud/Program.cs` is never disposed.

**Fix:**
```csharp
// In ShipEdgeWorker.cs, add to the class:
public override void Dispose()
{
    _persistLock.Dispose();
    base.Dispose();
}
```

In `FleetCloud/Program.cs`, change:
```csharp
// Before:
var body = await new StreamReader(ctx.Request.Body).ReadToEndAsync();

// After:
using var reader = new StreamReader(ctx.Request.Body);
var body = await reader.ReadToEndAsync();
```

### 1.5 Add Missing LessThan RuleOperator

**File:** `src/Shared/Models/Rule.cs`
**Problem:** `RuleOperator` only has `GreaterThan`, `GreaterThanOrEqual`, `LessThanOrEqual`. Missing `LessThan` and `Equals` is present but `LessThan` is not.

**Fix:**
```csharp
public enum RuleOperator
{
    GreaterThan,
    GreaterThanOrEqual,
    LessThan,
    LessThanOrEqual,
    Equals
}
```

Then update `RulesEngine.EvaluateCondition` to handle it.

---

## Phase 2: Make FleetCloud Actually Process Events (8–12 hours)

**Goal:** The cloud API must do more than log JSON. It must store, query, and surface events.

### 2.1 Create Event Store Schema

**New file:** `src/FleetCloud/EventStore/EventStoreDbContext.cs`

```csharp
using CargoShipMonitoring.Shared.Events;
using Microsoft.EntityFrameworkCore;

namespace CargoShipMonitoring.FleetCloud.EventStore;

public class EventStoreDbContext : DbContext
{
    private readonly string _dbPath;

    public DbSet<StoredEvent> Events => Set<StoredEvent>();

    public EventStoreDbContext(string dbPath)
    {
        _dbPath = dbPath;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseSqlite($"Data Source={_dbPath}");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<StoredEvent>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ShipId);
            entity.HasIndex(e => e.EventType);
            entity.HasIndex(e => e.Timestamp);
            entity.HasIndex(e => e.Priority);
        });
    }
}

public class StoredEvent
{
    public Guid Id { get; set; }
    public string ShipId { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string JsonPayload { get; set; } = string.Empty;
    public DateTimeOffset Timestamp { get; set; }
    public DateTimeOffset ReceivedAt { get; set; }
}
```

### 2.2 Create Event Store Service

**New file:** `src/FleetCloud/EventStore/EventStoreService.cs`

```csharp
using System.Text.Json;
using CargoShipMonitoring.Shared.Events;

namespace CargoShipMonitoring.FleetCloud.EventStore;

public interface IEventStoreService
{
    Task StoreAsync(ShipEvent evt);
    Task StoreBatchAsync(List<ShipEvent> events);
    Task<List<StoredEvent>> GetEventsForShipAsync(string shipId, int limit = 100);
    Task<List<StoredEvent>> GetEventsByPriorityAsync(string priority, int limit = 100);
    Task<List<StoredEvent>> GetEventsInTimeRangeAsync(DateTimeOffset from, DateTimeOffset to, int limit = 1000);
    Task<EventSummary> GetSummaryForShipAsync(string shipId);
}

public class EventStoreService : IEventStoreService
{
    private readonly string _dbPath;

    public EventStoreService(string dbPath)
    {
        _dbPath = dbPath;
        using var db = CreateContext();
        db.Database.EnsureCreated();
    }

    public async Task StoreAsync(ShipEvent evt)
    {
        using var db = CreateContext();
        db.Events.Add(new StoredEvent
        {
            Id = evt.EventId,
            ShipId = evt.ShipId,
            EventType = evt.EventType,
            Priority = evt.Priority.ToString(),
            JsonPayload = JsonSerializer.Serialize(evt, evt.GetType()),
            Timestamp = evt.Timestamp,
            ReceivedAt = DateTimeOffset.UtcNow
        });
        await db.SaveChangesAsync();
    }

    public async Task StoreBatchAsync(List<ShipEvent> events)
    {
        using var db = CreateContext();
        foreach (var evt in events)
        {
            db.Events.Add(new StoredEvent
            {
                Id = evt.EventId,
                ShipId = evt.ShipId,
                EventType = evt.EventType,
                Priority = evt.Priority.ToString(),
                JsonPayload = JsonSerializer.Serialize(evt, evt.GetType()),
                Timestamp = evt.Timestamp,
                ReceivedAt = DateTimeOffset.UtcNow
            });
        }
        await db.SaveChangesAsync();
    }

    public async Task<List<StoredEvent>> GetEventsForShipAsync(string shipId, int limit = 100)
    {
        using var db = CreateContext();
        return await db.Events
            .Where(e => e.ShipId == shipId)
            .OrderByDescending(e => e.Timestamp)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<List<StoredEvent>> GetEventsByPriorityAsync(string priority, int limit = 100)
    {
        using var db = CreateContext();
        return await db.Events
            .Where(e => e.Priority == priority)
            .OrderByDescending(e => e.Timestamp)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<List<StoredEvent>> GetEventsInTimeRangeAsync(DateTimeOffset from, DateTimeOffset to, int limit = 1000)
    {
        using var db = CreateContext();
        return await db.Events
            .Where(e => e.Timestamp >= from && e.Timestamp <= to)
            .OrderByDescending(e => e.Timestamp)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<EventSummary> GetSummaryForShipAsync(string shipId)
    {
        using var db = CreateContext();
        var events = await db.Events.Where(e => e.ShipId == shipId).ToListAsync();
        return new EventSummary
        {
            ShipId = shipId,
            TotalEvents = events.Count,
            CriticalEvents = events.Count(e => e.Priority == "Critical"),
            OperationalEvents = events.Count(e => e.Priority == "Operational"),
            TelemetryEvents = events.Count(e => e.Priority == "Telemetry"),
            LastEventAt = events.MaxBy(e => e.Timestamp)?.Timestamp
        };
    }

    private EventStoreDbContext CreateContext() => new(_dbPath);
}

public class EventSummary
{
    public string ShipId { get; set; } = string.Empty;
    public int TotalEvents { get; set; }
    public int CriticalEvents { get; set; }
    public int OperationalEvents { get; set; }
    public int TelemetryEvents { get; set; }
    public DateTimeOffset? LastEventAt { get; set; }
}
```

### 2.3 Update FleetCloud Program.cs to Use Polymorphic Deserialization

**File:** `src/FleetCloud/Program.cs`

Replace the raw JSON logging endpoints with actual event deserialization:

```csharp
using CargoShipMonitoring.Shared.Events;

// Register the event store
builder.Services.AddSingleton<IEventStoreService>(_ => new EventStoreService("fleet_events.db"));

// Update the event ingestion endpoint
app.MapPost("/api/events/{shipId}", async (string shipId, HttpContext ctx, IEventStoreService eventStore) =>
{
    if (string.IsNullOrWhiteSpace(shipId))
        return Results.BadRequest("shipId is required");

    if (ctx.Request.ContentLength > 1024 * 1024)
        return Results.BadRequest("Event payload exceeds 1MB limit");

    using var reader = new StreamReader(ctx.Request.Body);
    var body = await reader.ReadToEndAsync();

    if (string.IsNullOrWhiteSpace(body))
        return Results.BadRequest("Event body is required");

    ShipEvent? evt;
    try
    {
        evt = JsonSerializer.Deserialize<ShipEvent>(body, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }
    catch (JsonException ex)
    {
        logger.LogWarning(ex, "Failed to deserialize event from {ShipId}", shipId);
        return Results.BadRequest("Invalid JSON or unknown event type");
    }

    if (evt == null)
        return Results.BadRequest("Event could not be deserialized");

    await registry.UpdateLastSeenAsync(shipId);
    await eventStore.StoreAsync(evt);

    logger.LogInformation("[Cloud] Event from {ShipId}: {EventType} ({Priority})",
        shipId, evt.EventType, evt.Priority);

    return Results.Ok();
});

// Update batch endpoint similarly
app.MapPost("/api/events/{shipId}/batch", async (string shipId, HttpContext ctx, IEventStoreService eventStore) =>
{
    // ... validation ...
    using var reader = new StreamReader(ctx.Request.Body);
    var body = await reader.ReadToEndAsync();

    List<ShipEvent>? events;
    try
    {
        events = JsonSerializer.Deserialize<List<ShipEvent>>(body, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }
    catch (JsonException ex)
    {
        logger.LogWarning(ex, "Failed to deserialize batch from {ShipId}", shipId);
        return Results.BadRequest("Invalid JSON array");
    }

    if (events == null || events.Count == 0)
        return Results.BadRequest("Empty batch");

    await registry.UpdateLastSeenAsync(shipId);
    await eventStore.StoreBatchAsync(events);

    logger.LogInformation("[Cloud] Batch of {Count} events from {ShipId}", events.Count, shipId);
    return Results.Ok();
});
```

### 2.4 Add Event Query Endpoints

**File:** `src/FleetCloud/Program.cs`

Add these endpoints after the existing fleet endpoints:

```csharp
// Get recent events for a ship
app.MapGet("/api/events/{shipId}", async (string shipId, IEventStoreService eventStore, int? limit) =>
{
    if (string.IsNullOrWhiteSpace(shipId))
        return Results.BadRequest("shipId is required");

    var events = await eventStore.GetEventsForShipAsync(shipId, limit ?? 100);
    return Results.Ok(events);
});

// Get event summary for a ship
app.MapGet("/api/events/{shipId}/summary", async (string shipId, IEventStoreService eventStore) =>
{
    if (string.IsNullOrWhiteSpace(shipId))
        return Results.BadRequest("shipId is required");

    var summary = await eventStore.GetSummaryForShipAsync(shipId);
    return Results.Ok(summary);
});

// Get critical events across fleet
app.MapGet("/api/events/critical", async (IEventStoreService eventStore, int? limit) =>
{
    var events = await eventStore.GetEventsByPriorityAsync("Critical", limit ?? 50);
    return Results.Ok(events);
});
```

### 2.5 Add Integration Tests for Event Flow

**New file:** `tests/FleetCloud.Tests/EventStoreTests.cs`

```csharp
using CargoShipMonitoring.FleetCloud.EventStore;
using CargoShipMonitoring.Shared.Events;
using CargoShipMonitoring.Shared.Models;

namespace CargoShipMonitoring.FleetCloud.Tests;

public class EventStoreTests : IDisposable
{
    private readonly string _dbPath = $"test_events_{Guid.NewGuid()}.db";
    private readonly EventStoreService _service;

    public EventStoreTests()
    {
        _service = new EventStoreService(_dbPath);
    }

    [Fact]
    public async Task StoreAsync_Persists_SensorReadingEvent()
    {
        var evt = new SensorReadingEvent
        {
            ShipId = "MSC-001",
            Reading = new SensorReading
            {
                SensorId = "TEMP-01",
                SensorType = SensorType.EngineTemperature,
                Value = 120.5,
                Unit = "C"
            }
        };

        await _service.StoreAsync(evt);
        var events = await _service.GetEventsForShipAsync("MSC-001");

        Assert.Single(events);
        Assert.Equal("sensor.reading", events[0].EventType);
    }

    [Fact]
    public async Task StoreBatchAsync_Persists_MultipleEvents()
    {
        var events = new List<ShipEvent>
        {
            new SensorReadingEvent { ShipId = "MSC-001" },
            new AlertTriggeredEvent { ShipId = "MSC-001", Priority = Priority.Critical }
        };

        await _service.StoreBatchAsync(events);
        var stored = await _service.GetEventsForShipAsync("MSC-001");

        Assert.Equal(2, stored.Count);
    }

    [Fact]
    public async Task GetSummary_Returns_AccurateCounts()
    {
        await _service.StoreAsync(new AlertTriggeredEvent
        {
            ShipId = "MSC-001",
            Priority = Priority.Critical
        });
        await _service.StoreAsync(new SensorReadingEvent
        {
            ShipId = "MSC-001",
            Priority = Priority.Telemetry
        });

        var summary = await _service.GetSummaryForShipAsync("MSC-001");

        Assert.Equal(2, summary.TotalEvents);
        Assert.Equal(1, summary.CriticalEvents);
        Assert.Equal(1, summary.TelemetryEvents);
    }

    public void Dispose()
    {
        if (File.Exists(_dbPath))
            File.Delete(_dbPath);
    }
}
```

---

## Phase 3: Build a Live React Dashboard (12–16 hours)

**Goal:** Transform the landing page from a text blog into a live product demo. This is the highest-ROI phase for recruiter impressiveness.

### 3.1 Create Dashboard API Client

**New file:** `docs/landing-page/src/api/fleetApi.ts`

```typescript
const API_BASE = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000';

export interface ShipInfo {
  shipId: string;
  name: string;
  imoNumber?: string;
  status: string;
  lastSeen?: string;
}

export interface EventSummary {
  shipId: string;
  totalEvents: number;
  criticalEvents: number;
  operationalEvents: number;
  telemetryEvents: number;
  lastEventAt?: string;
}

export interface StoredEvent {
  id: string;
  shipId: string;
  eventType: string;
  priority: string;
  timestamp: string;
  jsonPayload: string;
}

export async function getFleet(): Promise<ShipInfo[]> {
  const res = await fetch(`${API_BASE}/api/fleet`);
  if (!res.ok) throw new Error('Failed to fetch fleet');
  return res.json();
}

export async function getShipEvents(shipId: string, limit = 50): Promise<StoredEvent[]> {
  const res = await fetch(`${API_BASE}/api/events/${shipId}?limit=${limit}`);
  if (!res.ok) throw new Error('Failed to fetch events');
  return res.json();
}

export async function getEventSummary(shipId: string): Promise<EventSummary> {
  const res = await fetch(`${API_BASE}/api/events/${shipId}/summary`);
  if (!res.ok) throw new Error('Failed to fetch summary');
  return res.json();
}

export async function getCriticalEvents(limit = 20): Promise<StoredEvent[]> {
  const res = await fetch(`${API_BASE}/api/events/critical?limit=${limit}`);
  if (!res.ok) throw new Error('Failed to fetch critical events');
  return res.json();
}
```

### 3.2 Create Live Fleet Dashboard Component

**New file:** `docs/landing-page/src/sections/LiveDashboard.tsx`

```tsx
import { useEffect, useState } from 'react';
import { getFleet, getEventSummary, type ShipInfo, type EventSummary } from '../api/fleetApi';

interface ShipWithSummary extends ShipInfo {
  summary?: EventSummary;
}

export default function LiveDashboard() {
  const [ships, setShips] = useState<ShipWithSummary[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [lastUpdated, setLastUpdated] = useState<Date>(new Date());

  useEffect(() => {
    async function fetchData() {
      try {
        const fleet = await getFleet();
        const shipsWithSummaries = await Promise.all(
          fleet.map(async (ship) => {
            try {
              const summary = await getEventSummary(ship.shipId);
              return { ...ship, summary };
            } catch {
              return ship;
            }
          })
        );
        setShips(shipsWithSummaries);
        setLastUpdated(new Date());
        setError(null);
      } catch (err) {
        setError(err instanceof Error ? err.message : 'Unknown error');
      } finally {
        setLoading(false);
      }
    }

    fetchData();
    const interval = setInterval(fetchData, 5000);
    return () => clearInterval(interval);
  }, []);

  if (loading) return <div className="text-[var(--text-muted)]">Loading fleet data...</div>;
  if (error) return <div className="text-red-500">Error: {error}</div>;

  const totalEvents = ships.reduce((sum, s) => sum + (s.summary?.totalEvents || 0), 0);
  const totalCritical = ships.reduce((sum, s) => sum + (s.summary?.criticalEvents || 0), 0);

  return (
    <section id="dashboard" className="max-w-3xl mx-auto px-6 py-12 border-t border-[var(--border)]">
      <div className="flex items-center justify-between mb-6">
        <h2 className="text-lg font-semibold">Live Fleet Dashboard</h2>
        <span className="text-xs text-[var(--text-muted)]">
          Updated: {lastUpdated.toLocaleTimeString()}
        </span>
      </div>

      <div className="grid grid-cols-3 gap-4 mb-8">
        <div className="border border-[var(--border)] rounded p-4">
          <div className="text-2xl font-semibold">{ships.length}</div>
          <div className="text-xs text-[var(--text-muted)]">Active Ships</div>
        </div>
        <div className="border border-[var(--border)] rounded p-4">
          <div className="text-2xl font-semibold">{totalEvents}</div>
          <div className="text-xs text-[var(--text-muted)]">Total Events</div>
        </div>
        <div className="border border-[var(--border)] rounded p-4">
          <div className="text-2xl font-semibold text-red-500">{totalCritical}</div>
          <div className="text-xs text-[var(--text-muted)]">Critical Alerts</div>
        </div>
      </div>

      <div className="space-y-4">
        {ships.map((ship) => (
          <div key={ship.shipId} className="border border-[var(--border)] rounded p-4">
            <div className="flex items-center justify-between mb-2">
              <div className="flex items-center gap-2">
                <div className={`w-2 h-2 rounded-full ${
                  ship.status === 'Online' ? 'bg-green-500' : 'bg-red-500'
                }`} />
                <span className="font-medium">{ship.name || ship.shipId}</span>
                <span className="text-xs text-[var(--text-muted)] font-mono">{ship.shipId}</span>
              </div>
              <span className="text-xs text-[var(--text-muted)]">
                {ship.lastSeen ? new Date(ship.lastSeen).toLocaleString() : 'Never'}
              </span>
            </div>
            {ship.summary && (
              <div className="flex gap-4 text-xs">
                <span className="text-[var(--text-secondary)]">
                  {ship.summary.totalEvents} events
                </span>
                <span className="text-red-500">
                  {ship.summary.criticalEvents} critical
                </span>
                <span className="text-yellow-500">
                  {ship.summary.operationalEvents} operational
                </span>
                <span className="text-[var(--text-muted)]">
                  {ship.summary.telemetryEvents} telemetry
                </span>
              </div>
            )}
          </div>
        ))}
      </div>
    </section>
  );
}
```

### 3.3 Add Dashboard to App.tsx

**File:** `docs/landing-page/src/App.tsx`

```tsx
import Header from './sections/Header'
import SystemOverview from './sections/SystemOverview'
import Architecture from './sections/Architecture'
import Challenges from './sections/Challenges'
import TechStack from './sections/TechStack'
import LiveDashboard from './sections/LiveDashboard'
import GettingStarted from './sections/GettingStarted'
import Footer from './sections/Footer'

export default function App() {
  return (
    <div className="min-h-screen">
      <Header />
      <SystemOverview />
      <LiveDashboard />
      <Architecture />
      <Challenges />
      <TechStack />
      <GettingStarted />
      <Footer />
    </div>
  )
}
```

### 3.4 Update Header Navigation

**File:** `docs/landing-page/src/sections/Header.tsx`

Add a dashboard link:
```tsx
<a href="#dashboard">Live Dashboard</a>
```

### 3.5 Add CORS Support to FleetCloud

**File:** `src/FleetCloud/Program.cs`

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("LandingPage", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "https://suzukiven0m.github.io")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Before app.MapControllers() or endpoints:
app.UseCors("LandingPage");
```

### 3.6 Create a Static Demo Mode

When the API is not running, the dashboard should show mock data instead of an error. Add a `useDemoMode` hook:

**New file:** `docs/landing-page/src/hooks/useDemoMode.ts`

```typescript
import { useState, useEffect } from 'react';
import type { ShipInfo, EventSummary, StoredEvent } from '../api/fleetApi';

export function useDemoMode() {
  const [isDemo, setIsDemo] = useState(false);

  useEffect(() => {
    // Check if API is available
    fetch(`${import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000'}/health`,
      { signal: AbortSignal.timeout(2000) }
    ).catch(() => setIsDemo(true));
  }, []);

  return isDemo;
}

export const demoShips: ShipInfo[] = [
  { shipId: 'MSC-001', name: 'MSC Alina', imoNumber: '9876543', status: 'Online', lastSeen: new Date().toISOString() },
  { shipId: 'MSC-002', name: 'MSC Bravo', imoNumber: '9876544', status: 'Online', lastSeen: new Date().toISOString() },
  { shipId: 'MSC-003', name: 'MSC Charlie', imoNumber: '9876545', status: 'Offline', lastSeen: new Date(Date.now() - 3600000).toISOString() },
];

export const demoSummaries: Record<string, EventSummary> = {
  'MSC-001': { shipId: 'MSC-001', totalEvents: 1247, criticalEvents: 3, operationalEvents: 42, telemetryEvents: 1202, lastEventAt: new Date().toISOString() },
  'MSC-002': { shipId: 'MSC-002', totalEvents: 892, criticalEvents: 0, operationalEvents: 18, telemetryEvents: 874, lastEventAt: new Date().toISOString() },
  'MSC-003': { shipId: 'MSC-003', totalEvents: 3456, criticalEvents: 1, operationalEvents: 67, telemetryEvents: 3388, lastEventAt: new Date(Date.now() - 3600000).toISOString() },
};
```

Update `LiveDashboard.tsx` to use demo data when the API is unavailable.

---

## Phase 4: Implement Realistic Sensor Simulation (6–8 hours)

**Goal:** Replace pure random noise with sensor data that behaves like real maritime sensors.

### 4.1 Create Realistic Sensor Models

**New file:** `src/ShipEdge/TelemetryCollector/RealisticSensorReader.cs`

```csharp
using CargoShipMonitoring.Shared.Models;

namespace CargoShipMonitoring.ShipEdge.TelemetryCollector;

public class RealisticSensorReader : ISensorReader
{
    private readonly string _shipId;
    private double _engineTemp = 95.0;
    private double _bilgeLevel = 15.0;
    private double _smokeBaseline = 0.02;
    private double _reeferTemp = -20.0;
    private double _rpm = 800;
    private bool _bilgePumpActive = false;

    public RealisticSensorReader(string shipId)
    {
        _shipId = shipId;
    }

    public List<SensorReading> ReadAllSensors()
    {
        SimulateEngine();
        SimulateBilge();
        SimulateSmoke();
        SimulateReefer();

        return new List<SensorReading>
        {
            new()
            {
                SensorId = "ENGINE-TEMP-01",
                SensorType = SensorType.EngineTemperature,
                Value = Math.Round(_engineTemp, 2),
                Unit = "°C",
                Timestamp = DateTimeOffset.UtcNow
            },
            new()
            {
                SensorId = "BILGE-01",
                SensorType = SensorType.BilgeLevel,
                Value = Math.Round(_bilgeLevel, 2),
                Unit = "%",
                Timestamp = DateTimeOffset.UtcNow
            },
            new()
            {
                SensorId = "SMOKE-HOLD3",
                SensorType = SensorType.SmokeDetector,
                Value = Math.Round(_smokeBaseline, 3),
                Location = "HOLD-3",
                Unit = "ppm",
                Timestamp = DateTimeOffset.UtcNow
            },
            new()
            {
                SensorId = "REEFER-01",
                SensorType = SensorType.ReeferTemperature,
                Value = Math.Round(_reeferTemp, 2),
                Unit = "°C",
                Timestamp = DateTimeOffset.UtcNow
            },
            new()
            {
                SensorId = "ENGINE-RPM-01",
                SensorType = SensorType.EngineRPM,
                Value = Math.Round(_rpm, 0),
                Unit = "RPM",
                Timestamp = DateTimeOffset.UtcNow
            }
        };
    }

    private void SimulateEngine()
    {
        // Engine temp correlates with RPM and slowly trends
        var targetTemp = 85 + (_rpm / 1000.0) * 25;
        var noise = (Random.Shared.NextDouble() - 0.5) * 2;
        _engineTemp += (targetTemp - _engineTemp) * 0.1 + noise;
        _rpm += (Random.Shared.NextDouble() - 0.5) * 20;
        _rpm = Math.Clamp(_rpm, 400, 1200);
    }

    private void SimulateBilge()
    {
        // Bilge slowly fills, pump cycles when threshold reached
        if (_bilgePumpActive)
        {
            _bilgeLevel -= 5;
            if (_bilgeLevel < 10) _bilgePumpActive = false;
        }
        else
        {
            _bilgeLevel += Random.Shared.NextDouble() * 2;
            if (_bilgeLevel > 75) _bilgePumpActive = true;
        }
        _bilgeLevel = Math.Clamp(_bilgeLevel, 0, 100);
    }

    private void SimulateSmoke()
    {
        // Baseline noise with rare spikes (fire simulation)
        _smokeBaseline = 0.02 + (Random.Shared.NextDouble() - 0.5) * 0.01;
        // 1% chance of fire spike per reading
        if (Random.Shared.NextDouble() < 0.01)
            _smokeBaseline += Random.Shared.NextDouble() * 2;
    }

    private void SimulateReefer()
    {
        // Reefer cycles on/off around setpoint
        var setpoint = -20;
        var drift = (Random.Shared.NextDouble() - 0.5) * 0.5;
        _reeferTemp += (setpoint - _reeferTemp) * 0.05 + drift;
    }

    public SensorReading TriggerEmergency(string sensorId, SensorType type, double value)
    {
        return new SensorReading
        {
            SensorId = sensorId,
            SensorType = type,
            Value = value,
            Unit = type switch
            {
                SensorType.EngineTemperature => "°C",
                SensorType.BilgeLevel => "%",
                SensorType.SmokeDetector => "ppm",
                SensorType.ReeferTemperature => "°C",
                SensorType.EngineRPM => "RPM",
                _ => ""
            },
            Timestamp = DateTimeOffset.UtcNow
        };
    }
}
```

### 4.2 Add EngineRPM to SensorType Enum

**File:** `src/Shared/Models/SensorType.cs` (or wherever the enum is defined)

```csharp
public enum SensorType
{
    EngineTemperature,
    BilgeLevel,
    SmokeDetector,
    ReeferTemperature,
    EngineRPM
}
```

### 4.3 Wire Up Realistic Reader in DI

**File:** `src/ShipEdge/Program.cs`

```csharp
builder.Services.AddSingleton<ISensorReader>(sp =>
    new RealisticSensorReader(sp.GetRequiredService<IOptions<ShipEdgeConfig>>().Value.ShipId));
```

---

## Phase 5: Command Retry Scheduler & Actuator Abstraction (4–6 hours)

**Goal:** Finish the half-built command retry system and replace hardcoded strings.

### 5.1 Create ActuatorCommand Enum

**New file:** `src/Shared/Models/ActuatorCommand.cs`

```csharp
namespace CargoShipMonitoring.Shared.Models;

public enum ActuatorCommand
{
    EmitAlert,
    TriggerFireSuppression,
    StartBilgePump,
    ReduceEnginePower,
    ShutdownReefer
}
```

### 5.2 Update Rule Model to Use Enum

**File:** `src/Shared/Models/Rule.cs`

```csharp
public record Rule
{
    public string RuleId { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public SensorType SensorType { get; init; }
    public string? Location { get; init; }
    public RuleOperator Operator { get; init; }
    public double Threshold { get; init; }
    public Priority Priority { get; init; }
    public List<ActuatorCommand> Actions { get; init; } = new();
}
```

### 5.3 Update RuleLoader

**File:** `src/ShipEdge/RulesEngine/RuleLoader.cs`

```csharp
public IReadOnlyList<Rule> LoadDefaultRules()
{
    return new List<Rule>
    {
        new()
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
        // ... update other rules similarly
    };
}
```

### 5.4 Update ShipEdgeWorker to Use Enum

**File:** `src/ShipEdge/ShipEdgeWorker.cs`

```csharp
private void ExecuteActuator(ActuatorCommand command, SensorReading reading)
{
    switch (command)
    {
        case ActuatorCommand.EmitAlert:
            _logger.LogWarning("[{ShipId}] ALERT triggered for {SensorType}", _shipId, reading.SensorType);
            break;
        case ActuatorCommand.TriggerFireSuppression:
            _logger.LogCritical("[{ShipId}] ACTUATOR: Fire suppression activated for {Location}", _shipId, reading.Location);
            break;
        case ActuatorCommand.StartBilgePump:
            _logger.LogCritical("[{ShipId}] ACTUATOR: Bilge pump started", _shipId);
            break;
        case ActuatorCommand.ReduceEnginePower:
            _logger.LogWarning("[{ShipId}] ACTUATOR: Engine power reduced", _shipId);
            break;
        case ActuatorCommand.ShutdownReefer:
            _logger.LogCritical("[{ShipId}] ACTUATOR: Reefer emergency shutdown", _shipId);
            break;
    }
}
```

### 5.5 Create Command Retry Background Service

**New file:** `src/FleetCloud/CommandService/CommandRetryWorker.cs`

```csharp
namespace CargoShipMonitoring.FleetCloud.CommandService;

public class CommandRetryWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CommandRetryWorker> _logger;

    public CommandRetryWorker(IServiceProvider serviceProvider, ILogger<CommandRetryWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(30));

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var commandService = scope.ServiceProvider.GetRequiredService<ICommandService>();
                var pendingCommands = await commandService.GetCommandsReadyForRetryAsync();

                foreach (var cmd in pendingCommands)
                {
                    _logger.LogInformation("Retrying command {CommandId} for ship {ShipId}",
                        cmd.CommandId, cmd.ShipId);
                    // In a real system, this would re-issue the command via a message broker
                    // For now, we just log and reset the retry timer
                    await commandService.ResetRetryTimerAsync(cmd.CommandId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in command retry worker");
            }
        }
    }
}
```

### 5.6 Add Retry Methods to CommandService

**File:** `src/FleetCloud/CommandService/CommandService.cs`

```csharp
public async Task<List<PendingCommand>> GetCommandsReadyForRetryAsync()
{
    using var db = new CommandDbContext(_dbPath);
    var now = DateTimeOffset.UtcNow;
    return await db.Commands
        .Where(c => c.Status == CommandStatus.Issued && c.NextRetryAt <= now)
        .OrderBy(c => c.NextRetryAt)
        .ToListAsync();
}

public async Task ResetRetryTimerAsync(Guid commandId)
{
    using var db = new CommandDbContext(_dbPath);
    var cmd = await db.Commands.FindAsync(commandId);
    if (cmd == null) return;

    var backoffSeconds = Math.Min(300, Math.Pow(2, cmd.RetryCount) * 5);
    cmd.NextRetryAt = DateTimeOffset.UtcNow.AddSeconds(backoffSeconds);
    await db.SaveChangesAsync();
}
```

---

## Phase 6: Domain Depth & Maritime Authenticity (8–10 hours)

**Goal:** Add maritime-specific details that separate this from generic IoT.

### 6.1 Add IMO Number Validation

**New file:** `src/Shared/Models/IMONumber.cs`

```csharp
using System.Text.RegularExpressions;

namespace CargoShipMonitoring.Shared.Models;

public readonly record struct IMONumber
{
    private readonly string _value;

    public IMONumber(string value)
    {
        if (!IsValid(value))
            throw new ArgumentException($"Invalid IMO number: {value}", nameof(value));
        _value = value;
    }

    public override string ToString() => _value;

    public static bool IsValid(string? imo)
    {
        if (string.IsNullOrWhiteSpace(imo)) return false;
        if (!Regex.IsMatch(imo, @"^\d{7}$")) return false;

        // IMO checksum algorithm
        int sum = 0;
        for (int i = 0; i < 6; i++)
        {
            sum += (imo[i] - '0') * (7 - i);
        }
        int checkDigit = sum % 10;
        return checkDigit == (imo[6] - '0');
    }
}
```

### 6.2 Add Voyage Model

**New file:** `src/Shared/Models/Voyage.cs`

```csharp
namespace CargoShipMonitoring.Shared.Models;

public record Voyage
{
    public Guid VoyageId { get; init; } = Guid.NewGuid();
    public string ShipId { get; init; } = string.Empty;
    public string DeparturePort { get; init; } = string.Empty;
    public string DestinationPort { get; init; } = string.Empty;
    public DateTimeOffset ETD { get; init; }
    public DateTimeOffset ETA { get; init; }
    public VoyageStatus Status { get; init; } = VoyageStatus.Planned;
    public List<CargoItem> CargoManifest { get; init; } = new();
    public double? CurrentLatitude { get; init; }
    public double? CurrentLongitude { get; init; }
    public double? CurrentSpeed { get; init; } // knots
    public double? CurrentHeading { get; init; } // degrees
}

public record CargoItem
{
    public string ContainerId { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string CargoType { get; init; } = string.Empty; // "Pharmaceuticals", "Electronics", "Food"
    public double WeightKg { get; init; }
    public bool RequiresReefer { get; init; }
    public double? MinTemperature { get; init; }
    public double? MaxTemperature { get; init; }
}

public enum VoyageStatus
{
    Planned,
    InProgress,
    Completed,
    Cancelled,
    Distressed
}
```

### 6.3 Add GPS Position to ShipInfo

**File:** `src/Shared/Models/ShipInfo.cs` (update or create)

```csharp
public record ShipInfo
{
    public string ShipId { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? IMONumber { get; init; }
    public string Status { get; init; } = "Offline";
    public DateTimeOffset? LastSeen { get; init; }
    public double? Latitude { get; init; }
    public double? Longitude { get; init; }
    public double? Speed { get; init; }
    public string? CurrentVoyageId { get; init; }
}
```

### 6.4 Add Geo-Fencing Rule

**File:** `src/ShipEdge/RulesEngine/RuleLoader.cs`

Add a new rule that triggers when a ship leaves its planned corridor:

```csharp
new()
{
    RuleId = "RULE-005",
    Name = "Off Course Alert",
    Description = "Ship has deviated from planned route corridor",
    SensorType = SensorType.GPSPosition,
    Operator = RuleOperator.GreaterThan,
    Threshold = 10, // nautical miles from planned route
    Priority = Priority.Operational,
    Actions = new List<ActuatorCommand> { ActuatorCommand.EmitAlert }
}
```

---

## Phase 7: Observability & Production Concerns (8–10 hours)

**Goal:** Add the tooling and practices that separate hobby projects from production systems.

### 7.1 Add OpenTelemetry Tracing

**Add NuGet packages:**
```bash
dotnet add src/Shared package OpenTelemetry
dotnet add src/Shared package OpenTelemetry.Instrumentation.Http
dotnet add src/Shared package OpenTelemetry.Exporter.Jaeger
dotnet add src/ShipEdge package OpenTelemetry.Extensions.Hosting
dotnet add src/FleetCloud package OpenTelemetry.Extensions.Hosting
```

**New file:** `src/Shared/Telemetry/TelemetryConfig.cs`

```csharp
using System.Diagnostics;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace CargoShipMonitoring.Shared.Telemetry;

public static class TelemetryConfig
{
    public static readonly ActivitySource Source = new("CargoShipMonitoring");

    public static IServiceCollection AddCargoShipTelemetry(this IServiceCollection services, string serviceName)
    {
        services.AddOpenTelemetry()
            .WithTracing(builder =>
            {
                builder
                    .AddSource("CargoShipMonitoring")
                    .SetResourceBuilder(ResourceBuilder.CreateDefault()
                        .AddService(serviceName, serviceVersion: "1.0.0"))
                    .AddHttpClientInstrumentation()
                    .AddAspNetCoreInstrumentation()
                    .AddJaegerExporter(options =>
                    {
                        options.AgentHost = Environment.GetEnvironmentVariable("JAEGER_HOST") ?? "localhost";
                    });
            });

        return services;
    }
}
```

### 7.2 Instrument ShipEdgeWorker

**File:** `src/ShipEdge/ShipEdgeWorker.cs`

```csharp
using CargoShipMonitoring.Shared.Telemetry;
using System.Diagnostics;

// In CollectTelemetryAsync:
using var activity = TelemetryConfig.Source.StartActivity("CollectTelemetry", ActivityKind.Internal);
activity?.SetTag("ship.id", _shipId);

// In ProcessQueueAsync, before transmission:
using var transmitActivity = TelemetryConfig.Source.StartActivity("TransmitEvents", ActivityKind.Client);
transmitActivity?.SetTag("ship.id", _shipId);
transmitActivity?.SetTag("event.count", stagedEvents.Count + criticalEvents.Count);

// Propagate trace context in SatelliteGateway
// Add traceparent header to HTTP requests
```

### 7.3 Add Prometheus Metrics

**Add NuGet package:**
```bash
dotnet add src/ShipEdge package Prometheus.Client
```

**In ShipEdgeWorker.cs:**
```csharp
private static readonly Counter EventsQueued = Metrics.CreateCounter(
    "shipedge_events_queued_total",
    "Total events queued",
    "priority");

private static readonly Counter EventsTransmitted = Metrics.CreateCounter(
    "shipedge_events_transmitted_total",
    "Total events successfully transmitted",
    "priority");

private static readonly Histogram TransmitDuration = Metrics.CreateHistogram(
    "shipedge_transmit_duration_seconds",
    "Event transmission duration",
    new HistogramConfiguration { Buckets = Histogram.ExponentialBuckets(0.001, 2, 10) });
```

### 7.4 Add GitHub Actions CI for .NET

**New file:** `.github/workflows/ci.yml`

```yaml
name: .NET CI

on:
  push:
    branches: [master]
  pull_request:
    branches: [master]

jobs:
  build-and-test:
    runs-on: ubuntu-latest

    steps:
      - uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'

      - name: Restore
        run: dotnet restore

      - name: Build
        run: dotnet build --no-restore --configuration Release

      - name: Test
        run: dotnet test --no-build --verbosity normal

      - name: Docker Build FleetCloud
        run: docker build -f src/FleetCloud/Dockerfile -t cargoship-fleetcloud:latest .

      - name: Docker Build ShipEdge
        run: docker build -f src/ShipEdge/Dockerfile -t cargoship-shipedge:latest .
```

### 7.5 Add Custom Health Checks

**File:** `src/FleetCloud/Program.cs`

```csharp
builder.Services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>("database")
    .AddCheck<EventStoreHealthCheck>("eventstore");

// Implement the health check classes
public class DatabaseHealthCheck : IHealthCheck
{
    private readonly IShipRegistryService _registry;

    public DatabaseHealthCheck(IShipRegistryService registry)
    {
        _registry = registry;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _registry.GetAllAsync();
            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Database connectivity failed", ex);
        }
    }
}
```

---

## Phase 8: Documentation & Polish (4–6 hours)

### 8.1 Update README with Architecture Diagram

Replace the ASCII diagram with a Mermaid diagram:

```markdown
```mermaid
graph LR
    S1[Sensor Array<br/>Engine, Bilge, Smoke, Reefer] -->|Readings| SE1[ShipEdge Worker<br/>Rules Engine + Priority Queue]
    S2[Sensor Array] -->|Readings| SE2[ShipEdge Worker]
    SE1 -->|Critical Events| SG[Satellite Gateway<br/>Circuit Breaker]
    SE1 -->|Batch Telemetry| SG
    SE2 -->|Critical Events| SG
    SG -->|HTTP| FC[FleetCloud API<br/>Event Store + Ship Registry]
    FC -->|Commands| SE1
    FC -->|Commands| SE2
    FC -->|Query| DASH[React Dashboard]
```
```

### 8.2 Add Screenshots Section

Add a `docs/screenshots/` directory with:
- `docker-compose-up.png` — Terminal showing services starting
- `api-response.png` — Swagger UI or curl showing event ingestion
- `dashboard.png` — The React dashboard with live data
- `sqlite-events.png` — SQLite browser showing stored events
- `jaeger-trace.png` — Jaeger UI showing distributed trace

### 8.3 Add Performance Benchmarks Section

Document load test results:

```markdown
## Performance Benchmarks

Tested on AMD Ryzen 5 5600X, 32GB RAM, SSD:

| Metric | Value |
|--------|-------|
| Event ingestion throughput | 2,400 events/second |
| Priority queue enqueue (50K items) | 145ms |
| SQLite persistence cycle | 12ms |
| API response time (p99) | 8ms |
| Circuit breaker recovery | 60s + 1 probe |
```

---

## File-by-File Checklist

Use this checklist to track progress. Check each box as you complete it.

### Critical Fixes
- [ ] Fix `return true` in `SatelliteGateway.TransmitAsync`
- [ ] Fix `return true` in `SatelliteGateway.TransmitBatchAsync`
- [ ] Fix `OperationCanceledException` swallowing
- [ ] Fix naming: `str` → `string`, `msg` → `message`, etc.
- [ ] Dispose `SemaphoreSlim` in `ShipEdgeWorker`
- [ ] Dispose `StreamReader` in `FleetCloud/Program.cs`
- [ ] Add `LessThan` to `RuleOperator`

### FleetCloud Event Processing
- [ ] Create `EventStoreDbContext.cs`
- [ ] Create `EventStoreService.cs`
- [ ] Update `/api/events/{shipId}` to deserialize polymorphic events
- [ ] Update `/api/events/{shipId}/batch` to deserialize polymorphic events
- [ ] Add `/api/events/{shipId}` GET endpoint
- [ ] Add `/api/events/{shipId}/summary` GET endpoint
- [ ] Add `/api/events/critical` GET endpoint
- [ ] Add `EventStoreTests.cs`

### Dashboard
- [ ] Create `fleetApi.ts`
- [ ] Create `LiveDashboard.tsx`
- [ ] Add dashboard to `App.tsx`
- [ ] Add dashboard link to `Header.tsx`
- [ ] Add CORS to FleetCloud
- [ ] Create `useDemoMode.ts`

### Realistic Simulation
- [ ] Create `RealisticSensorReader.cs`
- [ ] Add `EngineRPM` to `SensorType`
- [ ] Wire `RealisticSensorReader` in DI

### Command Retry
- [ ] Create `ActuatorCommand` enum
- [ ] Update `Rule` to use `ActuatorCommand`
- [ ] Update `RuleLoader` to use enum
- [ ] Update `ShipEdgeWorker` to use enum
- [ ] Create `CommandRetryWorker.cs`
- [ ] Add retry methods to `CommandService`

### Domain Depth
- [ ] Create `IMONumber.cs` with validation
- [ ] Create `Voyage.cs` and `CargoItem.cs`
- [ ] Update `ShipInfo` with GPS fields
- [ ] Add off-course rule to `RuleLoader`

### Observability
- [ ] Add OpenTelemetry NuGet packages
- [ ] Create `TelemetryConfig.cs`
- [ ] Instrument `ShipEdgeWorker` with activities
- [ ] Add Prometheus metrics
- [ ] Add Jaeger to Docker Compose
- [ ] Create `.github/workflows/ci.yml`
- [ ] Add custom health checks

### Polish
- [ ] Add Mermaid architecture diagram to README
- [ ] Add screenshots to `docs/screenshots/`
- [ ] Add performance benchmarks section
- [ ] Update landing page with system screenshots

---

## Timeline

| Phase | Hours | Cumulative | Deliverable |
|-------|-------|------------|-------------|
| 1: Critical Fixes | 4–6 | 6 | All tests pass, no swallowed exceptions |
| 2: Event Processing | 8–12 | 18 | Queryable event store, working data flow |
| 3: Dashboard | 12–16 | 34 | Live React dashboard with real data |
| 4: Realistic Simulation | 6–8 | 42 | Correlated sensor data with trends |
| 5: Command Retry | 4–6 | 48 | Working retry scheduler, type-safe actuators |
| 6: Domain Depth | 8–10 | 58 | IMO validation, voyages, cargo manifests |
| 7: Observability | 8–10 | 68 | Traces, metrics, CI/CD, health checks |
| 8: Documentation | 4–6 | 74 | Screenshots, diagrams, benchmarks |

**Buffer:** 6 hours for debugging, refactoring, and polish.

**Total: 60–80 hours.**

---

## Success Criteria

Before calling this "done," verify:

1. **All tests pass:** `dotnet test` exits with 0 failures.
2. **Dashboard loads:** The landing page shows live (or demo) fleet data.
3. **Events are traceable:** You can follow a sensor reading from `ShipEdge` → `SatelliteGateway` → `FleetCloud` → `EventStore` → API query.
4. **No critical bugs:** Run the system for 1 hour. No unhandled exceptions. No memory leaks.
5. **CI is green:** Every push to `master` triggers build, test, and Docker image creation.
6. **Recruiter test:** Show this to a non-technical friend. Can they understand what it does in 30 seconds? If not, the landing page needs work.

---

## What "Top-Tier Interview Ready" Looks Like

A recruiter at Stripe, Shopify, or a FAANG company should be able to:

1. Visit your GitHub profile and see a clean repo with a green CI badge.
2. Click the deployed landing page and see a live dashboard with updating data.
3. Read the README and understand the architecture in 2 minutes.
4. Look at the code and see consistent naming, proper error handling, and production patterns.
5. Ask "How do you handle backpressure?" and you point to the bounded priority queue with telemetry eviction.
6. Ask "How do you debug issues in production?" and you show them Jaeger traces and Prometheus dashboards.
7. Ask "What maritime regulations does this comply with?" and you mention SOLAS, IMO, and LRIT.

This roadmap gets you there. Start with Phase 1. Do not skip phases.
