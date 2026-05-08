import { Link } from 'react-router'
import logo from '../../assets/KumonAngryTears.png'

export function NavBar() {
  return (
    <nav className="flex items-center border-b border-border bg-surface-elevated px-6 py-3">
      <img src={logo} alt="Job Application Tracker" className="h-8 w-8" />
      <Link to="/" className="text-lg font-semibold text-text-primary px-6">
        Job Application Tracker
      </Link>
    </nav>
  )
}
