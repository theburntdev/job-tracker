import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { z } from 'zod'
import type { components } from '../../lib/api.types.gen'
import { apiClient } from '../../lib/apiClient'
import {
  type JobApplication,
  type CreateJobApplicationInput,
  type UpdateJobApplicationInput,
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

export function useJobApplications(page = 1, sortBy = 'updatedAt', sortDir = 'desc') {
  return useQuery({
    queryKey: [...QUERY_KEY, page, sortBy, sortDir],
    queryFn: async () => {
      const data = await apiClient.get<unknown>(
        `/api/job-applications?page=${page}&pageSize=20&sortBy=${sortBy}&sortDir=${sortDir}`,
      )
      return PageSchema.parse(data)
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

export function useUpdateJobApplication() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({ id, ...input }: { id: string } & UpdateJobApplicationInput) =>
      apiClient.put<JobApplication>(`/api/job-applications/${id}`, input),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: QUERY_KEY })
    },
  })
}
