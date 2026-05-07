import { renderHook } from '@testing-library/react'
import { useFilteredJobApplications } from './hooks'
import { useJobStore } from '../../stores/useJobStore'
import { makeJobApplication } from '../../test-utils/factories'

beforeEach(() => {
  useJobStore.setState({ stageFilter: null })
})

describe('useFilteredJobApplications', () => {
  it('returns all applications when stageFilter is null', () => {
    const apps = [
      makeJobApplication({ stage: 'Applied' }),
      makeJobApplication({ id: 'id-2', stage: 'Rejected' }),
    ]
    const { result } = renderHook(() => useFilteredJobApplications(apps))
    expect(result.current).toHaveLength(2)
  })

  it('filters to matching stage when stageFilter is set', () => {
    useJobStore.setState({ stageFilter: 'Applied' })
    const apps = [
      makeJobApplication({ stage: 'Applied' }),
      makeJobApplication({ id: 'id-2', stage: 'Rejected' }),
    ]
    const { result } = renderHook(() => useFilteredJobApplications(apps))
    expect(result.current).toHaveLength(1)
    expect(result.current[0].stage).toBe('Applied')
  })

  it('returns empty array when no applications match the filter', () => {
    useJobStore.setState({ stageFilter: 'Offer' })
    const apps = [makeJobApplication({ stage: 'Applied' })]
    const { result } = renderHook(() => useFilteredJobApplications(apps))
    expect(result.current).toHaveLength(0)
  })
})
