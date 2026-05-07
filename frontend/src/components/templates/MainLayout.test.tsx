import { render, screen } from '@testing-library/react'
import { MemoryRouter } from 'react-router'
import { MainLayout } from './MainLayout'

describe('MainLayout', () => {
  it('renders children inside the layout', () => {
    render(
      <MemoryRouter>
        <MainLayout>
          <p>Page content</p>
        </MainLayout>
      </MemoryRouter>,
    )
    expect(screen.getByText('Page content')).toBeInTheDocument()
  })

  it('renders the nav bar', () => {
    render(
      <MemoryRouter>
        <MainLayout>
          <p>Content</p>
        </MainLayout>
      </MemoryRouter>,
    )
    expect(screen.getByRole('navigation')).toBeInTheDocument()
  })
})
