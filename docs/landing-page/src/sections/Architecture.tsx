import { useState } from 'react'
import { ChevronDown, ChevronUp } from 'lucide-react'
import ScrollReveal from '../components/ScrollReveal'

interface Project {
  id: string
  name: string
  role: string
  description: string
  technologies: string[]
  responsibilities: string[]
  githubPath: string
}

const projects: Project[] = [
  {
    id: 'ShipEdge',
    name: 'ShipEdge',
    role: 'Background Worker',
    description:
      'Runs on every vessel. Collects sensor telemetry, evaluates business rules, and transmits critical events to FleetCloud via a resilient satellite gateway.',
    technologies: ['.NET 10', 'BackgroundService', 'Priority Queue', 'Circuit Breaker', 'SQLite'],
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
    technologies: ['.NET 10', 'Minimal APIs', 'EF Core', 'SQLite', 'xUnit'],
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
    technologies: ['.NET 10 Class Library', 'System.Text.Json'],
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
    technologies: ['.NET 10 Console', 'Randomized failure injection'],
    responsibilities: [
      'Simulate satellite link intermittency (random up/down)',
      'Generate synthetic sensor data streams',
      'Drive end-to-end integration tests',
    ],
    githubPath: 'src/Simulators',
  },
]

export default function Architecture() {
  const [expanded, setExpanded] = useState<string | null>('ShipEdge')

  return (
    <section id="architecture" className="py-20 md:py-28 border-b border-bg-tertiary">
      <div className="max-w-3xl mx-auto px-6">
        <ScrollReveal>
          <p className="text-text-muted text-sm font-mono mb-3">Architecture</p>
          <h2 className="text-3xl md:text-4xl font-semibold mb-4">
            How it works
          </h2>
          <p className="text-text-secondary mb-12 max-w-2xl">
            Four projects, one system. ShipEdge runs on the vessel, FleetCloud
            runs on shore. Shared keeps the contracts straight. Simulators make
            sure the whole thing holds up under failure.
          </p>
        </ScrollReveal>

        <div className="space-y-3">
          {projects.map((project) => {
            const isOpen = expanded === project.id
            return (
              <ScrollReveal key={project.id}>
                <div className="border border-bg-tertiary rounded-lg overflow-hidden">
                  <button
                    onClick={() => setExpanded(isOpen ? null : project.id)}
                    className="w-full flex items-center justify-between p-5 text-left hover:bg-bg-secondary transition-colors"
                  >
                    <div className="flex items-center gap-4">
                      <span className="text-accent font-mono text-sm">
                        {project.id}
                      </span>
                      <div>
                        <span className="font-medium text-text-primary">
                          {project.name}
                        </span>
                        <span className="text-text-muted text-sm ml-2">
                          {project.role}
                        </span>
                      </div>
                    </div>
                    {isOpen ? (
                      <ChevronUp size={18} className="text-text-muted" />
                    ) : (
                      <ChevronDown size={18} className="text-text-muted" />
                    )}
                  </button>

                  {isOpen && (
                    <div className="px-5 pb-5 border-t border-bg-tertiary bg-bg-secondary/30">
                      <p className="text-text-secondary text-sm mt-4 mb-4 leading-relaxed">
                        {project.description}
                      </p>

                      <div className="flex flex-wrap gap-2 mb-5">
                        {project.technologies.map((tech) => (
                          <span
                            key={tech}
                            className="px-2 py-1 bg-bg-primary rounded text-xs text-text-muted font-mono border border-bg-tertiary"
                          >
                            {tech}
                          </span>
                        ))}
                      </div>

                      <ul className="space-y-2 mb-5">
                        {project.responsibilities.map((item, i) => (
                          <li
                            key={i}
                            className="text-sm text-text-secondary flex items-start gap-2"
                          >
                            <span className="text-accent mt-1.5">—</span>
                            {item}
                          </li>
                        ))}
                      </ul>

                      <a
                        href={`https://github.com/suzukiven0m/alina/tree/master/${project.githubPath}`}
                        target="_blank"
                        rel="noopener noreferrer"
                        className="text-sm text-accent hover:text-text-primary transition-colors"
                      >
                        View source →
                      </a>
                    </div>
                  )}
                </div>
              </ScrollReveal>
            )
          })}
        </div>
      </div>
    </section>
  )
}
