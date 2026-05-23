import { useState } from 'react'
import { Copy, Check } from 'lucide-react'
import ScrollReveal from '../components/ScrollReveal'
import Terminal from '../components/Terminal'

const commands = [
  'git clone https://github.com/suzukiven0m/alina.git',
  'cd alina',
  'docker compose up',
]

export default function GettingStarted() {
  const [copied, setCopied] = useState(false)

  async function copyToClipboard() {
    await navigator.clipboard.writeText(commands.join('\n'))
    setCopied(true)
    setTimeout(() => setCopied(false), 2000)
  }

  return (
    <section className="py-24 md:py-32 relative z-10">
      <div className="max-w-4xl mx-auto px-6">
        <ScrollReveal>
          <h2 className="text-4xl md:text-5xl font-bold text-center mb-4">
            Get <span className="text-accent-cyan">Started</span>
          </h2>
          <p className="text-text-secondary text-center max-w-2xl mx-auto mb-12">
            Clone, compose, and go. Three commands to a running system.
          </p>
        </ScrollReveal>

        <ScrollReveal delay={0.2}>
          <div className="relative">
            <Terminal lines={commands} typingSpeed={40} />
            <button
              onClick={copyToClipboard}
              className="absolute top-3 right-3 p-2 bg-bg-secondary rounded-lg border border-[#30363d] hover:border-accent-cyan transition-colors"
              title="Copy to clipboard"
            >
              {copied ? (
                <Check size={16} className="text-accent-green" />
              ) : (
                <Copy size={16} className="text-text-muted" />
              )}
            </button>
          </div>
        </ScrollReveal>

        {/* Badges */}
        <ScrollReveal delay={0.4}>
          <div className="flex flex-wrap justify-center gap-4 mt-8">
            <span className="px-4 py-2 bg-bg-secondary rounded-full text-sm text-text-secondary border border-[#1e3a5f]">
              MIT License
            </span>
            <span className="px-4 py-2 bg-bg-secondary rounded-full text-sm text-text-secondary border border-[#1e3a5f]">
              .NET 10
            </span>
            <span className="px-4 py-2 bg-bg-secondary rounded-full text-sm text-text-secondary border border-[#1e3a5f]">
              Docker Ready
            </span>
          </div>
        </ScrollReveal>
      </div>
    </section>
  )
}
