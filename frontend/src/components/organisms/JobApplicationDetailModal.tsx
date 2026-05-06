import { useState, useEffect } from 'react'
import { Button } from '../atoms/Button'
import { type JobApplication, Stage } from '../../features/job-applications/types'
import { cn } from '../../lib/cn'

interface SaveData {
  stage: Stage
  description: string | null
}

interface JobApplicationDetailModalProps {
  application: JobApplication
  onClose: () => void
  onSave: (data: SaveData) => void
  isSaving: boolean
}

export function JobApplicationDetailModal({
  application,
  onClose,
  onSave,
  isSaving,
}: JobApplicationDetailModalProps) {
  const [stage, setStage] = useState<Stage>(application.stage)
  const [description, setDescription] = useState(application.description ?? '')
  const [copied, setCopied] = useState(false)

  useEffect(() => {
    setStage(application.stage)
    setDescription(application.description ?? '')
  }, [application.id])

  useEffect(() => {
    const handler = (e: KeyboardEvent) => {
      if (e.key === 'Escape') onClose()
    }
    window.addEventListener('keydown', handler)
    return () => window.removeEventListener('keydown', handler)
  }, [onClose])

  function handleCopy() {
    void navigator.clipboard.writeText(description)
    setCopied(true)
    setTimeout(() => setCopied(false), 2000)
  }

  function handleSave() {
    onSave({ stage, description: description || null })
  }

  const labelClass = 'text-xs font-medium uppercase tracking-wide text-text-secondary'
  const valueClass = 'mt-0.5 text-sm text-text-primary'

  return (
    <div
      className="fixed inset-0 z-50 flex items-center justify-center bg-black/50"
      onClick={onClose}
    >
      <div
        role="dialog"
        aria-modal="true"
        aria-labelledby="detail-modal-title"
        className="relative flex h-[80vh] max-h-[720px] w-[640px] flex-col rounded-lg bg-surface shadow-xl"
        onClick={(e) => e.stopPropagation()}
      >
        {/* header */}
        <div className="flex flex-none items-center justify-between border-b border-border px-6 py-4">
          <h2 id="detail-modal-title" className="text-lg font-semibold text-text-primary">
            {application.company}
            <span className="ml-2 text-base font-normal text-text-secondary">
              — {application.title}
            </span>
          </h2>
          <button
            onClick={onClose}
            aria-label="Close"
            className="rounded p-1 text-text-secondary hover:bg-surface-elevated hover:text-text-primary"
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
              <line x1="18" y1="6" x2="6" y2="18" />
              <line x1="6" y1="6" x2="18" y2="18" />
            </svg>
          </button>
        </div>

        {/* content */}
        <div className="flex flex-1 flex-col gap-5 overflow-hidden px-6 py-5">
          {/* fields grid */}
          <div className="grid flex-none grid-cols-2 gap-x-6 gap-y-4">
            <div>
              <p className={labelClass}>Company</p>
              <p className={valueClass}>{application.company}</p>
            </div>
            <div>
              <p className={labelClass}>Role</p>
              <p className={valueClass}>{application.title}</p>
            </div>
            <div>
              <p className={labelClass}>Location</p>
              <p className={cn(valueClass, !application.location && 'text-text-muted')}>
                {application.location ?? '—'}
              </p>
            </div>
            <div>
              <p className={labelClass}>URL</p>
              {application.url ? (
                <a
                  href={application.url}
                  target="_blank"
                  rel="noopener noreferrer"
                  className="mt-0.5 block break-all text-sm text-brand-primary hover:underline"
                >
                  {application.url}
                </a>
              ) : (
                <p className={cn(valueClass, 'text-text-muted')}>—</p>
              )}
            </div>
            <div>
              <p className={labelClass}>Stage</p>
              <select
                value={stage}
                onChange={(e) => setStage(e.target.value as Stage)}
                className="mt-1 rounded border border-border bg-surface-elevated px-2 py-1.5 text-sm text-text-primary focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-brand-primary"
              >
                {Stage.options.map((s) => (
                  <option key={s} value={s}>
                    {s}
                  </option>
                ))}
              </select>
            </div>
            <div>
              <p className={labelClass}>Applied</p>
              <p className={cn(valueClass, !application.appliedAt && 'text-text-muted')}>
                {application.appliedAt
                  ? new Date(application.appliedAt).toLocaleDateString()
                  : '—'}
              </p>
            </div>
            <div>
              <p className={labelClass}>Posted</p>
              <p className={cn(valueClass, !application.postedAt && 'text-text-muted')}>
                {application.postedAt
                  ? new Date(application.postedAt).toLocaleDateString()
                  : '—'}
              </p>
            </div>
            <div>
              <p className={labelClass}>Added</p>
              <p className={valueClass}>{new Date(application.createdAt).toLocaleDateString()}</p>
            </div>
          </div>

          {/* description */}
          <div className="flex min-h-0 flex-1 flex-col gap-2">
            <div className="flex items-center justify-between">
              <label className={labelClass}>Description</label>
              <button
                type="button"
                onClick={handleCopy}
                title="Copy to clipboard"
                className="flex items-center gap-1.5 rounded px-2 py-1 text-xs text-text-secondary hover:bg-surface-elevated hover:text-text-primary"
              >
                {copied ? (
                  <>
                    <svg
                      width="14"
                      height="14"
                      viewBox="0 0 24 24"
                      fill="none"
                      stroke="currentColor"
                      strokeWidth="2"
                      strokeLinecap="round"
                      strokeLinejoin="round"
                    >
                      <polyline points="20 6 9 17 4 12" />
                    </svg>
                    Copied
                  </>
                ) : (
                  <>
                    <svg
                      width="14"
                      height="14"
                      viewBox="0 0 24 24"
                      fill="none"
                      stroke="currentColor"
                      strokeWidth="2"
                      strokeLinecap="round"
                      strokeLinejoin="round"
                    >
                      <rect x="9" y="9" width="13" height="13" rx="2" ry="2" />
                      <path d="M5 15H4a2 2 0 0 1-2-2V4a2 2 0 0 1 2-2h9a2 2 0 0 1 2 2v1" />
                    </svg>
                    Copy
                  </>
                )}
              </button>
            </div>
            <textarea
              className="min-h-0 flex-1 w-full resize-none rounded border border-border bg-surface-elevated px-3 py-2 text-sm text-text-primary placeholder:text-text-muted focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-brand-primary"
              value={description}
              onChange={(e) => setDescription(e.target.value)}
              placeholder="No description"
            />
          </div>
        </div>

        {/* footer */}
        <div className="flex flex-none items-center justify-end gap-3 border-t border-border px-6 py-4">
          <Button variant="secondary" onClick={onClose} disabled={isSaving}>
            Cancel
          </Button>
          <Button onClick={handleSave} isLoading={isSaving}>
            Save
          </Button>
        </div>
      </div>
    </div>
  )
}
