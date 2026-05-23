import Hero from './sections/Hero'
import Architecture from './sections/Architecture'
import Challenges from './sections/Challenges'
import TechStack from './sections/TechStack'
import LiveMetrics from './sections/LiveMetrics'
import CodeHighlights from './sections/CodeHighlights'
import Demo from './sections/Demo'
import GettingStarted from './sections/GettingStarted'
import Footer from './sections/Footer'

export default function App() {
  return (
    <div className="relative min-h-screen">
      <Hero />
      <Architecture />
      <Challenges />
      <TechStack />
      <LiveMetrics />
      <CodeHighlights />
      <Demo />
      <GettingStarted />
      <Footer />
    </div>
  )
}
