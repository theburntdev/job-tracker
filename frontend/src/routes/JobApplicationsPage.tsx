import { useJobApplications } from '../features/job-applications/api'
import { useFilteredJobApplications } from '../features/job-applications/hooks'
import { useJobStore } from '../stores/useJobStore'
import { useUiStore } from '../stores/useUiStore'
import { JobApplicationTable } from '../components/organisms/JobApplicationTable'
import { MainLayout } from '../components/templates/MainLayout'
import { Button } from '../components/atoms/Button'

export default function JobApplicationsPage() {
  const { data = [], isLoading, error } = useJobApplications()
  const filtered = useFilteredJobApplications(data)
  const selectJob = useJobStore((s) => s.selectJob)
  const openCreateModal = useUiStore((s) => s.openCreateModal)

  return (
    <MainLayout>
      <div className="mb-6 flex items-center justify-between">
        <h1 className="text-2xl font-semibold text-text-primary">Applications</h1>
        <Button onClick={openCreateModal}>Add Application</Button>
      </div>
      <JobApplicationTable
        applications={filtered}
        isLoading={isLoading}
        error={error}
        onSelect={selectJob}
      />
    </MainLayout>
  )
}
