import { useEffect, useRef } from 'react'
import Prism from 'prismjs'
import 'prismjs/components/prism-csharp'

interface CodeBlockProps {
  code: string
  language?: string
}

export default function CodeBlock({ code, language = 'csharp' }: CodeBlockProps) {
  const ref = useRef<HTMLElement>(null)

  useEffect(() => {
    if (ref.current) {
      Prism.highlightElement(ref.current)
    }
  }, [code])

  return (
    <pre
      className="rounded-lg bg-[#1e293b] p-4 overflow-x-auto text-sm font-mono"
      aria-label={`${language} code example`}
      tabIndex={0}
    >
      <code ref={ref} className={`language-${language}`}>
        {code}
      </code>
    </pre>
  )
}
