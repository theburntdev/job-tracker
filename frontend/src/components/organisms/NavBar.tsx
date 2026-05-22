import { NavLink, Link } from 'react-router'
import logo from '../../assets/JobTracker.png'
import { cn } from '../../lib/cn'

export function NavBar() {
  return (
    <nav className="flex items-center border-b border-border bg-surface-elevated px-6 py-3">
      <img src={logo} alt="Job Application Tracker" className="h-10 w-10" />
      <Link to="/" className="text-lg font-semibold text-text-primary px-6">
        Job Application Tracker
      </Link>
      <div className="flex gap-1">
        <NavLink
          to="/"
          end
          className={({ isActive }) =>
            cn(
              'rounded-md px-3 py-1.5 text-sm font-medium transition-colors',
              isActive
                ? 'bg-brand-primary/10 text-brand-primary'
                : 'text-text-secondary hover:text-text-primary',
            )
          }
        >
          Applications
        </NavLink>
        <NavLink
          to="/activity"
          className={({ isActive }) =>
            cn(
              'rounded-md px-3 py-1.5 text-sm font-medium transition-colors',
              isActive
                ? 'bg-brand-primary/10 text-brand-primary'
                : 'text-text-secondary hover:text-text-primary',
            )
          }
        >
          Activity Feed
        </NavLink>
      </div>
    </nav>
  )
}
