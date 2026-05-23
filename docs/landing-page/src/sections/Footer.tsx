import { Github } from 'lucide-react'

export default function Footer() {
  return (
    <footer className="py-12 border-t border-bg-tertiary">
      <div className="max-w-3xl mx-auto px-6">
        <div className="flex flex-col md:flex-row items-center justify-between gap-4">
          <div>
            <span className="text-text-primary font-medium">
              Cargo Ship Monitoring
            </span>
            <span className="text-text-muted text-sm ml-2">
              MIT License
            </span>
          </div>

          <a
            href="https://github.com/suzukiven0m/alina"
            target="_blank"
            rel="noopener noreferrer"
            className="inline-flex items-center gap-2 text-text-muted hover:text-text-primary transition-colors text-sm"
          >
            <Github size={16} />
            suzukiven0m/alina
          </a>
        </div>
      </div>
    </footer>
  )
}
