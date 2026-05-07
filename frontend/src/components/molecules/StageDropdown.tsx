import { useRef, useEffect, useState } from 'react'
import { Stage } from '../../features/job-applications/types'
import { StatusBadge } from './StatusBadge'
import { cn } from '../../lib/cn'

interface StageDropdownProps {
  stage: Stage
  onChange: (stage: Stage) => void
  disabled?: boolean
}

export function StageDropdown({ stage, onChange, disabled = false }: StageDropdownProps) {
  const [open, setOpen] = useState(false)
  const ref = useRef<HTMLDivElement>(null)

  useEffect(() => {
    if (!open) return
    const handler = (e: MouseEvent) => {
      if (ref.current && !ref.current.contains(e.target as Node)) setOpen(false)
    }
    document.addEventListener('mousedown', handler)
    return () => document.removeEventListener('mousedown', handler)
  }, [open])

  return (
    <div ref={ref} className="relative">
      <button
        type="button"
        disabled={disabled}
        onClick={(e) => { e.stopPropagation(); setOpen((o) => !o) }}
        className="flex items-center gap-1.5 rounded px-2 py-1.5 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-brand-primary disabled:opacity-50"
      >
        <StatusBadge stage={stage} />
        <svg
          width="12"
          height="12"
          viewBox="0 0 24 24"
          fill="none"
          stroke="currentColor"
          strokeWidth="2"
          strokeLinecap="round"
          strokeLinejoin="round"
          className="text-text-secondary"
        >
          <polyline points="6 9 12 15 18 9" />
        </svg>
      </button>
      {open && (
        <div className="absolute left-0 top-full z-10 mt-1 flex flex-col gap-0.5 rounded bg-surface-elevated p-1 shadow-lg">
          {Stage.options.map((s) => (
            <button
              key={s}
              type="button"
              onClick={(e) => { e.stopPropagation(); onChange(s as Stage); setOpen(false) }}
              className={cn('rounded px-2 py-1 text-left hover:bg-surface', s === stage && 'bg-surface')}
            >
              <StatusBadge stage={s as Stage} />
            </button>
          ))}
        </div>
      )}
    </div>
  )
}