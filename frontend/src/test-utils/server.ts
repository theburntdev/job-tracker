import { setupServer } from 'msw/node'
import { http, HttpResponse } from 'msw'

export const server = setupServer(
  http.get(/\/api\/job-applications/, () =>
    HttpResponse.json({ items: [], total: 0, pageNumber: 1, pageSize: 20 }),
  ),
  http.post(/\/api\/job-applications/, () =>
    HttpResponse.json(null, { status: 201 }),
  ),
  http.put(/\/api\/job-applications/, () =>
    HttpResponse.json(null),
  ),
  http.delete(/\/api\/job-applications/, () =>
    new HttpResponse(null, { status: 204 }),
  ),
)
