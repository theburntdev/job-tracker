export interface ApiError {
  status: number
  code: string
  message: string
}

class ApiClientError extends Error implements ApiError {
  status: number
  code: string

  constructor({ status, code, message }: ApiError) {
    super(message)
    this.name = 'ApiClientError'
    this.status = status
    this.code = code
  }
}

async function request<T>(path: string, options?: RequestInit): Promise<T> {
  const baseUrl = import.meta.env.VITE_API_URL as string
  const response = await fetch(`${baseUrl}${path}`, {
    ...options,
    headers: {
      'Content-Type': 'application/json',
      ...options?.headers,
    },
  })

  if (!response.ok) {
    let errorBody: ApiError
    try {
      errorBody = (await response.json()) as ApiError
    } catch {
      errorBody = {
        status: response.status,
        code: 'UNKNOWN_ERROR',
        message: response.statusText,
      }
    }
    throw new ApiClientError(errorBody)
  }

  return response.json() as Promise<T>
}

export const apiClient = {
  get: <T>(path: string) => request<T>(path),
  post: <T>(path: string, body: unknown) =>
    request<T>(path, { method: 'POST', body: JSON.stringify(body) }),
  put: <T>(path: string, body: unknown) =>
    request<T>(path, { method: 'PUT', body: JSON.stringify(body) }),
  delete: <T>(path: string) => request<T>(path, { method: 'DELETE' }),
}
