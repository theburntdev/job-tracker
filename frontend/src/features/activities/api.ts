import { useQuery } from '@tanstack/react-query'
import { z } from 'zod'
import type { components } from '../../lib/api.types.gen'
import { apiClient } from '../../lib/apiClient'
import { ActivitySchema } from './types'

const QUERY_KEY = ['activities'] as const

type BackendPage = components['schemas']['PageOfActivityResponse']

const PageSchema: z.ZodType<BackendPage> = z.object({
  items: z.array(ActivitySchema),
  total: z.union([z.number(), z.string()]),
  pageNumber: z.union([z.number(), z.string()]),
  pageSize: z.union([z.number(), z.string()]),
})

export function useActivities(page = 1, pageSize = 20) {
  return useQuery({
    queryKey: [...QUERY_KEY, page, pageSize],
    queryFn: async () => {
      const data = await apiClient.get<unknown>(
        `/api/activities?page=${page}&pageSize=${pageSize}`,
      )
      return PageSchema.parse(data)
    },
  })
}
