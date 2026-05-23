import { useState } from 'react'
import ScrollReveal from '../components/ScrollReveal'

interface ProjectDetail {
  name: string
  description: string
  technologies: string[]
  githubPath: string
}

const projects: Record<string, ProjectDetail> = {
  FleetCloud: {
    name: 'FleetCloud',
    description: 'ASP.NET Core Minimal API that receives telemetry and alerts from ship-edge workers. Exposes REST endpoints for fleet-wide monitoring and health checks.',
    technologies: ['.NET 10', 'ASP.NET Core Minimal APIs', 'EF Core', 'SQLite'],
    githubPath: 'src/FleetCloud',
  },
  ShipEdge: {
    name: 'ShipEdge',
    description: 'Background worker running on each ship. Collects sensor data, evaluates rules, and transmits critical events via satellite gateway with circuit breaker resilience.',
    technologies: ['.NET 10', 'BackgroundService', 'Priority Queue', 'Circuit Breaker'],
    githubPath: 'src/ShipEdge',
  },
  Shared: {
    name: 'Shared',
    description: 'Common models, events, and contracts used by both FleetCloud and ShipEdge. Defines sensor readings, alert types, and serialization formats.',
    technologies: ['.NET 10 Class Library', 'System.Text.Json'],
    githubPath: 'src/Shared',
  },
  Simulators: {
    name: 'Simulators',
    description: 'Test harnesses that simulate satellite link intermittency and sensor data streams for integration and end-to-end testing.',
    technologies: ['.NET 10 Console', 'Randomized failure injection'],
    githubPath: 'src/Simulators',
  },
}

