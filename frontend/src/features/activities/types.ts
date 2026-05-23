import { z } from 'zod'
import type { components } from '../../lib/api.types.gen'
import { Stage } from '../job-applications/types'

type BackendActivityResponse = components['schemas']['ActivityResponse']
type BackendCreateActivityRequest = components['schemas']['CreateActivityRequest']

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

export const CreateActivityInputSchema = z.object({
  jobApplicationId: z.string().uuid(),
  activityType: ActivityType,
  occurredAt: z.string(),
  contactName: z.string().nullable().optional(),
  contactEmail: z.string().nullable().optional(),
  notes: z.string().nullable().optional(),
  newStage: Stage.nullable().optional(),
}) satisfies z.ZodType<BackendCreateActivityRequest>

export type CreateActivityInput = z.infer<typeof CreateActivityInputSchema>
