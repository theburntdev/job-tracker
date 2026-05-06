import { Link } from 'react-router'

export function NavBar() {
  return (
    <nav className="flex items-center justify-between border-b border-border bg-surface-elevated px-6 py-3">
      <Link to="/" className="text-lg font-semibold text-text-primary">
        Job Tracker
      </Link>
    </nav>
  )
}
