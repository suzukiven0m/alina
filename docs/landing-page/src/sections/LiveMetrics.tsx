import ScrollReveal from '../components/ScrollReveal'
import AnimatedCounter from '../components/AnimatedCounter'

const metrics = [
  { end: 51, suffix: '', label: 'Tests' },
  { end: 4, suffix: '', label: 'Projects' },
  { end: 12, suffix: '', label: 'API Endpoints' },
  { end: 100, suffix: '%', label: 'Pass Rate' },
]

export default function LiveMetrics() {
  return (
    <section className="py-20 md:py-28 border-b border-bg-tertiary">
      <div className="max-w-3xl mx-auto px-6">
        <ScrollReveal>
          <p className="text-text-muted text-sm font-mono mb-3">Quality</p>
          <h2 className="text-3xl md:text-4xl font-semibold mb-4">
            Test coverage
          </h2>
          <p className="text-text-secondary mb-12 max-w-2xl">
            Every component has tests. Integration tests verify end-to-end flow
            through the full stack.
          </p>
        </ScrollReveal>

        <div className="grid grid-cols-2 md:grid-cols-4 gap-8 mb-16">
          {metrics.map((metric) => (
            <AnimatedCounter
              key={metric.label}
              end={metric.end}
              suffix={metric.suffix}
              label={metric.label}
              duration={2000}
            />
          ))}
        </div>

        <ScrollReveal delay={0.2}>
          <div className="border border-bg-tertiary rounded-lg p-6">
            <h3 className="text-sm font-mono text-text-muted mb-4 uppercase tracking-wider">
              Test Breakdown
            </h3>
            <div className="space-y-3">
              <div className="flex justify-between items-center">
                <span className="text-text-secondary text-sm">ShipEdge.Tests — Unit & Integration</span>
                <span className="text-text-primary font-mono text-sm">34 tests</span>
              </div>
              <div className="w-full bg-bg-tertiary h-1 rounded">
                <div className="bg-accent h-1 rounded" style={{ width: '67%' }} />
              </div>
              <div className="flex justify-between items-center">
                <span className="text-text-secondary text-sm">FleetCloud.Tests — API Integration</span>
                <span className="text-text-primary font-mono text-sm">17 tests</span>
              </div>
              <div className="w-full bg-bg-tertiary h-1 rounded">
                <div className="bg-accent-dim h-1 rounded" style={{ width: '33%' }} />
              </div>
            </div>
          </div>
        </ScrollReveal>
      </div>
    </section>
  )
}
