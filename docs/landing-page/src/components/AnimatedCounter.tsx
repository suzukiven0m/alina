import { useEffect, useRef, useState } from 'react'

interface AnimatedCounterProps {
  end: number
  suffix?: string
  prefix?: string
  duration?: number
  label: string
}

export default function AnimatedCounter({
  end,
  suffix = '',
  prefix = '',
  duration = 2000,
  label,
}: AnimatedCounterProps) {
  const [count, setCount] = useState(0)
  const ref = useRef<HTMLDivElement>(null)
  const hasAnimated = useRef(false)
  const rafId = useRef<number>(0)

  useEffect(() => {
    const el = ref.current
    if (!el) return

    const observer = new IntersectionObserver(
      (entries) => {
        if (entries[0].isIntersecting && !hasAnimated.current) {
          hasAnimated.current = true
          const startTime = performance.now()

          function animate(currentTime: number) {
            const elapsed = currentTime - startTime
            const progress = Math.min(elapsed / duration, 1)
            const eased = 1 - Math.pow(1 - progress, 3)
            setCount(Math.floor(eased * end))

            if (progress < 1) {
              rafId.current = requestAnimationFrame(animate)
            }
          }

          rafId.current = requestAnimationFrame(animate)
        }
      },
      { threshold: 0.5 }
    )

    observer.observe(el)
    return () => {
      observer.disconnect()
      cancelAnimationFrame(rafId.current)
    }
  }, [end, duration])

  return (
    <div ref={ref} className="text-center">
      <div className="text-4xl md:text-5xl font-semibold text-accent font-mono">
        {prefix}
        {count}
        {suffix}
      </div>
      <div className="mt-2 text-text-secondary text-sm uppercase tracking-wider">
        {label}
      </div>
    </div>
  )
}
