import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { JobApplicationTable } from './JobApplicationTable'
import { makeJobApplication } from '../../test-utils/factories'

const defaultProps = {
  applications: [],
  isLoading: false,
  error: null,
  onSelect: vi.fn(),
  onStageChange: vi.fn(),
  sortField: 'appliedAt' as const,
  sortDir: 'desc' as const,
  onSortChange: vi.fn(),
}

describe('JobApplicationTable', () => {
  it('shows spinner when isLoading is true', () => {
    render(<JobApplicationTable {...defaultProps} isLoading />)
    expect(screen.getByLabelText('Loading')).toBeInTheDocument()
  })

  it('shows error message when error is set', () => {
    const error = new Error('Failed to load')
    render(<JobApplicationTable {...defaultProps} error={error} />)
    expect(screen.getByRole('alert')).toHaveTextContent('Failed to load')
  })

  it('shows empty state message when no applications', () => {
    render(<JobApplicationTable {...defaultProps} />)
    expect(screen.getByText(/no applications yet/i)).toBeInTheDocument()
  })

  it('renders a row per application with company and title', () => {
    const apps = [
      makeJobApplication({ company: 'Acme', title: 'Engineer' }),
      makeJobApplication({ id: 'id-2', company: 'Globex', title: 'Designer' }),
    ]
    render(<JobApplicationTable {...defaultProps} applications={apps} />)
    expect(screen.getByText('Acme')).toBeInTheDocument()
    expect(screen.getByText('Engineer')).toBeInTheDocument()
    expect(screen.getByText('Globex')).toBeInTheDocument()
    expect(screen.getByText('Designer')).toBeInTheDocument()
  })

  it('calls onSelect with application id when detail icon clicked', async () => {
    const user = userEvent.setup()
    const onSelect = vi.fn()
    const app = makeJobApplication({ id: 'app-id-1', company: 'Acme' })
    render(<JobApplicationTable {...defaultProps} applications={[app]} onSelect={onSelect} />)
    await user.click(screen.getByRole('button', { name: /open details for acme/i }))
    expect(onSelect).toHaveBeenCalledWith('app-id-1')
  })

  it('calls onSortChange with desc when clicking a new sort field', async () => {
    const user = userEvent.setup()
    const onSortChange = vi.fn()
    const app = makeJobApplication()
    render(<JobApplicationTable {...defaultProps} applications={[app]} onSortChange={onSortChange} />)
    await user.click(screen.getByRole('button', { name: /updated/i }))
    expect(onSortChange).toHaveBeenCalledWith('updatedAt', 'desc')
  })

  it('toggles sort direction when clicking the active sort field', async () => {
    const user = userEvent.setup()
    const onSortChange = vi.fn()
    const app = makeJobApplication()
    render(
      <JobApplicationTable
        {...defaultProps}
        applications={[app]}
        sortField="appliedAt"
        sortDir="desc"
        onSortChange={onSortChange}
      />,
    )
    await user.click(screen.getByRole('button', { name: /sort by applied/i }))
    expect(onSortChange).toHaveBeenCalledWith('appliedAt', 'asc')
  })
})
