import { create } from 'zustand'

interface JobState {
  selectedJobId: string | null
  stageFilter: string | null
  selectJob: (id: string | null) => void
  setStageFilter: (stage: string | null) => void
}

export const useJobStore = create<JobState>((set) => ({
  selectedJobId: null,
  stageFilter: null,
  selectJob: (id) => set({ selectedJobId: id }),
  setStageFilter: (stage) => set({ stageFilter: stage }),
}))
