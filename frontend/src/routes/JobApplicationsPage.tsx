import { useJobApplications, useUpdateJobApplication, useCreateJobApplication } from '../features/job-applications/api'
import { useFilteredJobApplications } from '../features/job-applications/hooks'
import { useJobStore } from '../stores/useJobStore'
import { useUiStore } from '../stores/useUiStore'
import { JobApplicationTable } from '../components/organisms/JobApplicationTable'
import { JobApplicationDetailModal } from '../components/organisms/JobApplicationDetailModal'
import { CreateJobApplicationModal } from '../components/organisms/CreateJobApplicationModal'
import { MainLayout } from '../components/templates/MainLayout'
import { Button } from '../components/atoms/Button'

export default function JobApplicationsPage() {
  const { data = [], isLoading, error } = useJobApplications()
  const filtered = useFilteredJobApplications(data)
  const selectedJobId = useJobStore((s) => s.selectedJobId)
  const selectJob = useJobStore((s) => s.selectJob)
  const sortField = useJobStore((s) => s.sortField)
  const sortDir = useJobStore((s) => s.sortDir)
  const setSort = useJobStore((s) => s.setSort)
  const isCreateModalOpen = useUiStore((s) => s.isCreateModalOpen)
  const openCreateModal = useUiStore((s) => s.openCreateModal)
  const closeCreateModal = useUiStore((s) => s.closeCreateModal)
  const { mutate: updateJob, isPending: isUpdating } = useUpdateJobApplication()
  const { mutate: createJob, isPending: isCreating } = useCreateJobApplication()

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
        sortField={sortField}
        sortDir={sortDir}
        onSortChange={setSort}
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
          isSaving={isUpdating}
        />
      )}
      {isCreateModalOpen && (
        <CreateJobApplicationModal
          onClose={closeCreateModal}
          onSave={(data) => createJob(data, { onSuccess: closeCreateModal })}
          isSaving={isCreating}
        />
      )}
    </MainLayout>
  )
}
