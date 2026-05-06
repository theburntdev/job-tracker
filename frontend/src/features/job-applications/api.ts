import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { z } from 'zod'
import type { components } from '../../lib/api.types.gen'
import { apiClient } from '../../lib/apiClient'
import {
  type JobApplication,
  type CreateJobApplicationInput,
  JobApplicationSchema,
} from './types'

const QUERY_KEY = ['job-applications'] as const

type BackendPage = components['schemas']['PageOfJobApplicationResponse']

// number | string matches OpenAPI int32 quirk — backend always sends number at runtime
const PageSchema: z.ZodType<BackendPage> = z.object({
  items: z.array(JobApplicationSchema),
  total: z.union([z.number(), z.string()]),
  pageNumber: z.union([z.number(), z.string()]),
  pageSize: z.union([z.number(), z.string()]),
})

export function useJobApplications() {
  return useQuery({
    queryKey: QUERY_KEY,
    queryFn: async () => {
      const data = await apiClient.get<unknown>('/api/job-applications')
      const items = PageSchema.parse(data).items
      return items.slice().sort((a, b) => {
        const aTime = a.appliedAt ? new Date(a.appliedAt).getTime() : null
        const bTime = b.appliedAt ? new Date(b.appliedAt).getTime() : null
        if (aTime !== null && bTime !== null) return bTime - aTime
        if (aTime !== null) return -1
        if (bTime !== null) return 1
        return new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime()
      })
    },
  })
}

export function useCreateJobApplication() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (input: CreateJobApplicationInput) =>
      apiClient.post<JobApplication>('/api/job-applications', input),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: QUERY_KEY })
    },
  })
}

export function useDeleteJobApplication() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (id: string) => apiClient.delete<void>(`/api/job-applications/${id}`),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: QUERY_KEY })
    },
  })
}
