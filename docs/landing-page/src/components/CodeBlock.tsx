interface CodeBlockProps {
  code: string
}

export default function CodeBlock({ code }: CodeBlockProps) {
  return (
    <pre aria-label="code example" tabIndex={0}>
      <code>{code}</code>
    </pre>
  )
}
