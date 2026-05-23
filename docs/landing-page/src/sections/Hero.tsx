import { Github, ArrowRight } from 'lucide-react'

export default function Hero() {
  return (
    <section className="relative min-h-[70vh] flex items-center justify-center border-b border-bg-tertiary">
      <div className="max-w-3xl mx-auto px-6 text-center py-24">
        <p className="text-text-muted text-sm font-mono mb-6 tracking-wide">
          .NET 10 / SQLite / Docker
        </p>

        <h1 className="text-4xl md:text-5xl font-semibold tracking-tight mb-6">
          Cargo Ship Monitoring
        </h1>

        <p className="text-lg text-text-secondary max-w-xl mx-auto mb-10">
          Real-time telemetry, intelligent alerts, and resilient satellite
          communication for maritime fleets. A distributed system designed
          for intermittent connectivity.
        </p>

        <div className="flex flex-col sm:flex-row gap-4 justify-center">
          <a
            href="https://github.com/suzukiven0m/alina"
            target="_blank"
            rel="noopener noreferrer"
            className="inline-flex items-center justify-center gap-2 px-6 py-3 bg-accent text-bg-primary font-medium rounded hover:bg-accent-dim transition-colors"
          >
            <Github size={18} />
            View on GitHub
          </a>
          <a
            href="#architecture"
            className="inline-flex items-center justify-center gap-2 px-6 py-3 text-accent font-medium rounded hover:text-text-primary transition-colors"
          >
            Read the architecture
            <ArrowRight size={18} />
          </a>
        </div>
      </div>
    </section>
  )
}
