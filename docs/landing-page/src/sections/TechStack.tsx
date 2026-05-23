import { useState } from 'react'
import ScrollReveal from '../components/ScrollReveal'

interface Tech {
  name: string
  category: string
  tooltip: string
}

const technologies: Tech[] = [
  { name: '.NET 10', category: 'Runtime', tooltip: 'Latest LTS with improved performance and minimal API enhancements' },
  { name: 'ASP.NET Core Minimal APIs', category: 'Web', tooltip: 'Low-ceremony HTTP layer perfect for telemetry ingestion endpoints' },
  { name: 'EF Core', category: 'Data', tooltip: 'Clean data access with migrations and SQLite provider' },
  { name: 'SQLite', category: 'Data', tooltip: 'Embedded, zero-config persistence perfect for edge devices and rapid testing' },
  { name: 'xUnit', category: 'Testing', tooltip: 'Modern testing framework with rich assertion library' },
  { name: 'WebApplicationFactory', category: 'Testing', tooltip: 'Full in-memory integration tests without HTTP overhead' },
  { name: 'Docker', category: 'DevOps', tooltip: 'Containerized deployment for consistent environments' },
  { name: 'Docker Compose', category: 'DevOps', tooltip: 'One-command orchestration of FleetCloud + ShipEdge + Simulators' },
  { name: 'GitHub Actions', category: 'CI/CD', tooltip: 'Automated build, test, and deployment pipelines' },
]

export default function TechStack() {
  const [hoveredTech, setHoveredTech] = useState<string | null>(null)

  return (
    <section className="py-24 md:py-32 relative z-10">
      <div className="max-w-6xl mx-auto px-6">
        <ScrollReveal>
          <h2 className="text-4xl md:text-5xl font-bold text-center mb-4">
            Tech <span className="text-accent-cyan">Stack</span>
          </h2>
          <p className="text-text-secondary text-center max-w-2xl mx-auto mb-16">
            Production-grade tools chosen for reliability, performance, and developer experience.
          </p>
        </ScrollReveal>

        <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-4">
          {technologies.map((tech, index) => (
            <ScrollReveal key={tech.name} delay={index * 0.05}>
              <div
                className="relative bg-bg-secondary rounded-lg p-6 border border-[#1e3a5f] hover:border-accent-cyan transition-all hover:scale-105 cursor-default"
                onMouseEnter={() => setHoveredTech(tech.name)}
                onMouseLeave={() => setHoveredTech(null)}
              >
                <div className="text-xs text-text-muted uppercase tracking-wider mb-2">
                  {tech.category}
                </div>
                <div className="text-lg font-semibold text-text-primary">
                  {tech.name}
                </div>

                {/* Tooltip */}
                <div
                  className={`absolute left-1/2 -translate-x-1/2 bottom-full mb-2 px-3 py-2 bg-bg-primary border border-accent-cyan rounded-lg text-sm text-text-secondary whitespace-nowrap z-30 transition-all ${
                    hoveredTech === tech.name
                      ? 'opacity-100 visible translate-y-0'
                      : 'opacity-0 invisible translate-y-2'
                  }`}
                >
                  {tech.tooltip}
                  <div className="absolute left-1/2 -translate-x-1/2 top-full w-2 h-2 bg-bg-primary border-r border-b border-accent-cyan rotate-45 -mt-1" />
                </div>
              </div>
            </ScrollReveal>
          ))}
        </div>
      </div>
    </section>
  )
}
