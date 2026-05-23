import ThemeToggle from '../components/ThemeToggle'

export default function Header() {
  return (
    <header className="max-w-3xl mx-auto px-6 pt-16 pb-12">
      <div className="flex items-start justify-between gap-4">
        <div>
          <h1 className="text-2xl font-semibold tracking-tight mb-2">
            Cargo Ship Monitoring
          </h1>
          <p className="text-[var(--text-secondary)]">
            Real-time telemetry, intelligent alerts, and resilient satellite
            communication for maritime fleets.
          </p>
          <p className="mt-2 text-sm text-[var(--text-muted)] font-mono">
            .NET 10 / SQLite / Docker
          </p>
        </div>
        <ThemeToggle />
      </div>
      <div className="mt-6 flex gap-4 text-sm">
        <a
          href="https://github.com/suzukiven0m/alina"
          target="_blank"
          rel="noopener noreferrer"
        >
          View on GitHub
        </a>
        <a href="#architecture">Architecture</a>
        <a href="#getting-started">Getting Started</a>
      </div>
    </header>
  )
}
