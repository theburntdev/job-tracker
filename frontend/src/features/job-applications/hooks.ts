import { useMemo } from 'react'
import { useJobStore } from '../../stores/useJobStore'
import { type JobApplication } from './types'

export function useFilteredJobApplications(applications: JobApplication[]) {
  const stageFilter = useJobStore((s) => s.stageFilter)
  const sortField = useJobStore((s) => s.sortField)
  const sortDir = useJobStore((s) => s.sortDir)

  return useMemo(() => {
    const filtered = stageFilter
      ? applications.filter((app) => app.stage === stageFilter)
      : applications

    return [...filtered].sort((a, b) => {
      const aVal = a[sortField] ?? ''
      const bVal = b[sortField] ?? ''
      const cmp = aVal < bVal ? -1 : aVal > bVal ? 1 : 0
      return sortDir === 'asc' ? cmp : -cmp
    })
  }, [applications, stageFilter, sortField, sortDir])
}
