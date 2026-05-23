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
    <section className="py-20 md:py-28 border-b border-bg-tertiary">
      <div className="max-w-3xl mx-auto px-6">
        <ScrollReveal>
          <p className="text-text-muted text-sm font-mono mb-3">Highlights</p>
          <h2 className="text-3xl md:text-4xl font-semibold mb-4">
            Code quality
          </h2>
          <p className="text-text-secondary mb-12 max-w-2xl">
            Engineering decisions that matter at scale.
          </p>
        </ScrollReveal>

        <div className="space-y-6">
          {highlights.map((highlight, index) => (
            <ScrollReveal key={highlight.title} delay={index * 0.1}>
              <article className="border border-bg-tertiary rounded-lg p-6">
                <h3 className="text-lg font-medium text-text-primary mb-2">
                  {highlight.title}
                </h3>
                <p className="text-text-secondary text-sm mb-4">
                  {highlight.description}
                </p>
                <CodeBlock code={highlight.code} />
              </article>
            </ScrollReveal>
          ))}
        </div>
      </div>
    </section>
  )
}
