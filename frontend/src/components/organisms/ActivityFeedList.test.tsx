import { render, screen } from '@testing-library/react'
import { ActivityFeedList } from './ActivityFeedList'
import type { Activity } from '../../features/activities/types'

function makeActivity(overrides: Partial<Activity> = {}): Activity {
  return {
    id: 'aaaaaaaa-0000-0000-0000-000000000001',
    jobApplicationId: 'bbbbbbbb-0000-0000-0000-000000000001',
    jobTitle: 'Software Engineer',
    company: 'Acme Corp',
    activityType: 'Applied',
    occurredAt: '2026-03-01T12:00:00.000Z',
    contactName: null,
    contactEmail: null,
    notes: null,
    createdAt: '2026-03-01T12:00:00.000Z',
    ...overrides,
  }
}

const defaultProps = {
  activities: [],
  isLoading: false,
  error: null,
}

describe('ActivityFeedList', () => {
  it('shows spinner when isLoading is true', () => {
    render(<ActivityFeedList {...defaultProps} isLoading />)
    expect(screen.getByLabelText('Loading')).toBeInTheDocument()
  })

  it('shows error message when error is set', () => {
    render(<ActivityFeedList {...defaultProps} error={new Error('fail')} />)
    expect(screen.getByText(/failed to load activity feed/i)).toBeInTheDocument()
  })

  it('shows empty state when no activities', () => {
    render(<ActivityFeedList {...defaultProps} />)
    expect(screen.getByText(/no activity recorded yet/i)).toBeInTheDocument()
  })

  it('renders activity type label for each activity', () => {
    const activities = [
      makeActivity({ activityType: 'Applied' }),
      makeActivity({ id: 'id-2', activityType: 'Interview' }),
    ]

    render(<ActivityFeedList {...defaultProps} activities={activities} />)

    expect(screen.getByText('Applied')).toBeInTheDocument()
    expect(screen.getByText('Interview')).toBeInTheDocument()
  })

  it('renders job title and company when present', () => {
    const activity = makeActivity({ jobTitle: 'Staff Engineer', company: 'Globex' })

    render(<ActivityFeedList {...defaultProps} activities={[activity]} />)

    expect(screen.getByText(/Staff Engineer/)).toBeInTheDocument()
    expect(screen.getByText(/Globex/)).toBeInTheDocument()
  })

  it('renders contact name and email when present', () => {
    const activity = makeActivity({
      contactName: 'Jane Doe',
      contactEmail: 'jane@example.com',
    })

    render(<ActivityFeedList {...defaultProps} activities={[activity]} />)

    expect(screen.getByText(/Jane Doe/)).toBeInTheDocument()
    expect(screen.getByText(/jane@example.com/)).toBeInTheDocument()
  })

  it('renders notes when present', () => {
    const activity = makeActivity({ notes: 'Great culture fit' })

    render(<ActivityFeedList {...defaultProps} activities={[activity]} />)

    expect(screen.getByText('Great culture fit')).toBeInTheDocument()
  })
})
