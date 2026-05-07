import { render, screen } from '@testing-library/react'
import { StatusBadge } from './StatusBadge'
import type { Stage } from '../../features/job-applications/types'

describe('StatusBadge', () => {
  const stages: Stage[] = ['Applied', 'Screening', 'Interviewing', 'Offer', 'Rejected', 'Withdrawn']

  it.each(stages)('renders the %s stage label', (stage) => {
    render(<StatusBadge stage={stage} />)
    expect(screen.getByText(stage)).toBeInTheDocument()
  })
})
