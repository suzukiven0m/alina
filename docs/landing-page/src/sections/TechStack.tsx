export default function TechStack() {
  return (
    <section className="max-w-3xl mx-auto px-6 py-12 border-t border-[var(--border)]">
      <h2 className="text-lg font-semibold mb-4">Tech Stack</h2>

      <ul className="space-y-2 text-sm text-[var(--text-secondary)]">
        <li><strong className="text-[var(--text-primary)]">.NET 10</strong> — Runtime and ASP.NET Core Minimal APIs</li>
        <li><strong className="text-[var(--text-primary)]">SQLite</strong> — Local event persistence and ship registry</li>
        <li><strong className="text-[var(--text-primary)]">Entity Framework Core</strong> — Data access layer</li>
        <li><strong className="text-[var(--text-primary)]">Docker</strong> — Containerized deployment</li>
        <li><strong className="text-[var(--text-primary)]">xUnit</strong> — Unit and integration testing</li>
        <li><strong className="text-[var(--text-primary)]">React + Vite + Tailwind</strong> — Landing page</li>
      </ul>
    </section>
  )
}
