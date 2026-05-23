import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { Button } from '../atoms/Button'
import { ActivityType } from '../../features/activities/types'
import { Stage } from '../../features/job-applications/types'
import type { CreateActivityInput } from '../../features/activities/types'
import { cn } from '../../lib/cn'

const FormSchema = z.object({
  activityType: ActivityType,
  occurredAt: z.string().min(1, 'Required'),
  contactName: z.string().optional(),
  contactEmail: z.string().optional(),
  notes: z.string().optional(),
  newStage: z.union([Stage, z.literal('')]).optional(),
})

type FormValues = z.infer<typeof FormSchema>

interface AddActivityFormProps {
  jobApplicationId: string
  onSubmit: (input: CreateActivityInput) => void
  isSaving?: boolean
}

function todayLocalDate() {
  const d = new Date()
  return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`
}

const ACTIVITY_TYPE_LABELS: Record<ActivityType, string> = {
  Applied: 'Applied',
  PhoneScreen: 'Phone Screen',
  Interview: 'Interview',
  Offer: 'Offer',
  Rejected: 'Rejected',
  Withdrew: 'Withdrew',
  EmailedRecruiter: 'Emailed Recruiter',
  CalledRecruiter: 'Called Recruiter',
}

export function AddActivityForm({ jobApplicationId, onSubmit, isSaving = false }: AddActivityFormProps) {
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<FormValues>({
    resolver: zodResolver(FormSchema),
    defaultValues: {
      activityType: 'PhoneScreen',
      occurredAt: todayLocalDate(),
      contactName: '',
      contactEmail: '',
      notes: '',
      newStage: '',
    },
  })

  function onFormSubmit(values: FormValues) {
    onSubmit({
      jobApplicationId,
      activityType: values.activityType,
      occurredAt: new Date(`${values.occurredAt}T00:00:00`).toISOString(),
      contactName: values.contactName || null,
      contactEmail: values.contactEmail || null,
      notes: values.notes || null,
      newStage: values.newStage || null,
    })
  }

  const labelClass = 'text-xs font-medium uppercase tracking-wide text-text-secondary'
  const inputClass =
    'mt-0.5 w-full rounded border border-border bg-surface-elevated px-2.5 py-1.5 text-sm text-text-primary placeholder:text-text-muted focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-brand-primary'
  const errorClass = 'mt-0.5 text-xs text-status-rejected'

  return (
    <form onSubmit={handleSubmit(onFormSubmit)} className="flex flex-col gap-4">
      <div className="grid grid-cols-2 gap-x-6 gap-y-4">
        <div>
          <label className={labelClass} htmlFor="activity-type">
            Activity Type *
          </label>
          <select
            id="activity-type"
            {...register('activityType')}
            className={cn(inputClass, 'cursor-pointer')}
          >
            {ActivityType.options.map((type) => (
              <option key={type} value={type}>
                {ACTIVITY_TYPE_LABELS[type]}
              </option>
            ))}
          </select>
          {errors.activityType && <p className={errorClass}>{errors.activityType.message}</p>}
        </div>

        <div>
          <label className={labelClass} htmlFor="activity-date">
            Date *
          </label>
          <input
            id="activity-date"
            type="date"
            {...register('occurredAt')}
            className={inputClass}
          />
          {errors.occurredAt && <p className={errorClass}>{errors.occurredAt.message}</p>}
        </div>

        <div>
          <label className={labelClass} htmlFor="contact-name">
            Contact Name
          </label>
          <input
            id="contact-name"
            {...register('contactName')}
            className={inputClass}
            placeholder="Jane Smith"
          />
        </div>

        <div>
          <label className={labelClass} htmlFor="contact-email">
            Contact Email
          </label>
          <input
            id="contact-email"
            type="email"
            {...register('contactEmail')}
            className={inputClass}
            placeholder="jane@example.com"
          />
        </div>

        <div>
          <label className={labelClass} htmlFor="new-stage">
            Update Stage To
          </label>
          <select
            id="new-stage"
            {...register('newStage')}
            className={cn(inputClass, 'cursor-pointer')}
          >
            <option value="">— no change —</option>
            {Stage.options.map((stage) => (
              <option key={stage} value={stage}>
                {stage}
              </option>
            ))}
          </select>
        </div>
      </div>

      <div>
        <label className={labelClass} htmlFor="activity-notes">
          Notes
        </label>
        <textarea
          id="activity-notes"
          {...register('notes')}
          className="mt-0.5 w-full resize-none rounded border border-border bg-surface-elevated px-3 py-2 text-sm text-text-primary placeholder:text-text-muted focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-brand-primary"
          rows={3}
          placeholder="Add notes..."
        />
      </div>

      <div className="flex justify-end">
        <Button type="submit" isLoading={isSaving}>
          Log Activity
        </Button>
      </div>
    </form>
  )
}
