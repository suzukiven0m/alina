import CodeBlock from '../components/CodeBlock'

export default function Challenges() {
  return (
    <section className="max-w-3xl mx-auto px-6 py-12 border-t border-[var(--border)]">
      <h2 className="text-lg font-semibold mb-2">Key Challenges</h2>
      <p className="text-[var(--text-secondary)] mb-8">
        Building a distributed system for vessels at sea means solving problems
        that do not exist in datacenter environments.
      </p>

      <div className="space-y-8">
        <div>
          <h3 className="font-semibold mb-2">Intermittent Connectivity</h3>
          <p className="text-[var(--text-secondary)] text-sm mb-3">
            Satellite links go down. The system cannot fail when they do. A
            circuit breaker pattern guards the satellite gateway: after three
            consecutive failures, the breaker opens and events queue locally.
            When the link recovers, the breaker closes and the backlog drains.
          </p>
          <CodeBlock code={`// CircuitBreaker.cs
public enum CircuitState { Closed, Open, HalfOpen }

public class CircuitBreaker
{
    private int _failureCount;
    private readonly int _threshold = 3;
    private readonly TimeSpan _timeout = TimeSpan.FromMinutes(5);

    public CircuitState State => _failureCount >= _threshold
        ? CircuitState.Open
        : CircuitState.Closed;
}`} />
        </div>

        <div>
          <h3 className="font-semibold mb-2">Rule Evaluation Engine</h3>
          <p className="text-[var(--text-secondary)] text-sm mb-3">
            Every sensor reading must be evaluated against business rules in
            real time. Rules are declarative: each defines a sensor type, a
            threshold, and a priority. When a rule fires, an alert event is
            queued immediately.
          </p>
          <CodeBlock code={`// Rule evaluation
foreach (var rule in _rules)
{
    var reading = readings.FirstOrDefault(r => r.Sensor == rule.Sensor);
    if (reading != null && rule.Evaluate(reading.Value))
    {
        yield return new AlertTriggeredEvent(rule, reading);
    }
}`} />
        </div>

        <div>
          <h3 className="font-semibold mb-2">Priority Event Queuing</h3>
          <p className="text-[var(--text-secondary)] text-sm mb-3">
            Not all events are equal. A hull breach is more urgent than a routine
            temperature reading. Events are queued by priority and drained in
            order. SQLite persistence ensures no event is lost across process
            restarts.
          </p>
        </div>
      </div>
    </section>
  )
}
