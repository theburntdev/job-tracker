import { useRef, useState, useEffect } from 'react'
import { useForm, Controller } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { Button } from '../atoms/Button'
import { StatusBadge } from '../molecules/StatusBadge'
import { cn } from '../../lib/cn'
import { Stage, type CreateJobApplicationInput } from '../../features/job-applications/types'

const FormSchema = z.object({
  title: z.string().min(1, 'Required'),
  company: z.string().min(1, 'Required'),
  location: z.string().optional(),
  url: z.string().optional(),
  description: z.string().optional(),
  stage: Stage,
  appliedAt: z.string(),
})

type FormValues = z.infer<typeof FormSchema>

interface CreateJobApplicationModalProps {
  onClose: () => void
  onSave: (data: CreateJobApplicationInput) => void
  isSaving: boolean
}

function todayLocalDate() {
  const d = new Date()
  return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`
}

export function CreateJobApplicationModal({
  onClose,
  onSave,
  isSaving,
}: CreateJobApplicationModalProps) {
  const [stageOpen, setStageOpen] = useState(false)
  const stageRef = useRef<HTMLDivElement>(null)

  const {
    register,
    handleSubmit,
    control,
    formState: { errors },
  } = useForm<FormValues>({
    resolver: zodResolver(FormSchema),
    defaultValues: {
      title: '',
      company: '',
      location: '',
      url: '',
      description: '',
      stage: 'Applied',
      appliedAt: todayLocalDate(),
    },
  })

  useEffect(() => {
    const handler = (e: KeyboardEvent) => {
      if (e.key === 'Escape') onClose()
    }
    window.addEventListener('keydown', handler)
    return () => window.removeEventListener('keydown', handler)
  }, [onClose])

  useEffect(() => {
    if (!stageOpen) return
    const handler = (e: MouseEvent) => {
      if (stageRef.current && !stageRef.current.contains(e.target as Node)) {
        setStageOpen(false)
      }
    }
    document.addEventListener('mousedown', handler)
    return () => document.removeEventListener('mousedown', handler)
  }, [stageOpen])

  function onSubmit(values: FormValues) {
    onSave({
      title: values.title,
      company: values.company,
      location: values.location || undefined,
      url: values.url || undefined,
      description: values.description || undefined,
      stage: values.stage,
      appliedAt: values.appliedAt ? new Date(`${values.appliedAt}T00:00:00`).toISOString() : null,
    })
  }

  const labelClass = 'text-xs font-medium uppercase tracking-wide text-text-secondary'
  const inputClass =
    'mt-0.5 w-full rounded border border-border bg-surface-elevated px-2.5 py-1.5 text-sm text-text-primary placeholder:text-text-muted focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-brand-primary'
  const errorClass = 'mt-0.5 text-xs text-status-rejected'

  return (
    <div
      className="fixed inset-0 z-50 flex items-center justify-center bg-black/50"
      onClick={onClose}
    >
      <div
        role="dialog"
        aria-modal="true"
        aria-labelledby="create-modal-title"
        className="relative flex h-[80vh] max-h-[720px] w-[640px] flex-col rounded-lg bg-surface shadow-xl"
        onClick={(e) => e.stopPropagation()}
      >
        {/* header */}
        <div className="flex flex-none items-center justify-between border-b border-border px-6 py-4">
          <h2 id="create-modal-title" className="text-lg font-semibold text-text-primary">
            Add Application
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
        <form
          id="create-form"
          onSubmit={handleSubmit(onSubmit)}
          className="flex flex-1 flex-col gap-5 overflow-y-auto px-6 py-5"
        >
          <div className="grid flex-none grid-cols-2 gap-x-6 gap-y-4">
            <div>
              <label className={labelClass} htmlFor="create-company">
                Company *
              </label>
              <input
                id="create-company"
                {...register('company')}
                className={inputClass}
                placeholder="Acme Corp"
              />
              {errors.company && <p className={errorClass}>{errors.company.message}</p>}
            </div>
            <div>
              <label className={labelClass} htmlFor="create-title">
                Role *
              </label>
              <input
                id="create-title"
                {...register('title')}
                className={inputClass}
                placeholder="Software Engineer"
              />
              {errors.title && <p className={errorClass}>{errors.title.message}</p>}
            </div>
            <div>
              <label className={labelClass} htmlFor="create-location">
                Location
              </label>
              <input
                id="create-location"
                {...register('location')}
                className={inputClass}
                placeholder="Remote"
              />
            </div>
            <div>
              <label className={labelClass} htmlFor="create-url">
                URL
              </label>
              <input
                id="create-url"
                {...register('url')}
                className={inputClass}
                placeholder="https://..."
              />
            </div>
            <div>
              <p className={labelClass}>Stage</p>
              <Controller
                control={control}
                name="stage"
                render={({ field }) => (
                  <div ref={stageRef} className="relative mt-1">
                    <button
                      type="button"
                      onClick={() => setStageOpen((o) => !o)}
                      className="flex items-center gap-1.5 rounded px-2 py-1.5 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-brand-primary"
                    >
                      <StatusBadge stage={field.value} />
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
                    {stageOpen && (
                      <div className="absolute left-0 top-full z-10 mt-1 flex flex-col gap-0.5 rounded bg-surface-elevated p-1 shadow-lg">
                        {Stage.options.map((s) => (
                          <button
                            key={s}
                            type="button"
                            onClick={() => {
                              field.onChange(s)
                              setStageOpen(false)
                            }}
                            className={cn(
                              'rounded px-2 py-1 text-left hover:bg-surface',
                              s === field.value && 'bg-surface',
                            )}
                          >
                            <StatusBadge stage={s as Stage} />
                          </button>
                        ))}
                      </div>
                    )}
                  </div>
                )}
              />
            </div>
            <div>
              <label className={labelClass} htmlFor="create-appliedAt">
                Applied
              </label>
              <input
                id="create-appliedAt"
                type="date"
                {...register('appliedAt')}
                className={inputClass}
              />
            </div>
          </div>

          {/* description */}
          <div className="flex min-h-0 flex-1 flex-col gap-2">
            <label className={labelClass} htmlFor="create-description">
              Description
            </label>
            <textarea
              id="create-description"
              {...register('description')}
              className="min-h-0 flex-1 w-full resize-none rounded border border-border bg-surface-elevated px-3 py-2 text-sm text-text-primary placeholder:text-text-muted focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-brand-primary"
              placeholder="Paste job description..."
            />
          </div>
        </form>

        {/* footer */}
        <div className="flex flex-none items-center justify-end gap-3 border-t border-border px-6 py-4">
          <Button variant="secondary" onClick={onClose} disabled={isSaving}>
            Cancel
          </Button>
          <Button type="submit" form="create-form" isLoading={isSaving}>
            Add
          </Button>
        </div>
      </div>
    </div>
  )
}
