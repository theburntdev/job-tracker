import { Badge } from '../atoms/Badge'
import { type Stage } from '../../features/job-applications/types'

const STAGE_VARIANT: Record<Stage, 'default' | 'info' | 'warning' | 'success' | 'error'> = {
  Applied: 'info',
  Screening: 'default',
  Interviewing: 'warning',
  Offer: 'success',
  Rejected: 'error',
  Withdrawn: 'default',
}

interface StatusBadgeProps {
  stage: Stage
}

export function StatusBadge({ stage }: StatusBadgeProps) {
  return <Badge variant={STAGE_VARIANT[stage]}>{stage}</Badge>
}
