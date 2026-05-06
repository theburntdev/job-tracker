import { useJobApplications, useUpdateJobApplication } from '../features/job-applications/api'
import { useFilteredJobApplications } from '../features/job-applications/hooks'
import { useJobStore } from '../stores/useJobStore'
import { useUiStore } from '../stores/useUiStore'
import { JobApplicationTable } from '../components/organisms/JobApplicationTable'
import { JobApplicationDetailModal } from '../components/organisms/JobApplicationDetailModal'
import { MainLayout } from '../components/templates/MainLayout'
import { Button } from '../components/atoms/Button'

export default function JobApplicationsPage() {
  const { data = [], isLoading, error } = useJobApplications()
  const filtered = useFilteredJobApplications(data)
  const selectedJobId = useJobStore((s) => s.selectedJobId)
  const selectJob = useJobStore((s) => s.selectJob)
  const openCreateModal = useUiStore((s) => s.openCreateModal)
  const { mutate: updateJob, isPending } = useUpdateJobApplication()

  const selectedJob = data.find((app) => app.id === selectedJobId) ?? null

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
      {selectedJob && (
        <JobApplicationDetailModal
          application={selectedJob}
          onClose={() => selectJob(null)}
          onSave={({ stage, description }) => {
            updateJob(
              {
                id: selectedJob.id,
                title: selectedJob.title,
                company: selectedJob.company,
                location: selectedJob.location,
                url: selectedJob.url,
                description,
                stage,
                appliedAt: selectedJob.appliedAt,
                postedAt: selectedJob.postedAt,
              },
              { onSuccess: () => selectJob(null) },
            )
          }}
          isSaving={isPending}
        />
      )}
    </MainLayout>
  )
}
