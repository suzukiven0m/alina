import ScrollReveal from '../components/ScrollReveal'

interface Tech {
  name: string
  category: string
  why: string
}

const technologies: Tech[] = [
  { name: '.NET 10', category: 'Runtime', why: 'Latest LTS with AOT compilation support' },
  { name: 'ASP.NET Core Minimal APIs', category: 'Web', why: 'Low-ceremony HTTP for telemetry ingestion' },
  { name: 'EF Core', category: 'Data', why: 'Migrations and SQLite provider out of the box' },
  { name: 'SQLite', category: 'Data', why: 'Zero-config persistence for edge devices' },
  { name: 'xUnit', category: 'Testing', why: '51 tests covering unit and integration layers' },
  { name: 'WebApplicationFactory', category: 'Testing', why: 'Full in-memory integration tests' },
  { name: 'Docker', category: 'DevOps', why: 'Consistent deployment across environments' },
  { name: 'Docker Compose', category: 'DevOps', why: 'One command to spin up the entire system' },
]

export default function TechStack() {
  return (
    <section className="py-20 md:py-28 border-b border-bg-tertiary">
      <div className="max-w-3xl mx-auto px-6">
        <ScrollReveal>
          <p className="text-text-muted text-sm font-mono mb-3">Stack</p>
          <h2 className="text-3xl md:text-4xl font-semibold mb-4">
            What it is built with
          </h2>
          <p className="text-text-secondary mb-12 max-w-2xl">
            Tools chosen for reliability at the edge and developer velocity on shore.
          </p>
        </ScrollReveal>

        <div className="grid md:grid-cols-2 gap-px bg-bg-tertiary rounded-lg overflow-hidden border border-bg-tertiary">
          {technologies.map((tech, index) => (
            <ScrollReveal key={tech.name} delay={index * 0.03}>
              <div className="bg-bg-primary p-5 hover:bg-bg-secondary transition-colors">
                <div className="text-text-muted text-xs font-mono mb-1 uppercase tracking-wider">
                  {tech.category}
                </div>
                <div className="font-medium text-text-primary mb-1">
                  {tech.name}
                </div>
                <div className="text-text-secondary text-sm">
                  {tech.why}
                </div>
              </div>
            </ScrollReveal>
          ))}
        </div>
      </div>
    </section>
  )
}
