import ScrollReveal from '../components/ScrollReveal'
import CodeBlock from '../components/CodeBlock'

interface Highlight {
  title: string
  description: string
  code: string
}

const highlights: Highlight[] = [
  {
    title: 'Thread-Safe CircuitBreaker',
    description: 'Lock-free failure counting with atomic state transitions. No locks, no contention.',
    code: `private int _failureCount = 0;
private CircuitBreakerState _state = CircuitBreakerState.Closed;

public void RecordFailure()
{
    var count = Interlocked.Increment(ref _failureCount);
    if (count >= _failureThreshold)
    {
        _state = CircuitBreakerState.Open;
        _lastFailure = _timeProvider.GetUtcNow();
    }
}`,
  },
  {
    title: 'Incremental Persistence',
    description: 'O(k) writes for k new events instead of O(n) full-table rewrites. SQLite WAL mode for concurrency.',
    code: `private readonly List<ShipEvent> _unpersistedEvents = new();

public void Enqueue(ShipEvent evt)
{
    _unpersistedEvents.Add(evt);
    // Only appended to in-memory queue
    // PersistAsync writes only the delta
}`,
  },
  {
    title: 'Parallel Processing',
    description: 'Telemetry collection and queue transmission run concurrently. SQLite serialization lock prevents corruption.',
    code: `await Task.WhenAll(
    CollectTelemetryAsync(ct),
    TransmitQueueAsync(ct),
    PersistQueueAsync(ct)
);`,
  },
  {
    title: 'Input Validation',
    description: 'Request size limits and JSON schema validation on all inbound endpoints. Defense in depth.',
    code: `app.Use(async (context, next) =>
{
    context.Request.EnableBuffering();
    var body = await new StreamReader(context.Request.Body).ReadToEndAsync();
    context.Request.Body.Position = 0;

    if (body.Length > MaxRequestSize)
    {
        context.Response.StatusCode = 413;
        return;
    }
    await next();
});`,
  },
]

export default function CodeHighlights() {
  return (
    <section className="py-24 md:py-32 relative z-10">
      <div className="max-w-6xl mx-auto px-6">
        <ScrollReveal>
          <h2 className="text-4xl md:text-5xl font-bold text-center mb-4">
            Code <span className="text-accent-cyan">Quality</span>
          </h2>
          <p className="text-text-secondary text-center max-w-2xl mx-auto mb-16">
            Engineering decisions that matter at scale.
          </p>
        </ScrollReveal>

        <div className="grid md:grid-cols-2 gap-6">
          {highlights.map((highlight, index) => (
            <ScrollReveal key={highlight.title} delay={index * 0.1}>
              <div className="bg-bg-secondary rounded-xl p-6 border border-[#1e3a5f] hover:border-accent-cyan transition-all">
                <h3 className="text-xl font-bold text-accent-amber mb-2">
                  {highlight.title}
                </h3>
                <p className="text-text-secondary text-sm mb-4">
                  {highlight.description}
                </p>
                <CodeBlock code={highlight.code} />
              </div>
            </ScrollReveal>
          ))}
        </div>
      </div>
    </section>
  )
}
