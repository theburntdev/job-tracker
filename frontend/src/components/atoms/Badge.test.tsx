import { render, screen } from '@testing-library/react'
import { Badge } from './Badge'

describe('Badge', () => {
  it('renders children', () => {
    render(<Badge>Applied</Badge>)
    expect(screen.getByText('Applied')).toBeInTheDocument()
  })

  it('forwards extra props to the span', () => {
    render(<Badge data-testid="badge">Applied</Badge>)
    expect(screen.getByTestId('badge')).toBeInTheDocument()
  })

  it('applies custom className', () => {
    render(<Badge className="extra-class">Applied</Badge>)
    expect(screen.getByText('Applied')).toHaveClass('extra-class')
  })
})
