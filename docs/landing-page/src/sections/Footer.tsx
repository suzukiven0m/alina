import { Github, Star, Coffee } from 'lucide-react'

export default function Footer() {
  return (
    <footer className="relative py-16 z-10 overflow-hidden">
      {/* Wave animation at top of footer */}
      <div className="absolute top-0 left-0 w-full overflow-hidden leading-none">
        <svg
          className="relative block w-[200%] h-12 wave-animate"
          viewBox="0 0 1200 120"
          preserveAspectRatio="none"
        >
          <path
            d="M0,60 C150,120 350,0 600,60 C850,120 1050,0 1200,60 L1200,120 L0,120 Z"
            fill="#111840"
            opacity="0.5"
          />
        </svg>
      </div>

      <div className="max-w-6xl mx-auto px-6 pt-8">
        <div className="flex flex-col md:flex-row items-center justify-between gap-6">
          <div className="text-center md:text-left">
            <h3 className="text-xl font-bold text-text-primary mb-2">
              Cargo Ship Monitoring
            </h3>
            <p className="text-text-muted text-sm flex items-center gap-1 justify-center md:justify-start">
              Built with .NET 10, SQLite, and a lot of
              <Coffee size={14} className="text-accent-amber" />
            </p>
          </div>

          <a
            href="https://github.com/suzukiven0m/alina"
            target="_blank"
            rel="noopener noreferrer"
            className="inline-flex items-center gap-2 px-6 py-3 bg-bg-secondary rounded-lg border border-[#1e3a5f] hover:border-accent-cyan transition-colors"
          >
            <Github size={20} />
            <span className="text-text-secondary">View on GitHub</span>
            <span className="flex items-center gap-1 text-accent-amber">
              <Star size={14} />
            </span>
          </a>
        </div>

        <div className="mt-8 pt-8 border-t border-[#1e3a5f] text-center text-text-muted text-sm">
          Open source under the MIT License. Designed for hiring managers and recruiters.
        </div>
      </div>
    </footer>
  )
}
