import { useState } from 'react'
import { Play, Terminal as TerminalIcon } from 'lucide-react'
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
    <section className="py-24 md:py-32 relative z-10">
      <div className="max-w-6xl mx-auto px-6">
        <ScrollReveal>
          <h2 className="text-4xl md:text-5xl font-bold text-center mb-4">
            See It <span className="text-accent-cyan">In Action</span>
          </h2>
          <p className="text-text-secondary text-center max-w-2xl mx-auto mb-12">
            Watch the system handle satellite intermittency, circuit breaker transitions, and queue persistence.
          </p>
        </ScrollReveal>

        <ScrollReveal delay={0.2}>
          {!isPlaying ? (
            <div className="relative bg-bg-secondary rounded-xl border border-[#1e3a5f] overflow-hidden">
              <div className="aspect-video flex flex-col items-center justify-center">
                <TerminalIcon size={48} className="text-accent-cyan mb-4" />
                <p className="text-text-secondary mb-6">
                  Terminal recording of the full system running
                </p>
                <button
                  onClick={() => setIsPlaying(true)}
                  className="inline-flex items-center gap-2 px-6 py-3 bg-accent-amber text-bg-primary font-semibold rounded-lg hover:bg-amber-400 transition-colors"
                >
                  <Play size={20} />
                  Run Demo
                </button>
              </div>
            </div>
          ) : (
            <Terminal
              lines={demoLines}
              typingSpeed={25}
              className="min-h-[400px]"
            />
          )}
        </ScrollReveal>
      </div>
    </section>
  )
}
