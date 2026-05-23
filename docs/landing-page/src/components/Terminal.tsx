import { useEffect, useState } from 'react'

interface TerminalProps {
  lines: string[]
  typingSpeed?: number
  className?: string
}

export default function Terminal({ lines, typingSpeed = 30, className = '' }: TerminalProps) {
  const [displayedLines, setDisplayedLines] = useState<string[]>([])
  const [currentLine, setCurrentLine] = useState(0)
  const [currentChar, setCurrentChar] = useState(0)

  useEffect(() => {
    setDisplayedLines([])
    setCurrentLine(0)
    setCurrentChar(0)
  }, [lines])

  useEffect(() => {
    if (currentLine >= lines.length) return

    const line = lines[currentLine]
    if (currentChar < line.length) {
      const timer = setTimeout(() => {
        setCurrentChar((c) => c + 1)
      }, typingSpeed)
      return () => clearTimeout(timer)
    } else {
      const timer = setTimeout(() => {
        setDisplayedLines((prev) => [...prev, line])
        setCurrentLine((l) => l + 1)
        setCurrentChar(0)
      }, 300)
      return () => clearTimeout(timer)
    }
  }, [currentLine, currentChar, lines, typingSpeed])

  return (
    <div className={`rounded-lg bg-[#0d1117] border border-[#30363d] overflow-hidden ${className}`}>
      <div className="flex items-center gap-2 px-4 py-2 bg-[#161b22] border-b border-[#30363d]">
        <div className="w-3 h-3 rounded-full bg-accent-red" />
        <div className="w-3 h-3 rounded-full bg-accent-amber" />
        <div className="w-3 h-3 rounded-full bg-accent-green" />
        <span className="ml-2 text-xs text-text-muted">bash</span>
      </div>
      <div className="p-4 font-mono text-sm text-text-secondary">
        {displayedLines.map((line, i) => (
          <div key={i} className="leading-relaxed">
            <span className="text-accent-green">$</span> {line}
          </div>
        ))}
        {currentLine < lines.length && (
          <div className="leading-relaxed">
            <span className="text-accent-green">$</span>{' '}
            {lines[currentLine].slice(0, currentChar)}
            <span className="animate-pulse text-accent-cyan">|</span>
          </div>
        )}
      </div>
    </div>
  )
}
