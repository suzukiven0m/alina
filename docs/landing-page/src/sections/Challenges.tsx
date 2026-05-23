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
    icon: <WifiOff size={32} className="text-accent-red" />,
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
    icon: <Layers size={32} className="text-accent-amber" />,
    title: 'Event Prioritization',
    problem: 'Not all telemetry is equal. Fire alarms must outrank routine engine temperature checks.',
    solution: 'Priority-aware queue with three tiers: Critical, Operational, Telemetry. Dequeue always pulls highest priority first.',
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
    icon: <Database size={32} className="text-accent-green" />,
    title: 'Data Loss Prevention',
    problem: 'A ship-edge worker crash must not lose events that failed to transmit.',
    solution: 'Incremental SQLite persistence. Only new events are appended. On restart, the queue is restored from disk.',
    code: `public async Task PersistAsync()
{
    await using var connection = new SqliteConnection(_connectionString);
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
  const [hoveredIndex, setHoveredIndex] = useState<number | null>(null)

  return (
    <section className="py-24 md:py-32 relative z-10">
      <div className="max-w-6xl mx-auto px-6">
        <ScrollReveal>
          <h2 className="text-4xl md:text-5xl font-bold text-center mb-4">
            Hard <span className="text-accent-cyan">Problems</span> Solved
          </h2>
          <p className="text-text-secondary text-center max-w-2xl mx-auto mb-16">
            Real maritime constraints demand real engineering solutions.
          </p>
        </ScrollReveal>

        <div className="grid md:grid-cols-3 gap-6">
          {challenges.map((challenge, index) => (
            <ScrollReveal key={challenge.title} delay={index * 0.15}>
              <div
                className="relative bg-bg-secondary rounded-xl p-8 border border-[#1e3a5f] hover:border-accent-cyan transition-all hover:-translate-y-1 group"
                onMouseEnter={() => setHoveredIndex(index)}
                onMouseLeave={() => setHoveredIndex(null)}
              >
                <div className="mb-4">{challenge.icon}</div>
                <h3 className="text-xl font-bold text-text-primary mb-3">
                  {challenge.title}
                </h3>
                <p className="text-text-secondary text-sm mb-4">
                  <span className="text-accent-red font-medium">Problem: </span>
                  {challenge.problem}
                </p>
                <p className="text-text-secondary text-sm">
                  <span className="text-accent-green font-medium">Solution: </span>
                  {challenge.solution}
                </p>

                {/* Code tooltip */}
                <div
                  className={`absolute left-0 right-0 -bottom-4 translate-y-full z-20 p-4 transition-all duration-300 ${
                    hoveredIndex === index
                      ? 'opacity-100 visible'
                      : 'opacity-0 invisible'
                  }`}
                >
                  <CodeBlock code={challenge.code} />
                </div>
              </div>
            </ScrollReveal>
          ))}
        </div>
      </div>
    </section>
  )
}
