import { useEffect } from 'react'
import type { CreateActivityInput } from '../../features/activities/types'
import { AddActivityForm } from './AddActivityForm'

interface LogActivityModalProps {
  jobApplicationId: string
  onClose: () => void
  onSubmit: (input: CreateActivityInput) => void
  isSaving?: boolean
}

export function LogActivityModal({ jobApplicationId, onClose, onSubmit, isSaving = false }: LogActivityModalProps) {
  useEffect(() => {
    const handler = (e: KeyboardEvent) => { if (e.key === 'Escape') onClose(); };
    window.addEventListener('keydown', handler);
    return () => window.removeEventListener('keydown', handler);
  }, [onClose])

  return (
    <div
      className="fixed inset-0 z-50 flex items-center justify-center bg-black/50"
      onClick={onClose}
    >
      <div
        role="dialog"
        aria-modal="true"
        aria-labelledby="log-activity-modal-title"
        className="w-[560px] rounded-lg bg-surface p-6 shadow-xl"
        onClick={(e) => e.stopPropagation()}
      >
        <div className="mb-5 flex items-center justify-between">
          <h2 id="log-activity-modal-title" className="text-lg font-semibold text-text-primary">
            Log Activity
          </h2>
          <button
            onClick={onClose}
            aria-label="Close"
            className="rounded p-1 text-text-secondary hover:bg-surface-elevated hover:text-text-primary"
          >
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
              <line x1="18" y1="6" x2="6" y2="18" />
              <line x1="6" y1="6" x2="18" y2="18" />
            </svg>
          </button>
        </div>
        <AddActivityForm
          jobApplicationId={jobApplicationId}
          onSubmit={onSubmit}
          isSaving={isSaving}
        />
      </div>
    </div>
  )
}
