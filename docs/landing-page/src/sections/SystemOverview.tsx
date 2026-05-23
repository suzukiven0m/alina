export default function SystemOverview() {
  return (
    <section className="max-w-3xl mx-auto px-6 py-12 border-t border-[var(--border)]">
      <h2 className="text-lg font-semibold mb-4">System Overview</h2>

      <pre className="font-mono text-sm">
        <code>{`+----------+     +----------+     +----------+     +----------+
| Sensors  | --> | ShipEdge | --> | Satellite| --> |FleetCloud|
| (vessel) |     | (worker) |     | (link)   |     | (API)    |
+----------+     +----------+     +----------+     +----------+`}</code>
      </pre>

      <p className="mt-4 text-[var(--text-secondary)]">
        ShipEdge runs on each vessel as a background worker. It collects sensor
        telemetry, evaluates business rules, and queues events by priority.
        Critical events transmit via satellite to FleetCloud, an ASP.NET Core
        Minimal API running on shore. When the satellite link is down, events
        persist to SQLite and retry automatically.
      </p>
    </section>
  )
}
