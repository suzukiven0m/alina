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
    <section className="py-20 md:py-28 border-b border-bg-tertiary">
      <div className="max-w-3xl mx-auto px-6">
        <ScrollReveal>
          <p className="text-text-muted text-sm font-mono mb-3">Setup</p>
          <h2 className="text-3xl md:text-4xl font-semibold mb-4">
            Get started
          </h2>
          <p className="text-text-secondary mb-12 max-w-2xl">
            Clone, compose, and go. Three commands to a running system.
          </p>
        </ScrollReveal>

        <ScrollReveal delay={0.2}>
          <div className="relative">
            <Terminal lines={commands} typingSpeed={40} />
            <button
              onClick={copyToClipboard}
              className="absolute top-3 right-3 p-2 bg-bg-secondary rounded-lg border border-bg-tertiary hover:border-accent transition-colors"
              title="Copy to clipboard"
              aria-label="Copy commands to clipboard"
            >
              {copied ? (
                <Check size={16} className="text-accent-green" />
              ) : (
                <Copy size={16} className="text-text-muted" />
              )}
            </button>
          </div>
        </ScrollReveal>

        <ScrollReveal delay={0.4}>
          <div className="flex flex-wrap gap-3 mt-8">
            <span className="px-3 py-1.5 bg-bg-secondary rounded text-sm text-text-secondary border border-bg-tertiary font-mono">
              MIT License
            </span>
            <span className="px-3 py-1.5 bg-bg-secondary rounded text-sm text-text-secondary border border-bg-tertiary font-mono">
              .NET 10
            </span>
            <span className="px-3 py-1.5 bg-bg-secondary rounded text-sm text-text-secondary border border-bg-tertiary font-mono">
              Docker Ready
            </span>
          </div>
        </ScrollReveal>
      </div>
    </section>
  )
}
