import { useMemo } from 'react'
import { useJobStore } from '../../stores/useJobStore'
import { type JobApplication } from './types'

export function useFilteredJobApplications(applications: JobApplication[]) {
  const stageFilter = useJobStore((s) => s.stageFilter)

  return useMemo(
    () =>
      stageFilter ? applications.filter((app) => app.stage === stageFilter) : applications,
    [applications, stageFilter],
  )
}
