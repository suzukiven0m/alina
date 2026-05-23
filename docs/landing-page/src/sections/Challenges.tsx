import { useState } from 'react'
import { WifiOff, Layers, Database } from 'lucide-react'
import ScrollReveal from '../components/ScrollReveal'
import CodeBlock from '../components/CodeBlock'

interface Challenge {
  icon: React.ReactNode
  title: string
  problem: string
  solution: string
  code: string
}

const challenges: Challenge[] = [
  {
    icon: <WifiOff size={24} className="text-accent-red" />,
    title: 'Satellite Intermittency',
    problem: 'Ships lose satellite connectivity unpredictably. Failed transmissions must not crash the system or flood the network.',
    solution: 'Circuit breaker pattern with exponential backoff. After N failures, the circuit opens and queues events locally.',
    code: `public bool CanTransmit()
{
    if (State == CircuitBreakerState.Open)
    {
        if (_timeProvider.GetUtcNow() - _lastFailure < _timeout)
            return false;
        State = CircuitBreakerState.HalfOpen;
    }
    return true;
}`,
  },
  {
    icon: <Layers size={24} className="text-accent-amber" />,
    title: 'Event Prioritization',
    problem: 'Not all telemetry is equal. Fire alarms must outrank routine engine temperature checks.',
    solution: 'Priority-aware queue with three tiers. Dequeue always pulls highest priority first.',
    code: `public ShipEvent? Dequeue()
{
    if (_critical.TryDequeue(out var critical))
        return critical;
    if (_operational.TryDequeue(out var op))
        return op;
    if (_telemetry.TryDequeue(out var telem))
        return telem;
    return null;
}`,
  },
  {
    icon: <Database size={24} className="text-accent-green" />,
    title: 'Data Loss Prevention',
    problem: 'A ship-edge worker crash must not lose events that failed to transmit.',
    solution: 'Incremental SQLite persistence. Only new events are appended. On restart, the queue is restored from disk.',
    code: `public async Task PersistAsync()
{
    await using var connection =
        new SqliteConnection(_connectionString);
    await connection.OpenAsync();
    foreach (var evt in _unpersistedEvents)
    {
        await InsertEventAsync(connection, evt);
    }
    _unpersistedEvents.Clear();
}`,
  },
]

export default function Challenges() {
  const [expanded, setExpanded] = useState<number | null>(null)

  return (
    <section className="py-20 md:py-28 border-b border-bg-tertiary">
      <div className="max-w-3xl mx-auto px-6">
        <ScrollReveal>
          <p className="text-text-muted text-sm font-mono mb-3">Design Decisions</p>
          <h2 className="text-3xl md:text-4xl font-semibold mb-4">
            Hard problems solved
          </h2>
          <p className="text-text-secondary mb-12 max-w-2xl">
            Maritime constraints demand specific engineering tradeoffs.
            Here is how the system handles three of the hardest.
          </p>
        </ScrollReveal>

        <div className="space-y-6">
          {challenges.map((challenge, index) => {
            const isOpen = expanded === index
            return (
              <ScrollReveal key={challenge.title} delay={index * 0.1}>
                <article className="border border-bg-tertiary rounded-lg p-6 hover:border-bg-tertiary/80 transition-colors">
                  <div className="flex items-start gap-4">
                    <div className="mt-1 flex-shrink-0">{challenge.icon}</div>
                    <div className="flex-1 min-w-0">
                      <h3 className="text-lg font-medium text-text-primary mb-2">
                        {challenge.title}
                      </h3>
                      <p className="text-text-secondary text-sm mb-3">
                        <span className="text-accent-red">Problem: </span>
                        {challenge.problem}
                      </p>
                      <p className="text-text-secondary text-sm mb-4">
                        <span className="text-accent-green">Solution: </span>
                        {challenge.solution}
                      </p>

                      <button
                        onClick={() => setExpanded(isOpen ? null : index)}
                        className="text-sm text-accent hover:text-text-primary transition-colors font-mono"
                      >
                        {isOpen ? 'Hide implementation' : 'Show implementation'}
                      </button>

                      {isOpen && (
                        <div className="mt-4">
                          <CodeBlock code={challenge.code} />
                        </div>
                      )}
                    </div>
                  </div>
                </article>
              </ScrollReveal>
            )
          })}
        </div>
      </div>
    </section>
  )
}
