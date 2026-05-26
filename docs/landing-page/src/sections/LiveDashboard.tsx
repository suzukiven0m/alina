import { useEffect, useState } from 'react';
import { getFleet, getEventSummary, getCriticalEvents, type ShipInfo, type EventSummary, type StoredEvent } from '../api/fleetApi';
import { useDemoMode, demoShips, demoSummaries, demoCriticalEvents } from '../hooks/useDemoMode';

interface ShipWithSummary extends ShipInfo {
  summary?: EventSummary;
}

export default function LiveDashboard() {
  const isDemo = useDemoMode();
  const [ships, setShips] = useState<ShipWithSummary[]>([]);
  const [criticalEvents, setCriticalEvents] = useState<StoredEvent[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [lastUpdated, setLastUpdated] = useState<Date>(new Date());

  useEffect(() => {
    async function fetchData() {
      try {
        if (isDemo) {
          const shipsWithSummaries = demoShips.map((ship) => ({
            ...ship,
            summary: demoSummaries[ship.shipId],
          }));
          setShips(shipsWithSummaries);
          setCriticalEvents(demoCriticalEvents);
          setLastUpdated(new Date());
          setError(null);
          setLoading(false);
          return;
        }

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

        try {
          const critical = await getCriticalEvents(5);
          setCriticalEvents(critical);
        } catch {
          setCriticalEvents([]);
        }

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
  }, [isDemo]);

  if (loading) return <div className="text-[var(--text-muted)]">Loading fleet data...</div>;
  if (error) return <div className="text-red-500">Error: {error}</div>;

  const totalEvents = ships.reduce((sum, s) => sum + (s.summary?.totalEvents || 0), 0);
  const totalCritical = ships.reduce((sum, s) => sum + (s.summary?.criticalEvents || 0), 0);

  return (
    <section id="dashboard" className="max-w-3xl mx-auto px-6 py-12 border-t border-[var(--border)]">
      <div className="flex items-center justify-between mb-6">
        <div className="flex items-center gap-3">
          <h2 className="text-lg font-semibold">Live Fleet Dashboard</h2>
          {isDemo && (
            <span className="text-xs px-2 py-0.5 rounded-full bg-[var(--bg-tertiary)] text-[var(--text-muted)]">
              Demo Mode
            </span>
          )}
        </div>
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
          <div className="text-2xl font-semibold">{totalEvents.toLocaleString()}</div>
          <div className="text-xs text-[var(--text-muted)]">Total Events</div>
        </div>
        <div className="border border-[var(--border)] rounded p-4">
          <div className="text-2xl font-semibold text-red-500">{totalCritical}</div>
          <div className="text-xs text-[var(--text-muted)]">Critical Alerts</div>
        </div>
      </div>

      {criticalEvents.length > 0 && (
        <div className="mb-8">
          <h3 className="text-sm font-medium text-red-500 mb-3">Recent Critical Events</h3>
          <div className="space-y-2">
            {criticalEvents.slice(0, 3).map((evt) => (
              <div key={evt.id} className="border border-red-900/30 rounded p-3 bg-red-500/5">
                <div className="flex items-center justify-between">
                  <span className="text-xs font-medium">{evt.shipId}</span>
                  <span className="text-xs text-[var(--text-muted)]">
                    {new Date(evt.timestamp).toLocaleTimeString()}
                  </span>
                </div>
                <div className="text-xs text-[var(--text-secondary)] mt-1">
                  {evt.eventType}
                </div>
              </div>
            ))}
          </div>
        </div>
      )}

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
                  {ship.summary.totalEvents.toLocaleString()} events
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
