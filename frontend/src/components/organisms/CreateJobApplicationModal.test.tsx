import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { CreateJobApplicationModal } from './CreateJobApplicationModal'

const defaultProps = {
  onClose: vi.fn(),
  onSave: vi.fn(),
  isSaving: false,
}

describe('CreateJobApplicationModal', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('renders the dialog with title "Add Application"', () => {
    render(<CreateJobApplicationModal {...defaultProps} />)
    expect(screen.getByRole('dialog')).toBeInTheDocument()
    expect(screen.getByText('Add Application')).toBeInTheDocument()
  })

  it('calls onClose when Cancel button clicked', async () => {
    const user = userEvent.setup()
    const onClose = vi.fn()
    render(<CreateJobApplicationModal {...defaultProps} onClose={onClose} />)
    await user.click(screen.getByRole('button', { name: /cancel/i }))
    expect(onClose).toHaveBeenCalledOnce()
  })

  it('calls onClose when close button clicked', async () => {
    const user = userEvent.setup()
    const onClose = vi.fn()
    render(<CreateJobApplicationModal {...defaultProps} onClose={onClose} />)
    await user.click(screen.getByRole('button', { name: /close/i }))
    expect(onClose).toHaveBeenCalledOnce()
  })

  it('calls onClose when Escape key pressed', async () => {
    const user = userEvent.setup()
    const onClose = vi.fn()
    render(<CreateJobApplicationModal {...defaultProps} onClose={onClose} />)
    await user.keyboard('{Escape}')
    expect(onClose).toHaveBeenCalledOnce()
  })

  it('shows validation errors when required fields are empty on submit', async () => {
    const user = userEvent.setup()
    render(<CreateJobApplicationModal {...defaultProps} />)
    await user.click(screen.getByRole('button', { name: /^add$/i }))
    await waitFor(() => {
      expect(screen.getAllByText('Required')).toHaveLength(2)
    })
  })

  it('calls onSave with form data when submitted with valid input', async () => {
    const user = userEvent.setup()
    const onSave = vi.fn()
    render(<CreateJobApplicationModal {...defaultProps} onSave={onSave} />)

    await user.type(screen.getByLabelText(/company/i), 'Acme Corp')
    await user.type(screen.getByLabelText(/role/i), 'Software Engineer')
    await user.click(screen.getByRole('button', { name: /^add$/i }))

    await waitFor(() => {
      expect(onSave).toHaveBeenCalledWith(
        expect.objectContaining({
          company: 'Acme Corp',
          title: 'Software Engineer',
          stage: 'Applied',
        }),
      )
    })
  })

  it('shows loading spinner on Add button when isSaving is true', () => {
    render(<CreateJobApplicationModal {...defaultProps} isSaving />)
    expect(screen.getByLabelText('Loading')).toBeInTheDocument()
  })
})
