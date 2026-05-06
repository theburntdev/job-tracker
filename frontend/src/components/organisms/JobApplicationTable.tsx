import { Spinner } from '../atoms/Spinner'
import { StatusBadge } from '../molecules/StatusBadge'
import { type JobApplication } from '../../features/job-applications/types'

interface JobApplicationTableProps {
  applications: JobApplication[]
  isLoading: boolean
  error: Error | null
  onSelect: (id: string) => void
}

export function JobApplicationTable({
  applications,
  isLoading,
  error,
  onSelect,
}: JobApplicationTableProps) {
  if (isLoading) {
    return (
      <div className="flex justify-center p-8">
        <Spinner className="h-6 w-6 text-text-muted" />
      </div>
    )
  }

  if (error) {
    return (
      <p role="alert" className="p-4 text-status-rejected">
        {error.message}
      </p>
    )
  }

  if (applications.length === 0) {
    return (
      <p className="p-8 text-center text-text-muted">
        No applications yet. Add one to get started.
      </p>
    )
  }

  return (
    <table className="w-full border-collapse">
      <thead>
        <tr className="border-b border-border text-left">
          <th className="pb-2 pr-4 text-sm font-medium text-text-secondary">Company</th>
          <th className="pb-2 pr-4 text-sm font-medium text-text-secondary">Role</th>
          <th className="pb-2 text-sm font-medium text-text-secondary">Stage</th>
        </tr>
      </thead>
      <tbody>
        {applications.map((app) => (
          <tr
            key={app.id}
            className="cursor-pointer border-b border-border transition-colors hover:bg-surface"
            onClick={() => onSelect(app.id)}
          >
            <td className="py-3 pr-4 text-text-primary">{app.company}</td>
            <td className="py-3 pr-4 text-text-secondary">{app.title}</td>
            <td className="py-3">
              <StatusBadge stage={app.stage} />
            </td>
          </tr>
        ))}
      </tbody>
    </table>
  )
}
