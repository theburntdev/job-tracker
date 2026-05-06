import { BrowserRouter, Routes, Route } from 'react-router'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import JobApplicationsPage from './routes/JobApplicationsPage'

const queryClient = new QueryClient()

export function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <BrowserRouter>
        <Routes>
          <Route path="/" element={<JobApplicationsPage />} />
        </Routes>
      </BrowserRouter>
    </QueryClientProvider>
  )
}
