import { Spinner } from '../atoms/Spinner'
import { StatusBadge } from '../molecules/StatusBadge'
import { type JobApplication } from '../../features/job-applications/types'
import { type SortField, type SortDir } from '../../stores/useJobStore'

interface JobApplicationTableProps {
  applications: JobApplication[]
  isLoading: boolean
  error: Error | null
  onSelect: (id: string) => void
  sortField: SortField
  sortDir: SortDir
  onSortChange: (field: SortField, dir: SortDir) => void
}

function SortIcon({ field, sortField, sortDir }: { field: SortField; sortField: SortField; sortDir: SortDir }) {
  if (field !== sortField) return <span className="ml-1 text-text-muted opacity-40">↕</span>
  return <span className="ml-1">{sortDir === 'asc' ? '↑' : '↓'}</span>
}

export function JobApplicationTable({
  applications,
  isLoading,
  error,
  onSelect,
  sortField,
  sortDir,
  onSortChange,
}: JobApplicationTableProps) {
  function handleSortClick(field: SortField) {
    if (field === sortField) {
      onSortChange(field, sortDir === 'asc' ? 'desc' : 'asc')
    } else {
      onSortChange(field, 'desc')
    }
  }

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
          <th className="pb-2 pr-4 text-sm font-medium text-text-secondary">Stage</th>
          <th className="pb-2 pr-4 text-sm font-medium text-text-secondary">
            <button
              className="flex cursor-pointer items-center text-text-secondary hover:text-text-primary"
              onClick={() => handleSortClick('appliedAt')}
            >
              Applied
              <SortIcon field="appliedAt" sortField={sortField} sortDir={sortDir} />
            </button>
          </th>
          <th className="pb-2 text-sm font-medium text-text-secondary">
            <button
              className="flex cursor-pointer items-center text-text-secondary hover:text-text-primary"
              onClick={() => handleSortClick('updatedAt')}
            >
              Updated
              <SortIcon field="updatedAt" sortField={sortField} sortDir={sortDir} />
            </button>
          </th>
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
            <td className="py-3 pr-4">
              <StatusBadge stage={app.stage} />
            </td>
            <td className="py-3 pr-4 text-text-secondary">
              {app.appliedAt ? new Date(app.appliedAt).toLocaleDateString() : '—'}
            </td>
            <td className="py-3 text-text-secondary">
              {new Date(app.updatedAt).toLocaleDateString()}
            </td>
          </tr>
        ))}
      </tbody>
    </table>
  )
}
