import CodeBlock from '../components/CodeBlock'

const commands = `git clone https://github.com/suzukiven0m/alina.git
cd alina
docker compose up`

export default function GettingStarted() {
  return (
    <section id="getting-started" className="max-w-3xl mx-auto px-6 py-12 border-t border-[var(--border)]">
      <h2 className="text-lg font-semibold mb-2">Getting Started</h2>
      <p className="text-[var(--text-secondary)] mb-6">
        Clone, compose, and go. Three commands to a running system.
      </p>

      <CodeBlock code={commands} />

      <p className="mt-4 text-sm text-[var(--text-muted)]">
        The simulator will generate synthetic sensor data and exercise the full
        pipeline including satellite link failures.
      </p>
    </section>
  )
}
