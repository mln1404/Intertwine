import { useAuthStore } from '../stores/authStore'
import { pinia } from '../stores/pinia'

export class ApiError extends Error {
  status: number
  constructor(message: string, status: number) {
    super(message)
    this.status = status
  }
}
const baseUrl = (import.meta.env.VITE_API_URL || '').replace(/\/$/, '')
export async function request<T>(path: string, options: RequestInit = {}): Promise<T> {
  const headers = new Headers(options.headers)
  headers.set('Accept', 'application/json')
  if (options.body) headers.set('Content-Type', 'application/json')
  const token = useAuthStore(pinia).accessToken
  if (token) headers.set('Authorization', `Bearer ${token}`)
  let response: Response
  try {
    response = await fetch(`${baseUrl}${path}`, {
      ...options,
      headers,
      signal: options.signal ?? AbortSignal.timeout(15000),
    })
  } catch {
    throw new ApiError('We couldn’t reach Intertwine. Check your connection and try again.', 0)
  }
  const data = response.headers.get('content-type')?.includes('json') ? await response.json() : null
  if (!response.ok) {
    const validation =
      data?.errors && typeof data.errors === 'object'
        ? Object.values(data.errors).flat().join(' ')
        : ''
    const fallback =
      response.status === 401
        ? 'Your session has ended. Please sign in again.'
        : response.status === 429
          ? 'Too many attempts. Please wait a minute and try again.'
          : 'Something went wrong. Please try again.'
    throw new ApiError(data?.error || data?.message || validation || fallback, response.status)
  }
  if (response.status !== 204 && data === null)
    throw new ApiError('The API returned an unexpected response. Check the API connection.', 502)
  return data as T
}
