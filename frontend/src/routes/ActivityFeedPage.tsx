import { useState } from 'react'
import { useActivities } from '../features/activities/api'
import { ActivityFeedList } from '../components/organisms/ActivityFeedList'
import { MainLayout } from '../components/templates/MainLayout'
import { Pagination } from '../components/molecules/Pagination'

const PAGE_SIZE = 20

export default function ActivityFeedPage() {
  const [page, setPage] = useState(1)
  const { data, isLoading, error } = useActivities(page, PAGE_SIZE)
  const activities = data?.items ?? []
  const totalPages = data ? Math.ceil(Number(data.total) / PAGE_SIZE) : 1

  return (
    <MainLayout>
      <div className="mb-6">
        <h1 className="text-2xl font-semibold text-text-primary">Activity Feed</h1>
      </div>
      <ActivityFeedList
        activities={activities}
        isLoading={isLoading}
        error={error}
      />
      <Pagination page={page} totalPages={totalPages} onPageChange={setPage} />
    </MainLayout>
  )
}
