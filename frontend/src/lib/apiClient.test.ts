import { apiClient } from './apiClient'
import { server } from '../test-utils/server'
import { http, HttpResponse } from 'msw'

describe('apiClient', () => {
  describe('get', () => {
    it('fetches and returns parsed JSON', async () => {
      server.use(
        http.get('http://localhost:5000/api/test', () => HttpResponse.json({ ok: true })),
      )
      const data = await apiClient.get<{ ok: boolean }>('/api/test')
      expect(data).toEqual({ ok: true })
    })
  })

  describe('post', () => {
    it('sends POST with JSON body and returns parsed JSON', async () => {
      server.use(
        http.post('http://localhost:5000/api/test', () =>
          HttpResponse.json({ created: true }, { status: 201 }),
        ),
      )
      const data = await apiClient.post<{ created: boolean }>('/api/test', { name: 'Test' })
      expect(data).toEqual({ created: true })
    })
  })

  describe('put', () => {
    it('sends PUT and returns parsed JSON', async () => {
      server.use(
        http.put('http://localhost:5000/api/test/1', () =>
          HttpResponse.json({ updated: true }),
        ),
      )
      const data = await apiClient.put<{ updated: boolean }>('/api/test/1', { name: 'Updated' })
      expect(data).toEqual({ updated: true })
    })
  })

  describe('error handling', () => {
    it('throws ApiClientError with status and code from error response body', async () => {
      server.use(
        http.get('http://localhost:5000/api/test', () =>
          HttpResponse.json(
            { status: 404, code: 'NOT_FOUND', message: 'Not found' },
            { status: 404 },
          ),
        ),
      )
      await expect(apiClient.get('/api/test')).rejects.toMatchObject({
        status: 404,
        code: 'NOT_FOUND',
        message: 'Not found',
      })
    })

    it('throws ApiClientError with UNKNOWN_ERROR when error body is not JSON', async () => {
      server.use(
        http.get('http://localhost:5000/api/test', () =>
          new HttpResponse('Internal Server Error', {
            status: 500,
            headers: { 'Content-Type': 'text/plain' },
          }),
        ),
      )
      await expect(apiClient.get('/api/test')).rejects.toMatchObject({
        status: 500,
        code: 'UNKNOWN_ERROR',
      })
    })
  })
})
