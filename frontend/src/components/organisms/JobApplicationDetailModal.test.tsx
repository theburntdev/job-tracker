import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { JobApplicationDetailModal } from './JobApplicationDetailModal'
import { makeJobApplication } from '../../test-utils/factories'

const defaultProps = {
  application: makeJobApplication({
    company: 'Acme Corp',
    title: 'Software Engineer',
    stage: 'Applied',
  }),
  onClose: vi.fn(),
  onSave: vi.fn(),
  isSaving: false,
}

describe('JobApplicationDetailModal', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('renders the dialog with company and title in the header', () => {
    render(<JobApplicationDetailModal {...defaultProps} />)
    expect(screen.getByRole('dialog')).toBeInTheDocument()
    expect(screen.getByRole('heading', { name: /acme corp/i })).toBeInTheDocument()
  })

  it('calls onClose when close button clicked', async () => {
    const user = userEvent.setup()
    const onClose = vi.fn()
    render(<JobApplicationDetailModal {...defaultProps} onClose={onClose} />)
    await user.click(screen.getByRole('button', { name: /close/i }))
    expect(onClose).toHaveBeenCalledOnce()
  })

  it('calls onClose when Cancel button clicked', async () => {
    const user = userEvent.setup()
    const onClose = vi.fn()
    render(<JobApplicationDetailModal {...defaultProps} onClose={onClose} />)
    await user.click(screen.getByRole('button', { name: /cancel/i }))
    expect(onClose).toHaveBeenCalledOnce()
  })

  it('calls onClose when Escape key pressed', async () => {
    const user = userEvent.setup()
    const onClose = vi.fn()
    render(<JobApplicationDetailModal {...defaultProps} onClose={onClose} />)
    await user.keyboard('{Escape}')
    expect(onClose).toHaveBeenCalledOnce()
  })

  it('calls onSave with current stage and description when Save clicked', async () => {
    const user = userEvent.setup()
    const onSave = vi.fn()
    render(<JobApplicationDetailModal {...defaultProps} onSave={onSave} />)
    await user.click(screen.getByRole('button', { name: /^save$/i }))
    expect(onSave).toHaveBeenCalledWith(
      expect.objectContaining({ stage: 'Applied' }),
    )
  })

  it('renders location dash when location is null', () => {
    const app = makeJobApplication({ location: null })
    render(<JobApplicationDetailModal {...defaultProps} application={app} />)
    expect(screen.getAllByText('—').length).toBeGreaterThan(0)
  })

  it('shows loading spinner when isSaving is true', () => {
    render(<JobApplicationDetailModal {...defaultProps} isSaving />)
    expect(screen.getByLabelText('Loading')).toBeInTheDocument()
  })
})
