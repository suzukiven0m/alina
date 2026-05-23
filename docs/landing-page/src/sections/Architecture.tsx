interface Component {
  id: string
  name: string
  role: string
  description: string
  responsibilities: string[]
  githubPath: string
}

const components: Component[] = [
  {
    id: 'ShipEdge',
    name: 'ShipEdge',
    role: 'Background Worker',
    description:
      'Runs on every vessel. Collects sensor telemetry, evaluates business rules, and transmits critical events to FleetCloud via a resilient satellite gateway.',
    responsibilities: [
      'Collect telemetry from engine, hull, and cargo sensors',
      'Evaluate rules: fire detection, engine overheat, hull breach',
      'Queue events by priority (Critical > Operational > Telemetry)',
      'Transmit via satellite with circuit breaker resilience',
      'Persist failed events to SQLite for crash recovery',
    ],
    githubPath: 'src/ShipEdge',
  },
  {
    id: 'FleetCloud',
    name: 'FleetCloud',
    role: 'Fleet API',
    description:
      'ASP.NET Core Minimal API receiving telemetry and alerts from all ships. Exposes endpoints for fleet health monitoring and ship registry management.',
    responsibilities: [
      'Ingest telemetry batches from ShipEdge workers',
      'Maintain ship registry with online/offline status',
      'Expose REST endpoints for fleet dashboard data',
      'Validate inbound requests with size limits and JSON schema',
    ],
    githubPath: 'src/FleetCloud',
  },
  {
    id: 'Shared',
    name: 'Shared',
    role: 'Contracts',
    description:
      'Common domain models, events, and contracts referenced by both FleetCloud and ShipEdge. Keeps the system contractually consistent.',
    responsibilities: [
      'Define sensor reading, alert, and command models',
      'Serialize events for wire transfer and SQLite storage',
      'Ensure type safety across service boundaries',
    ],
    githubPath: 'src/Shared',
  },
  {
    id: 'Simulators',
    name: 'Simulators',
    role: 'Test Harness',
    description:
      'Console applications that inject realistic failure modes into the system for integration and end-to-end testing.',
    responsibilities: [
      'Simulate satellite link intermittency (random up/down)',
      'Generate synthetic sensor data streams',
      'Drive end-to-end integration tests',
    ],
    githubPath: 'src/Simulators',
  },
]

export default function Architecture() {
  return (
    <section id="architecture" className="max-w-3xl mx-auto px-6 py-12 border-t border-[var(--border)]">
      <h2 className="text-lg font-semibold mb-2">Architecture</h2>
      <p className="text-[var(--text-secondary)] mb-8">
        Four projects, one system. ShipEdge runs on the vessel, FleetCloud runs
        on shore. Shared keeps the contracts straight. Simulators make sure the
        whole thing holds up under failure.
      </p>

      {components.map((c) => (
        <div key={c.id} className="mb-8">
          <div className="flex items-baseline gap-3 mb-2">
            <h3 className="font-semibold">{c.name}</h3>
            <span className="text-sm text-[var(--text-muted)] font-mono">{c.role}</span>
          </div>
          <p className="text-[var(--text-secondary)] text-sm mb-3">{c.description}</p>

          <table>
            <thead>
              <tr>
                <th>Responsibility</th>
              </tr>
            </thead>
            <tbody>
              {c.responsibilities.map((r, i) => (
                <tr key={i}>
                  <td>{r}</td>
                </tr>
              ))}
            </tbody>
          </table>

          <a
            href={`https://github.com/suzukiven0m/alina/tree/master/${c.githubPath}`}
            target="_blank"
            rel="noopener noreferrer"
            className="text-sm"
          >
            View source
          </a>
        </div>
      ))}
    </section>
  )
}
