import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { StageDropdown } from './StageDropdown'
import { Stage } from '../../features/job-applications/types'

describe('StageDropdown', () => {
  it('renders trigger button with current stage', () => {
    render(<StageDropdown stage="Applied" onChange={() => {}} />)

    expect(screen.getByRole('button', { name: /applied/i })).toBeInTheDocument()
  })

  it('does not show options initially', () => {
    render(<StageDropdown stage="Applied" onChange={() => {}} />)

    expect(screen.queryByRole('button', { name: /screening/i })).not.toBeInTheDocument()
  })

  it('opens dropdown when trigger clicked', async () => {
    const user = userEvent.setup()
    render(<StageDropdown stage="Applied" onChange={() => {}} />)

    await user.click(screen.getByRole('button', { name: /applied/i }))

    for (const s of Stage.options) {
      expect(screen.getAllByText(s).length).toBeGreaterThan(0)
    }
  })

  it('calls onChange with selected stage and closes dropdown', async () => {
    const user = userEvent.setup()
    const onChange = vi.fn()
    render(<StageDropdown stage="Applied" onChange={onChange} />)

    await user.click(screen.getByRole('button', { name: /applied/i }))
    await user.click(screen.getAllByRole('button', { name: /offer/i })[0])

    expect(onChange).toHaveBeenCalledWith('Offer')
    expect(screen.queryByRole('button', { name: /screening/i })).not.toBeInTheDocument()
  })

  it('does not call onChange when disabled', async () => {
    const user = userEvent.setup()
    const onChange = vi.fn()
    render(<StageDropdown stage="Applied" onChange={onChange} disabled />)

    await user.click(screen.getByRole('button', { name: /applied/i }))

    expect(onChange).not.toHaveBeenCalled()
    expect(screen.queryByRole('button', { name: /screening/i })).not.toBeInTheDocument()
  })

  it('closes dropdown on outside click', async () => {
    const user = userEvent.setup()
    render(
      <div>
        <StageDropdown stage="Applied" onChange={() => {}} />
        <div data-testid="outside">outside</div>
      </div>
    )

    await user.click(screen.getByRole('button', { name: /applied/i }))
    expect(screen.getAllByText('Screening').length).toBeGreaterThan(0)

    await user.click(screen.getByTestId('outside'))

    expect(screen.queryByRole('button', { name: /screening/i })).not.toBeInTheDocument()
  })

  it('toggles closed when trigger clicked while open', async () => {
    const user = userEvent.setup()
    render(<StageDropdown stage="Applied" onChange={() => {}} />)

    await user.click(screen.getByRole('button', { name: /applied/i }))
    expect(screen.getAllByText('Screening').length).toBeGreaterThan(0)

    await user.click(screen.getAllByRole('button', { name: /applied/i })[0])

    expect(screen.queryByRole('button', { name: /screening/i })).not.toBeInTheDocument()
  })
})
