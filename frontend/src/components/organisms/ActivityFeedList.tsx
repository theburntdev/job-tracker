import type { Activity } from '../../features/activities/types'
import { Spinner } from '../atoms/Spinner'

interface Props {
  activities: Activity[]
  isLoading: boolean
  error: Error | null
}

const ACTIVITY_LABELS: Record<Activity['activityType'], string> = {
  Applied: 'Applied',
  PhoneScreen: 'Phone Screen',
  Interview: 'Interview',
  Offer: 'Offer',
  Rejected: 'Rejected',
  Withdrew: 'Withdrew',
  EmailedRecruiter: 'Emailed Recruiter',
  CalledRecruiter: 'Called Recruiter',
}

export function ActivityFeedList({ activities, isLoading, error }: Props) {
  if (isLoading) {
    return (
      <div className="flex justify-center py-12">
        <Spinner />
      </div>
    )
  }

  if (error) {
    return (
      <div className="rounded-md bg-status-rejected/10 p-4 text-sm text-status-rejected">
        Failed to load activity feed.
      </div>
    )
  }

  if (activities.length === 0) {
    return (
      <p className="py-12 text-center text-sm text-text-secondary">
        No activity recorded yet.
      </p>
    )
  }

  return (
    <ul className="divide-y divide-border">
      {activities.map((activity) => (
        <li key={activity.id} className="flex flex-col gap-1 py-4">
          <div className="flex items-center justify-between">
            <span className="text-sm font-medium text-text-primary">
              {ACTIVITY_LABELS[activity.activityType]}
            </span>
            <time className="text-xs text-text-secondary">
              {new Date(activity.occurredAt).toLocaleDateString()}
            </time>
          </div>
          {(activity.jobTitle || activity.company) && (
            <p className="text-sm text-text-secondary">
              {[activity.jobTitle, activity.company].filter(Boolean).join(' · ')}
            </p>
          )}
          {activity.contactName && (
            <p className="text-xs text-text-secondary">
              {activity.contactName}
              {activity.contactEmail ? ` · ${activity.contactEmail}` : ''}
            </p>
          )}
          {activity.notes && (
            <p className="text-xs text-text-secondary">{activity.notes}</p>
          )}
        </li>
      ))}
    </ul>
  )
}
