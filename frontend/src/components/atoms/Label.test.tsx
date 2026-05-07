import { render, screen } from '@testing-library/react'
import { Label } from './Label'

describe('Label', () => {
  it('renders children', () => {
    render(<Label>Company</Label>)
    expect(screen.getByText('Company')).toBeInTheDocument()
  })

  it('renders required indicator when required is true', () => {
    const { container } = render(<Label required>Company</Label>)
    expect(container.querySelector('[aria-hidden]')).toBeInTheDocument()
  })

  it('does not render required indicator when required is not set', () => {
    const { container } = render(<Label>Company</Label>)
    expect(container.querySelector('[aria-hidden]')).not.toBeInTheDocument()
  })

  it('passes htmlFor to the label element', () => {
    render(<Label htmlFor="company-input">Company</Label>)
    expect(screen.getByText('Company')).toHaveAttribute('for', 'company-input')
  })
})
