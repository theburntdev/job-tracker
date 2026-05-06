import { z } from 'zod'
import type { components } from '../../lib/api.types.gen'

type BackendJobApplication = components['schemas']['JobApplicationResponse']

export const Stage = z.enum([
  'Applied',
  'Screening',
  'Interviewing',
  'Offer',
  'Rejected',
  'Withdrawn',
])
export type Stage = z.infer<typeof Stage>

export const JobApplicationSchema = z.object({
  id: z.string().uuid(),
  title: z.string(),
  company: z.string(),
  location: z.string().nullable(),
  url: z.string().nullable(),
  description: z.string().nullable(),
  stage: Stage,
  appliedAt: z.string().nullable(),
  postedAt: z.string().nullable(),
  createdAt: z.string(),
  updatedAt: z.string(),
}) satisfies z.ZodType<BackendJobApplication>

export type JobApplication = z.infer<typeof JobApplicationSchema>

export const CreateJobApplicationSchema = z.object({
  company: z.string().min(1),
  title: z.string().min(1),
  location: z.string().optional(),
  url: z.string().optional(),
  description: z.string().optional(),
  stage: Stage,
})
export type CreateJobApplicationInput = z.infer<typeof CreateJobApplicationSchema>
