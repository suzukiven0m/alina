import { useEffect, useRef } from 'react'

interface Particle {
  x: number
  y: number
  size: number
  speedX: number
  speedY: number
  opacity: number
}

export default function Starfield() {
  const canvasRef = useRef<HTMLCanvasElement>(null)

  useEffect(() => {
    const canvas = canvasRef.current
    if (!canvas) return
    const ctx = canvas.getContext('2d')
    if (!ctx) return

    let animId: number
    const particles: Particle[] = []
    const PARTICLE_COUNT = 200

    function resize() {
      canvas!.width = window.innerWidth
      canvas!.height = window.innerHeight
    }

    function initParticles() {
      particles.length = 0
      for (let i = 0; i < PARTICLE_COUNT; i++) {
        particles.push({
          x: Math.random() * canvas!.width,
          y: Math.random() * canvas!.height,
          size: Math.random() * 1.5 + 0.5,
          speedX: (Math.random() - 0.5) * 0.2,
          speedY: (Math.random() - 0.5) * 0.2,
          opacity: Math.random() * 0.5 + 0.2,
        })
      }
    }

    function draw() {
      ctx!.clearRect(0, 0, canvas!.width, canvas!.height)
      for (const p of particles) {
        ctx!.beginPath()
        ctx!.arc(p.x, p.y, p.size, 0, Math.PI * 2)
        ctx!.fillStyle = `rgba(255, 255, 255, ${p.opacity})`
        ctx!.fill()

        p.x += p.speedX
        p.y += p.speedY

        if (p.x < 0) p.x = canvas!.width
        if (p.x > canvas!.width) p.x = 0
        if (p.y < 0) p.y = canvas!.height
        if (p.y > canvas!.height) p.y = 0
      }
      animId = requestAnimationFrame(draw)
    }

    resize()
    initParticles()
    draw()

    const handleResize = () => {
      resize()
      initParticles()
    }
    window.addEventListener('resize', handleResize)

    return () => {
      cancelAnimationFrame(animId)
      window.removeEventListener('resize', handleResize)
    }
  }, [])

  return <canvas ref={canvasRef} id="starfield-canvas" />
}
