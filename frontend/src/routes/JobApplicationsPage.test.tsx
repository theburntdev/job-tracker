import { render, screen, waitFor } from '@testing-library/react'
import { MemoryRouter } from 'react-router'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import userEvent from '@testing-library/user-event'
import JobApplicationsPage from './JobApplicationsPage'
import { useJobStore } from '../stores/useJobStore'
import { useUiStore } from '../stores/useUiStore'

function wrapper({ children }: { children: React.ReactNode }) {
  const qc = new QueryClient({
    defaultOptions: { queries: { retry: false } },
  })
  return (
    <QueryClientProvider client={qc}>
      <MemoryRouter>{children}</MemoryRouter>
    </QueryClientProvider>
  )
}

beforeEach(() => {
  useJobStore.setState({ selectedJobId: null, stageFilter: null, sortField: 'appliedAt', sortDir: 'desc' })
  useUiStore.setState({ isCreateModalOpen: false })
})

describe('JobApplicationsPage', () => {
  it('renders the Applications heading', () => {
    render(<JobApplicationsPage />, { wrapper })
    expect(screen.getByRole('heading', { name: /applications/i })).toBeInTheDocument()
  })

  it('renders Add Application button', () => {
    render(<JobApplicationsPage />, { wrapper })
    expect(screen.getByRole('button', { name: /add application/i })).toBeInTheDocument()
  })

  it('shows empty state after query resolves with no data', async () => {
    render(<JobApplicationsPage />, { wrapper })
    await waitFor(() => {
      expect(screen.getByText(/no applications yet/i)).toBeInTheDocument()
    })
  })

  it('opens create modal when Add Application clicked', async () => {
    const user = userEvent.setup()
    render(<JobApplicationsPage />, { wrapper })
    await user.click(screen.getByRole('button', { name: /add application/i }))
    expect(screen.getByRole('dialog')).toBeInTheDocument()
  })
})
