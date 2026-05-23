import { useEffect, useRef } from 'react'
import gsap from 'gsap'
import { ScrollTrigger } from 'gsap/ScrollTrigger'

gsap.registerPlugin(ScrollTrigger)

interface ScrollRevealOptions {
  y?: number
  duration?: number
  delay?: number
  stagger?: number
  start?: string
}

export function useScrollReveal<T extends HTMLElement>(
  options: ScrollRevealOptions = {}
) {
  const ref = useRef<T>(null)
  const {
    y = 40,
    duration = 0.8,
    delay = 0,
    stagger = 0.1,
    start = 'top 85%',
  } = options

  useEffect(() => {
    const el = ref.current
    if (!el) return

    const children = el.children.length > 0 ? el.children : [el]

    gsap.set(children, { opacity: 0, y })

    const tween = gsap.to(children, {
      opacity: 1,
      y: 0,
      duration,
      delay,
      stagger,
      ease: 'power3.out',
      scrollTrigger: {
        trigger: el,
        start,
        toggleActions: 'play none none none',
      },
    })

    return () => {
      tween.scrollTrigger?.kill()
      tween.kill()
    }
  }, [y, duration, delay, stagger, start])

  return ref
}
