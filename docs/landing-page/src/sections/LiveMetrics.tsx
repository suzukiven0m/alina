import ScrollReveal from '../components/ScrollReveal'
import AnimatedCounter from '../components/AnimatedCounter'

const metrics = [
  { end: 51, suffix: '', label: 'Unit & Integration Tests' },
  { end: 4, suffix: '', label: 'Microservices / Projects' },
  { end: 12, suffix: '', label: 'REST API Endpoints' },
  { end: 100, suffix: '%', label: 'Test Pass Rate' },
]

const fleetStatus = [
  { name: 'MSC-001', status: 'online', location: 'Atlantic Ocean' },
  { name: 'MSC-002', status: 'online', location: 'Mediterranean' },
  { name: 'EVER-003', status: 'offline', location: 'Pacific Ocean' },
  { name: 'MAER-004', status: 'online', location: 'Indian Ocean' },
]

export default function LiveMetrics() {
  return (
    <section className="py-24 md:py-32 relative z-10">
      <div className="max-w-6xl mx-auto px-6">
        <ScrollReveal>
          <h2 className="text-4xl md:text-5xl font-bold text-center mb-4">
            Live <span className="text-accent-cyan">Metrics</span>
          </h2>
          <p className="text-text-secondary text-center max-w-2xl mx-auto mb-16">
            Numbers that tell the story of engineering quality.
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

        {/* Fleet status dashboard mockup */}
        <ScrollReveal delay={0.3}>
          <div className="bg-bg-secondary rounded-xl p-6 border border-[#1e3a5f]">
            <h3 className="text-lg font-semibold text-text-primary mb-4 flex items-center gap-2">
              <span className="w-2 h-2 rounded-full bg-accent-green animate-pulse" />
              Fleet Status Monitor
            </h3>
            <div className="grid md:grid-cols-2 gap-4">
              {fleetStatus.map((ship) => (
                <div
                  key={ship.name}
                  className="flex items-center justify-between bg-bg-primary rounded-lg p-4"
                >
                  <div className="flex items-center gap-3">
                    <div
                      className={`w-3 h-3 rounded-full ${
                        ship.status === 'online'
                          ? 'bg-accent-green'
                          : 'bg-accent-red'
                      }`}
                    >
                      {ship.status === 'online' && (
                        <div className="w-3 h-3 rounded-full bg-accent-green animate-ping opacity-50" />
                      )}
                    </div>
                    <div>
                      <div className="font-medium text-text-primary">
                        {ship.name}
                      </div>
                      <div className="text-xs text-text-muted">
                        {ship.location}
                      </div>
                    </div>
                  </div>
                  <span
                    className={`text-xs px-2 py-1 rounded-full ${
                      ship.status === 'online'
                        ? 'bg-accent-green/20 text-accent-green'
                        : 'bg-accent-red/20 text-accent-red'
                    }`}
                  >
                    {ship.status.toUpperCase()}
                  </span>
                </div>
              ))}
            </div>
          </div>
        </ScrollReveal>
      </div>
    </section>
  )
}
