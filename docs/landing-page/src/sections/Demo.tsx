import { useState } from 'react'
import { Play } from 'lucide-react'
import ScrollReveal from '../components/ScrollReveal'
import Terminal from '../components/Terminal'

const demoLines = [
  'docker compose up',
  '[+] Running 3/3',
  ' ✔ Container alina-fleetcloud-1  Started',
  ' ✔ Container alina-shipedge-1    Started',
  ' ✔ Container alina-simulators-1  Started',
  '[ShipEdge] Worker starting for ship: MSC-001',
  '[ShipEdge] Connected to satellite gateway',
  '[ShipEdge] Telemetry collection started',
  '[Simulators] Satellite link: UP (latency: 230ms)',
  '[ShipEdge] Sending telemetry batch (12 events)',
  '[FleetCloud] Received 12 events from MSC-001',
  '[Simulators] Satellite link: DOWN (intermittency)',
  '[ShipEdge] Transmission failed: Circuit breaker opening...',
  '[ShipEdge] Circuit breaker OPEN. Queuing events locally.',
  '[ShipEdge] Persisting 8 events to SQLite...',
  '[ShipEdge] Queue persisted successfully',
  '[Simulators] Satellite link: UP (latency: 180ms)',
  '[ShipEdge] Circuit breaker entering Half-Open...',
  '[ShipEdge] Retry succeeded! Circuit breaker CLOSED.',
  '[ShipEdge] Transmitting queued events (8 queued)...',
  '[FleetCloud] Received 8 events from MSC-001',
]

export default function Demo() {
  const [isPlaying, setIsPlaying] = useState(false)

  return (
    <section className="py-20 md:py-28 border-b border-bg-tertiary">
      <div className="max-w-3xl mx-auto px-6">
        <ScrollReveal>
          <p className="text-text-muted text-sm font-mono mb-3">Demo</p>
          <h2 className="text-3xl md:text-4xl font-semibold mb-4">
            In action
          </h2>
          <p className="text-text-secondary mb-12 max-w-2xl">
            Watch the system handle satellite intermittency, circuit breaker
            transitions, and queue persistence.
          </p>
        </ScrollReveal>

        <ScrollReveal delay={0.2}>
          {!isPlaying ? (
            <button
              onClick={() => setIsPlaying(true)}
              className="w-full border border-bg-tertiary rounded-lg p-8 text-center hover:border-accent transition-colors group"
            >
              <Play size={32} className="mx-auto mb-4 text-text-muted group-hover:text-accent transition-colors" />
              <p className="text-text-secondary text-sm">
                Run terminal demo
              </p>
            </button>
          ) : (
            <Terminal lines={demoLines} typingSpeed={25} />
          )}
        </ScrollReveal>
      </div>
    </section>
  )
}
