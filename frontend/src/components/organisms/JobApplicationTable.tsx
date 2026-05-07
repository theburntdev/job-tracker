import { Spinner } from '../atoms/Spinner'
import { StageDropdown } from '../molecules/StageDropdown'
import { type JobApplication, type Stage } from '../../features/job-applications/types'
import { type SortField, type SortDir } from '../../stores/useJobStore'

interface JobApplicationTableProps {
  applications: JobApplication[]
  isLoading: boolean
  error: Error | null
  onSelect: (id: string) => void
  onStageChange: (app: JobApplication, stage: Stage) => void
  isUpdatingStage?: boolean
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
  onStageChange,
  isUpdatingStage,
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
          <th className="w-8 pb-2" />
          <th className="pb-2 pr-4 text-sm font-medium text-text-secondary">Company</th>
          <th className="pb-2 pr-4 text-sm font-medium text-text-secondary">Role</th>
          <th className="pb-2 pr-4 text-sm font-medium text-text-secondary">Stage</th>
          <th className="pb-2 pr-4 text-sm font-medium text-text-secondary">
            <button
              aria-label="Sort by Applied date"
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
            className="border-b border-border transition-colors hover:bg-surface"
          >
            <td className="py-3 pr-2">
              <button
                type="button"
                onClick={() => onSelect(app.id)}
                aria-label={`Open details for ${app.company} – ${app.title}`}
                className="rounded p-1 text-text-muted hover:bg-surface-elevated hover:text-text-primary"
              >
                <svg
                  width="16"
                  height="16"
                  viewBox="0 0 24 24"
                  fill="none"
                  stroke="currentColor"
                  strokeWidth="2"
                  strokeLinecap="round"
                  strokeLinejoin="round"
                >
                  <path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z" />
                  <circle cx="12" cy="12" r="3" />
                </svg>
              </button>
            </td>
            <td className="py-3 pr-4 text-text-primary">{app.company}</td>
            <td className="py-3 pr-4 text-text-secondary">
              {app.url?.trim() ? (
                <a
                  href={app.url}
                  target="_blank"
                  rel="noopener noreferrer"
                  className="text-brand-primary hover:underline"
                >
                  {app.title}
                </a>
              ) : (
                app.title
              )}
            </td>
            <td className="py-3 pr-4">
              <StageDropdown
                stage={app.stage}
                onChange={(s) => onStageChange(app, s)}
                disabled={isUpdatingStage}
              />
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