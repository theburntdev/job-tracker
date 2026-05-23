import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { AddActivityForm } from './AddActivityForm'
import type { CreateActivityInput } from '../../features/activities/types'

const JOB_APP_ID = 'aaaaaaaa-0000-0000-0000-000000000001'

const defaultProps = {
  jobApplicationId: JOB_APP_ID,
  onSubmit: vi.fn<[CreateActivityInput], void>(),
  isSaving: false,
}

describe('AddActivityForm', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('renders activity type and date fields', () => {
    render(<AddActivityForm {...defaultProps} />)

    expect(screen.getByLabelText(/activity type/i)).toBeInTheDocument()
    expect(screen.getByLabelText(/^date/i)).toBeInTheDocument()
  })

  it('renders optional contact and notes fields', () => {
    render(<AddActivityForm {...defaultProps} />)

    expect(screen.getByLabelText(/contact name/i)).toBeInTheDocument()
    expect(screen.getByLabelText(/contact email/i)).toBeInTheDocument()
    expect(screen.getByLabelText(/notes/i)).toBeInTheDocument()
  })

  it('renders optional stage update field', () => {
    render(<AddActivityForm {...defaultProps} />)

    expect(screen.getByLabelText(/update stage/i)).toBeInTheDocument()
  })

  it('calls onSubmit without newStage when stage not changed', async () => {
    const user = userEvent.setup()
    const onSubmit = vi.fn<[CreateActivityInput], void>()
    render(<AddActivityForm {...defaultProps} onSubmit={onSubmit} />)

    await user.selectOptions(screen.getByLabelText(/activity type/i), 'PhoneScreen')
    await user.click(screen.getByRole('button', { name: /log activity/i }))

    await waitFor(() => {
      expect(onSubmit).toHaveBeenCalledWith(
        expect.objectContaining({
          jobApplicationId: JOB_APP_ID,
          activityType: 'PhoneScreen',
          newStage: null,
        }),
      )
    })
  })

  it('calls onSubmit with newStage when stage is selected', async () => {
    const user = userEvent.setup()
    const onSubmit = vi.fn<[CreateActivityInput], void>()
    render(<AddActivityForm {...defaultProps} onSubmit={onSubmit} />)

    await user.selectOptions(screen.getByLabelText(/activity type/i), 'Interview')
    await user.selectOptions(screen.getByLabelText(/update stage/i), 'Interviewing')
    await user.click(screen.getByRole('button', { name: /log activity/i }))

    await waitFor(() => {
      expect(onSubmit).toHaveBeenCalledWith(
        expect.objectContaining({
          activityType: 'Interview',
          newStage: 'Interviewing',
        }),
      )
    })
  })
})
