import { create } from 'zustand'

export type SortField = 'appliedAt' | 'updatedAt'
export type SortDir = 'asc' | 'desc'

interface JobState {
  selectedJobId: string | null
  stageFilter: string | null
  sortField: SortField
  sortDir: SortDir
  selectJob: (id: string | null) => void
  setStageFilter: (stage: string | null) => void
  setSort: (field: SortField, dir: SortDir) => void
}

export const useJobStore = create<JobState>((set) => ({
  selectedJobId: null,
  stageFilter: null,
  sortField: 'appliedAt',
  sortDir: 'desc',
  selectJob: (id) => set({ selectedJobId: id }),
  setStageFilter: (stage) => set({ stageFilter: stage }),
  setSort: (field, dir) => set({ sortField: field, sortDir: dir }),
}))
