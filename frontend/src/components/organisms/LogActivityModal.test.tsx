import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { LogActivityModal } from './LogActivityModal'
import type { CreateActivityInput } from '../../features/activities/types'

const JOB_APP_ID = 'aaaaaaaa-0000-0000-0000-000000000001'

const defaultProps = {
  jobApplicationId: JOB_APP_ID,
  onClose: vi.fn(),
  onSubmit: vi.fn<[CreateActivityInput], void>(),
  isSaving: false,
}

describe('LogActivityModal', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('renders dialog with "Log Activity" heading', () => {
    render(<LogActivityModal {...defaultProps} />)
    expect(screen.getByRole('dialog')).toBeInTheDocument()
    expect(screen.getByRole('heading', { name: /log activity/i })).toBeInTheDocument()
  })

  it('renders activity type and date fields', () => {
    render(<LogActivityModal {...defaultProps} />)
    expect(screen.getByLabelText(/activity type/i)).toBeInTheDocument()
    expect(screen.getByLabelText(/^date/i)).toBeInTheDocument()
  })

  it('calls onClose when close button clicked', async () => {
    const user = userEvent.setup()
    const onClose = vi.fn()
    render(<LogActivityModal {...defaultProps} onClose={onClose} />)
    await user.click(screen.getByRole('button', { name: /close/i }))
    expect(onClose).toHaveBeenCalledOnce()
  })

  it('calls onClose when Escape key pressed', async () => {
    const user = userEvent.setup()
    const onClose = vi.fn()
    render(<LogActivityModal {...defaultProps} onClose={onClose} />)
    await user.keyboard('{Escape}')
    expect(onClose).toHaveBeenCalledOnce()
  })

  it('calls onSubmit with form data when Log Activity submitted', async () => {
    const user = userEvent.setup()
    const onSubmit = vi.fn<[CreateActivityInput], void>()
    render(<LogActivityModal {...defaultProps} onSubmit={onSubmit} />)

    await user.selectOptions(screen.getByLabelText(/activity type/i), 'Interview')
    await user.click(screen.getByRole('button', { name: /log activity/i }))

    await waitFor(() => {
      expect(onSubmit).toHaveBeenCalledWith(
        expect.objectContaining({
          jobApplicationId: JOB_APP_ID,
          activityType: 'Interview',
        }),
      )
    })
  })

  it('shows loading spinner when isSaving is true', () => {
    render(<LogActivityModal {...defaultProps} isSaving />)
    expect(screen.getByLabelText('Loading')).toBeInTheDocument()
  })
})
