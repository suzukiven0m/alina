import { useScrollReveal } from '../hooks/useScrollReveal'

interface ScrollRevealProps {
  children: React.ReactNode
  className?: string
  y?: number
  duration?: number
  delay?: number
  stagger?: number
  start?: string
}

export default function ScrollReveal({
  children,
  className = '',
  y = 40,
  duration = 0.8,
  delay = 0,
  stagger = 0.1,
  start,
}: ScrollRevealProps) {
  const ref = useScrollReveal<HTMLDivElement>({ y, duration, delay, stagger, start })
  return (
    <div ref={ref} className={className}>
      {children}
    </div>
  )
}
