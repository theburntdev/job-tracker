import { render, screen } from '@testing-library/react'
import { FormField } from './FormField'
import { Input } from '../atoms/Input'

describe('FormField', () => {
  it('renders label text', () => {
    render(
      <FormField label="Company" htmlFor="company">
        <Input id="company" aria-label="Company" />
      </FormField>,
    )
    expect(screen.getByText('Company')).toBeInTheDocument()
  })

  it('shows error message when error prop set', () => {
    render(
      <FormField label="Company" htmlFor="company" error="Required">
        <Input id="company" aria-label="Company" />
      </FormField>,
    )
    expect(screen.getByRole('alert')).toHaveTextContent('Required')
  })

  it('does not render error element when no error', () => {
    render(
      <FormField label="Company" htmlFor="company">
        <Input id="company" aria-label="Company" />
      </FormField>,
    )
    expect(screen.queryByRole('alert')).not.toBeInTheDocument()
  })

  it('renders required indicator when required prop set', () => {
    render(
      <FormField label="Company" htmlFor="company" required>
        <Input id="company" aria-label="Company" />
      </FormField>,
    )
    expect(screen.getByText('*')).toBeInTheDocument()
  })
})