export default function Architecture() {
  const [selected, setSelected] = useState<string | null>(null)
  const [failureMode, setFailureMode] = useState(false)

  const selectedProject = selected ? projects[selected] : null

  return (
    <section id="architecture" className="py-24 md:py-32 relative z-10">
      <div className="max-w-6xl mx-auto px-6">
        <ScrollReveal>
          <h2 className="text-4xl md:text-5xl font-bold text-center mb-4">
            System <span className="text-accent-cyan">Architecture</span>
          </h2>
          <p className="text-text-secondary text-center max-w-2xl mx-auto mb-12">
            Four interconnected projects forming a resilient distributed monitoring system.
          </p>
        </ScrollReveal>

        <ScrollReveal delay={0.2}>
          <div className="flex justify-center mb-8">
            <button
              onClick={() => setFailureMode((prev) => !prev)}
              aria-pressed={failureMode}
              className={`px-6 py-2 rounded-lg font-medium transition-colors ${
                failureMode
                  ? 'bg-accent-red text-white'
                  : 'bg-accent-green text-bg-primary'
              }`}
            >
              {failureMode ? 'Failure Mode' : 'Normal Operations'}
            </button>
          </div>
        </ScrollReveal>

        <ScrollReveal delay={0.3}>
          <div className="relative bg-bg-secondary rounded-xl p-8 md:p-12 overflow-hidden">
            {/* Architecture diagram */}
            <div className="flex flex-col md:flex-row items-center justify-center gap-8 md:gap-16">
              {/* ShipEdge */}
              <div
                role="button"
                tabIndex={0}
                aria-pressed={selected === 'ShipEdge'}
                aria-label="Select ShipEdge project details"
                onClick={() => setSelected('ShipEdge')}
                onKeyDown={(e) => e.key === 'Enter' && setSelected('ShipEdge')}
                className={`cursor-pointer rounded-xl p-6 border-2 transition-all hover:scale-105 ${
                  selected === 'ShipEdge'
                    ? 'border-accent-cyan bg-bg-primary'
                    : 'border-[#1e3a5f] hover:border-accent-cyan'
                }`}
              >
                <div className="text-2xl font-bold text-accent-amber mb-1">ShipEdge</div>
                <div className="text-sm text-text-muted">Ship-side Worker</div>
              </div>

              {/* Connection with arrow */}
              <div className="flex flex-col items-center">
                <svg width="80" height="40" className="hidden md:block">
                  <line
                    x1="0"
                    y1="20"
                    x2="70"
                    y2="20"
                    stroke={failureMode ? '#ef4444' : '#10b981'}
                    strokeWidth="2"
                    className={failureMode ? '' : 'pulse-line'}
                  />
                  <polygon
                    points="70,20 60,15 60,25"
                    fill={failureMode ? '#ef4444' : '#10b981'}
                  />
                  {failureMode && (
                    <text x="35" y="12" textAnchor="middle" fill="#ef4444" fontSize="10">
                      OPEN
                    </text>
                  )}
                </svg>
                <span className="text-xs text-text-muted mt-1">HTTP / Satellite</span>
              </div>

              {/* FleetCloud */}
              <div
                role="button"
                tabIndex={0}
                aria-pressed={selected === 'FleetCloud'}
                aria-label="Select FleetCloud project details"
                onClick={() => setSelected('FleetCloud')}
                onKeyDown={(e) => e.key === 'Enter' && setSelected('FleetCloud')}
                className={`cursor-pointer rounded-xl p-6 border-2 transition-all hover:scale-105 ${
                  selected === 'FleetCloud'
                    ? 'border-accent-cyan bg-bg-primary'
                    : 'border-[#1e3a5f] hover:border-accent-cyan'
                }`}
              >
                <div className="text-2xl font-bold text-accent-green mb-1">FleetCloud</div>
                <div className="text-sm text-text-muted">Fleet API</div>
              </div>
            </div>

            {/* Shared and Simulators below */}
            <div className="flex justify-center gap-8 mt-12">
              <div
                role="button"
                tabIndex={0}
                aria-pressed={selected === 'Shared'}
                aria-label="Select Shared project details"
                onClick={() => setSelected('Shared')}
                onKeyDown={(e) => e.key === 'Enter' && setSelected('Shared')}
                className={`cursor-pointer rounded-xl p-6 border-2 transition-all hover:scale-105 ${
                  selected === 'Shared'
                    ? 'border-accent-cyan bg-bg-primary'
                    : 'border-[#1e3a5f] hover:border-accent-cyan'
                }`}
              >
                <div className="text-xl font-bold text-accent-cyan mb-1">Shared</div>
                <div className="text-sm text-text-muted">Models & Events</div>
              </div>

              <div
                role="button"
                tabIndex={0}
                aria-pressed={selected === 'Simulators'}
                aria-label="Select Simulators project details"
                onClick={() => setSelected('Simulators')}
                onKeyDown={(e) => e.key === 'Enter' && setSelected('Simulators')}
                className={`cursor-pointer rounded-xl p-6 border-2 transition-all hover:scale-105 ${
                  selected === 'Simulators'
                    ? 'border-accent-cyan bg-bg-primary'
                    : 'border-[#1e3a5f] hover:border-accent-cyan'
                }`}
              >
                <div className="text-xl font-bold text-text-secondary mb-1">Simulators</div>
                <div className="text-sm text-text-muted">Test Harness</div>
              </div>
            </div>

            {/* Detail panel */}
            {selectedProject && (
              <div className="mt-8 p-6 bg-bg-primary rounded-lg border border-[#1e3a5f]">
                <h3 className="text-2xl font-bold text-accent-amber mb-2">
                  {selectedProject.name}
                </h3>
                <p className="text-text-secondary mb-4">
                  {selectedProject.description}
                </p>
                <div className="flex flex-wrap gap-2 mb-4">
                  {selectedProject.technologies.map((tech) => (
                    <span
                      key={tech}
                      className="px-3 py-1 bg-bg-secondary rounded-full text-sm text-accent-cyan"
                    >
                      {tech}
                    </span>
                  ))}
                </div>
                <a
                  href={`https://github.com/suzukiven0m/alina/tree/master/${selectedProject.githubPath}`}
                  target="_blank"
                  rel="noopener noreferrer"
                  className="text-accent-amber hover:underline text-sm"
                >
                  View source →
                </a>
              </div>
            )}
          </div>
        </ScrollReveal>
      </div>
    </section>
  )
}
