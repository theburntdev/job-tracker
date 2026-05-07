import { useUiStore } from './useUiStore'

beforeEach(() => {
  useUiStore.setState({ isCreateModalOpen: false })
})

describe('useUiStore', () => {
  describe('openCreateModal', () => {
    it('sets isCreateModalOpen to true', () => {
      useUiStore.getState().openCreateModal()
      expect(useUiStore.getState().isCreateModalOpen).toBe(true)
    })
  })

  describe('closeCreateModal', () => {
    it('sets isCreateModalOpen to false when modal is open', () => {
      useUiStore.setState({ isCreateModalOpen: true })
      useUiStore.getState().closeCreateModal()
      expect(useUiStore.getState().isCreateModalOpen).toBe(false)
    })
  })
})
