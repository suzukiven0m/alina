import { Github, ArrowRight } from 'lucide-react'
import Starfield from '../components/Starfield'

export default function Hero() {
  return (
    <section className="relative min-h-screen flex items-center justify-center overflow-hidden">
      <Starfield />

      <div className="relative z-10 max-w-6xl mx-auto px-6 text-center">
        {/* Animated ship/satellite/cloud illustration */}
        <div className="relative w-full max-w-lg mx-auto mb-12 h-64">
          <svg viewBox="0 0 600 250" className="w-full h-full" role="img" aria-label="Ship transmitting sensor data to a satellite, which relays it to a cloud server">
            <title>Ship-to-satellite data transmission illustration</title>
            {/* Ship */}
            <g transform="translate(20, 120)">
              <path
                d="M10 80 L30 80 L35 70 L160 70 L165 80 L190 80 L185 95 L15 95 Z"
                fill="#1e293b"
                stroke="#06b6d4"
                strokeWidth="1.5"
              />
              <rect x="45" y="55" width="30" height="15" fill="#1e293b" stroke="#06b6d4" strokeWidth="1" />
              <rect x="85" y="50" width="30" height="20" fill="#1e293b" stroke="#06b6d4" strokeWidth="1" />
              <rect x="125" y="55" width="25" height="15" fill="#1e293b" stroke="#06b6d4" strokeWidth="1" />
            </g>

            {/* Satellite */}
            <g transform="translate(320, 30)">
              <circle cx="0" cy="0" r="10" fill="#1e293b" stroke="#f59e0b" strokeWidth="1.5" />
              <rect x="-25" y="-4" width="15" height="8" fill="#1e293b" stroke="#f59e0b" strokeWidth="1" />
              <rect x="10" y="-4" width="15" height="8" fill="#1e293b" stroke="#f59e0b" strokeWidth="1" />
              <circle cx="0" cy="0" r="16" stroke="#f59e0b" strokeWidth="0.5" opacity="0.5" aria-hidden="true">
                <animate attributeName="r" values="16;22;16" dur="2s" repeatCount="indefinite" />
                <animate attributeName="opacity" values="0.5;0;0.5" dur="2s" repeatCount="indefinite" />
              </circle>
              <circle cx="0" cy="0" r="24" stroke="#f59e0b" strokeWidth="0.5" opacity="0.3" aria-hidden="true">
                <animate attributeName="r" values="24;30;24" dur="2s" repeatCount="indefinite" begin="0.3s" />
                <animate attributeName="opacity" values="0.3;0;0.3" dur="2s" repeatCount="indefinite" begin="0.3s" />
              </circle>
            </g>

            {/* Cloud */}
            <g transform="translate(480, 50)">
              <path
                d="M0 30 Q10 10 30 15 Q40 0 60 10 Q80 5 90 25 Q105 20 100 40 Q105 55 85 55 L10 55 Q-5 50 0 30Z"
                fill="#1e293b"
                stroke="#10b981"
                strokeWidth="1.5"
              />
              <rect x="35" y="60" width="30" height="4" fill="#10b981" opacity="0.5">
                <animate attributeName="width" values="30;40;30" dur="1.5s" repeatCount="indefinite" />
              </rect>
              <rect x="30" y="68" width="40" height="3" fill="#10b981" opacity="0.3">
                <animate attributeName="width" values="40;50;40" dur="1.5s" repeatCount="indefinite" begin="0.2s" />
              </rect>
            </g>

            {/* Data flow path */}
            <path
              id="dataPath"
              d="M 210 180 Q 280 140 340 90 Q 380 60 480 75"
              fill="none"
              stroke="#06b6d4"
              strokeWidth="1"
              strokeDasharray="4 4"
              opacity="0.4"
            />

            {/* Data packets */}
            <circle r="3" fill="#f59e0b" aria-hidden="true">
              <animateMotion dur="3s" repeatCount="indefinite" path="M 210 180 Q 280 140 340 90 Q 380 60 480 75" />
            </circle>
            <circle r="3" fill="#f59e0b" aria-hidden="true">
              <animateMotion dur="3s" repeatCount="indefinite" begin="1s" path="M 210 180 Q 280 140 340 90 Q 380 60 480 75" />
            </circle>
            <circle r="3" fill="#f59e0b" aria-hidden="true">
              <animateMotion dur="3s" repeatCount="indefinite" begin="2s" path="M 210 180 Q 280 140 340 90 Q 380 60 480 75" />
            </circle>
          </svg>
        </div>

        <h1 className="text-5xl md:text-7xl font-extrabold tracking-tight mb-6">
          Cargo Ship{' '}
          <span className="text-accent-cyan">Monitoring</span>
        </h1>

        <p className="text-xl md:text-2xl text-text-secondary font-light max-w-2xl mx-auto mb-10 leading-relaxed">
          Real-time telemetry, intelligent alerts, and resilient satellite communication for maritime fleets.
        </p>

        <div className="flex flex-col sm:flex-row gap-4 justify-center">
          <a
            href="https://github.com/suzukiven0m/alina"
            target="_blank"
            rel="noopener noreferrer"
            className="inline-flex items-center justify-center gap-2 px-8 py-4 bg-accent-amber text-bg-primary font-semibold rounded-lg hover:bg-amber-400 transition-colors"
          >
            <Github size={20} />
            View on GitHub
          </a>
          <a
            href="#architecture"
            className="inline-flex items-center justify-center gap-2 px-8 py-4 border-2 border-accent-cyan text-accent-cyan font-semibold rounded-lg hover:bg-accent-cyan hover:text-bg-primary transition-colors"
          >
            See Architecture
            <ArrowRight size={20} />
          </a>
        </div>
      </div>
    </section>
  )
}
