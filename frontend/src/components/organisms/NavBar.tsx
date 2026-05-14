import { Link } from 'react-router'
import logo from '../../assets/JobTracker.png'

export function NavBar() {
  return (
    <nav className="flex items-center border-b border-border bg-surface-elevated px-6 py-3">
      <img src={logo} alt="Job Application Tracker" className="h-10 w-10" />
      <Link to="/" className="text-lg font-semibold text-text-primary px-6">
        Job Application Tracker
      </Link>
    </nav>
  )
}
