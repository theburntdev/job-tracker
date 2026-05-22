import { z } from 'zod'
import type { components } from '../../lib/api.types.gen'

type BackendActivityResponse = components['schemas']['ActivityResponse']

export const ActivityType = z.enum([
  'Applied',
  'PhoneScreen',
  'Interview',
  'Offer',
  'Rejected',
  'Withdrew',
  'EmailedRecruiter',
  'CalledRecruiter',
])
export type ActivityType = z.infer<typeof ActivityType>

export const ActivitySchema = z.object({
  id: z.string().uuid(),
  jobApplicationId: z.string().uuid(),
  jobTitle: z.string().nullable(),
  company: z.string().nullable(),
  activityType: ActivityType,
  occurredAt: z.string(),
  contactName: z.string().nullable(),
  contactEmail: z.string().nullable(),
  notes: z.string().nullable(),
  createdAt: z.string(),
}) satisfies z.ZodType<BackendActivityResponse>

export type Activity = z.infer<typeof ActivitySchema>
