import { useMemo } from 'react'
import { useJobStore } from '../../stores/useJobStore'
import { type JobApplication } from './types'

export function useFilteredJobApplications(applications: JobApplication[]) {
  const stageFilter = useJobStore((s) => s.stageFilter)

  return useMemo(() => {
    if (!stageFilter) return applications
    return applications.filter((app) => app.stage === stageFilter)
  }, [applications, stageFilter])
}
