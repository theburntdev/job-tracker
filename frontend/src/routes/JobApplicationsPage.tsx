import { useState, useEffect } from 'react'
import { useJobApplications, useUpdateJobApplication, useCreateJobApplication } from '../features/job-applications/api'
import { useFilteredJobApplications } from '../features/job-applications/hooks'
import { useJobStore, type SortField, type SortDir } from '../stores/useJobStore'
import { useUiStore } from '../stores/useUiStore'
import { JobApplicationTable } from '../components/organisms/JobApplicationTable'
import { JobApplicationDetailModal } from '../components/organisms/JobApplicationDetailModal'
import { CreateJobApplicationModal } from '../components/organisms/CreateJobApplicationModal'
import { MainLayout } from '../components/templates/MainLayout'
import { Button } from '../components/atoms/Button'
import { Pagination } from '../components/molecules/Pagination'

export default function JobApplicationsPage() {
  const [page, setPage] = useState(1)
  const stageFilter = useJobStore((s) => s.stageFilter)
  const sortField = useJobStore((s) => s.sortField)
  const sortDir = useJobStore((s) => s.sortDir)
  const setSort = useJobStore((s) => s.setSort)
  useEffect(() => { setPage(1) }, [stageFilter])

  function handleSortChange(field: SortField, dir: SortDir) {
    setSort(field, dir)
    setPage(1)
  }

  const { data, isLoading, error } = useJobApplications(page, sortField, sortDir)
  const items = data?.items ?? []
  const filtered = useFilteredJobApplications(items)
  const totalPages = data ? Math.ceil(Number(data.total) / Number(data.pageSize)) : 1

  const selectedJobId = useJobStore((s) => s.selectedJobId)
  const selectJob = useJobStore((s) => s.selectJob)
  const isCreateModalOpen = useUiStore((s) => s.isCreateModalOpen)
  const openCreateModal = useUiStore((s) => s.openCreateModal)
  const closeCreateModal = useUiStore((s) => s.closeCreateModal)
  const { mutate: updateJob, isPending: isUpdating } = useUpdateJobApplication()
  const { mutate: createJob, isPending: isCreating } = useCreateJobApplication()

  const selectedJob = items.find((app) => app.id === selectedJobId) ?? null

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
        onSortChange={handleSortChange}
      />
      <Pagination page={page} totalPages={totalPages} onPageChange={setPage} />
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
