import { render, screen } from '@testing-library/react'
import { Spinner } from './Spinner'

describe('Spinner', () => {
  it('renders with aria-label "Loading"', () => {
    render(<Spinner />)
    expect(screen.getByLabelText('Loading')).toBeInTheDocument()
  })

  it('applies custom className', () => {
    render(<Spinner className="h-6 w-6" />)
    expect(screen.getByLabelText('Loading')).toHaveClass('h-6')
  })
})
