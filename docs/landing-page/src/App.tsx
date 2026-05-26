import Header from './sections/Header'
import SystemOverview from './sections/SystemOverview'
import LiveDashboard from './sections/LiveDashboard'
import Architecture from './sections/Architecture'
import Challenges from './sections/Challenges'
import TechStack from './sections/TechStack'
import GettingStarted from './sections/GettingStarted'
import Footer from './sections/Footer'

export default function App() {
  return (
    <div className="min-h-screen">
      <Header />
      <SystemOverview />
      <LiveDashboard />
      <Architecture />
      <Challenges />
      <TechStack />
      <GettingStarted />
      <Footer />
    </div>
  )
}
