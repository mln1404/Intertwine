import axios, { AxiosHeaders, isAxiosError } from 'axios'
import type { AxiosRequestConfig, AxiosResponseHeaders, RawAxiosResponseHeaders } from 'axios'
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
const unreachableMessage = 'We couldn’t reach Intertwine. Check your connection and try again.'

export const apiClient = axios.create({
  baseURL: baseUrl,
  withCredentials: true,
  timeout: 15000,
})

apiClient.interceptors.request.use((config) => {
  const token = useAuthStore(pinia).accessToken
  if (token) config.headers.set('Authorization', `Bearer ${token}`)
  return config
})

function responseMessage(data: unknown, status: number): string {
  const payload = data && typeof data === 'object' ? (data as Record<string, unknown>) : {}
  const validation =
    payload.errors && typeof payload.errors === 'object'
      ? Object.values(payload.errors).flat().join(' ')
      : ''
  const fallback =
    status === 401
      ? 'Your session has ended. Please sign in again.'
      : status === 429
        ? 'Too many attempts. Please wait a minute and try again.'
        : 'Something went wrong. Please try again.'
  return (
    (typeof payload.error === 'string' && payload.error) ||
    (typeof payload.message === 'string' && payload.message) ||
    validation ||
    fallback
  )
}

function responseContentType(
  headers: AxiosResponseHeaders | RawAxiosResponseHeaders,
): string | null {
  const value =
    headers instanceof AxiosHeaders
      ? headers.get('content-type')
      : (headers['content-type'] ?? headers['Content-Type'])
  return typeof value === 'string' ? value : null
}

export async function request<T>(path: string, options: RequestInit = {}): Promise<T> {
  const headers = new AxiosHeaders()
  new Headers(options.headers).forEach((value, key) => headers.set(key, value))
  headers.set('Accept', 'application/json')
  if (options.body) headers.set('Content-Type', 'application/json')

  const config: AxiosRequestConfig = {
    url: path,
    method: options.method,
    headers,
    data: options.body,
    signal: options.signal ?? undefined,
  }

  try {
    const response = await apiClient.request<T>(config)
    if (response.status === 204) return null as T
    const contentType = responseContentType(response.headers)
    if (
      typeof contentType !== 'string' ||
      !contentType.includes('json') ||
      response.data === null ||
      response.data === undefined ||
      response.data === ''
    ) {
      throw new ApiError('The API returned an unexpected response. Check the API connection.', 502)
    }
    return response.data
  } catch (cause) {
    if (cause instanceof ApiError) throw cause
    if (isAxiosError(cause) && cause.response) {
      const contentType = responseContentType(cause.response.headers)
      const data =
        typeof contentType === 'string' && contentType.includes('json') ? cause.response.data : null
      throw new ApiError(responseMessage(data, cause.response.status), cause.response.status)
    }
    throw new ApiError(unreachableMessage, 0)
  }
}
