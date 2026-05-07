import { render, screen } from '@testing-library/react'
import { MemoryRouter } from 'react-router'
import { NavBar } from './NavBar'

describe('NavBar', () => {
  it('renders the app name as a link', () => {
    render(
      <MemoryRouter>
        <NavBar />
      </MemoryRouter>,
    )
    expect(screen.getByRole('link', { name: /Job Application Tracker/i })).toBeInTheDocument()
  })
})
