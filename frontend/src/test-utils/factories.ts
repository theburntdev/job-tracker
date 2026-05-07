import type { JobApplication } from '../features/job-applications/types'

export function makeJobApplication(overrides: Partial<JobApplication> = {}): JobApplication {
  return {
    id: 'aaaaaaaa-0000-0000-0000-000000000001',
    title: 'Software Engineer',
    company: 'Acme Corp',
    stage: 'Applied',
    location: null,
    url: null,
    description: null,
    appliedAt: '2026-01-01T00:00:00.000Z',
    postedAt: null,
    createdAt: '2026-01-01T00:00:00.000Z',
    updatedAt: '2026-01-02T00:00:00.000Z',
    ...overrides,
  }
}
