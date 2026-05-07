import { useJobStore } from './useJobStore'

beforeEach(() => {
  useJobStore.setState({
    selectedJobId: null,
    stageFilter: null,
    sortField: 'appliedAt',
    sortDir: 'desc',
  })
})

describe('useJobStore', () => {
  describe('selectJob', () => {
    it('sets selectedJobId to given id', () => {
      useJobStore.getState().selectJob('abc-123')
      expect(useJobStore.getState().selectedJobId).toBe('abc-123')
    })

    it('clears selectedJobId when called with null', () => {
      useJobStore.setState({ selectedJobId: 'abc-123' })
      useJobStore.getState().selectJob(null)
      expect(useJobStore.getState().selectedJobId).toBeNull()
    })
  })

  describe('setStageFilter', () => {
    it('sets stageFilter to given stage', () => {
      useJobStore.getState().setStageFilter('Applied')
      expect(useJobStore.getState().stageFilter).toBe('Applied')
    })

    it('clears stageFilter when called with null', () => {
      useJobStore.setState({ stageFilter: 'Rejected' })
      useJobStore.getState().setStageFilter(null)
      expect(useJobStore.getState().stageFilter).toBeNull()
    })
  })

  describe('setSort', () => {
    it('sets sortField and sortDir', () => {
      useJobStore.getState().setSort('updatedAt', 'asc')
      expect(useJobStore.getState().sortField).toBe('updatedAt')
      expect(useJobStore.getState().sortDir).toBe('asc')
    })
  })
})
