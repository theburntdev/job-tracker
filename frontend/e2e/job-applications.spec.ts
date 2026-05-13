import { test, expect } from '@playwright/test'

const API = 'http://localhost:5001'

async function seedApp(
  request: Parameters<Parameters<typeof test>[1]>[0]['request'],
  overrides: Record<string, unknown> = {}
) {
  const res = await request.post(`${API}/api/job-applications`, {
    data: {
      title: 'Software Engineer',
      company: 'Acme Corp',
      stage: 'Applied',
      ...overrides,
    },
  })
  return res.json() as Promise<{ id: string; company: string; title: string }>
}

test.beforeEach(async ({ request }) => {
  await request.delete(`${API}/api/test/reset`)
})

test('displays empty state when no applications exist', async ({ page }) => {
  await page.goto('/')
  await expect(
    page.getByText('No applications yet. Add one to get started.')
  ).toBeVisible()
})

test('creates a new application and shows it in the table', async ({ page }) => {
  await page.goto('/')
  await page.getByRole('button', { name: 'Add Application' }).click()

  const modal = page.getByRole('dialog', { name: 'Add Application' })
  await modal.getByLabel(/company/i).fill('Globex')
  await modal.getByLabel(/role/i).fill('Backend Engineer')
  await modal.getByRole('button', { name: 'Add' }).click()

  await expect(page.getByText('Globex')).toBeVisible()
  await expect(page.getByText('Backend Engineer')).toBeVisible()
})

test('edits an application stage via the table dropdown', async ({ page, request }) => {
  await seedApp(request)
  await page.goto('/')

  // The StageDropdown trigger shows the current stage label — scope to tbody to avoid the "Applied" sort header button
  await page.locator('tbody').getByRole('button', { name: 'Applied' }).first().click()
  await page.getByRole('button', { name: 'Screening' }).click()

  await expect(page.getByRole('button', { name: 'Screening' })).toBeVisible()
})

test('opens detail modal and saves updated description', async ({ page, request }) => {
  await seedApp(request, { company: 'Initech' })
  await page.goto('/')

  await page.getByRole('button', { name: /Open details for Initech/i }).click()

  const modal = page.getByRole('dialog')
  await modal.getByLabel(/description/i).fill('Interesting role')
  await modal.getByRole('button', { name: 'Save' }).click()

  // Modal closes on successful save
  await expect(modal).not.toBeVisible()
})

test('deletes an application', async ({ page, request }) => {
  await seedApp(request, { company: 'Umbrella' })
  await page.goto('/')

  await page.getByRole('button', { name: /Open details for Umbrella/i }).click()

  const modal = page.getByRole('dialog')
  await modal.getByRole('button', { name: 'Delete' }).click()
  await modal.getByRole('button', { name: 'Confirm' }).click()

  await expect(
    page.getByText('No applications yet. Add one to get started.')
  ).toBeVisible()
})

test('paginates when more than one page of results exists', async ({ page, request }) => {
  // Page size is 20 — seed 21 to trigger page 2
  await Promise.all(
    Array.from({ length: 21 }, (_, i) =>
      seedApp(request, { company: `Company ${i + 1}`, title: `Engineer ${i + 1}` })
    )
  )

  await page.goto('/')
  await expect(page.getByText('Page 1 of 2')).toBeVisible()

  await page.getByRole('button', { name: 'Next' }).click()
  await expect(page.getByText('Page 2 of 2')).toBeVisible()
})
