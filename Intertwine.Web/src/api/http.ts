import axios, { AxiosHeaders, isAxiosError } from 'axios'
import type {
  AxiosRequestConfig,
  AxiosResponseHeaders,
  InternalAxiosRequestConfig,
  RawAxiosResponseHeaders,
} from 'axios'
import { useAuthStore } from '../stores/authStore'
import { pinia } from '../stores/pinia'
import { apiBaseUrl, apiTimeoutMs } from './apiConfig'
import { refreshSession } from './authApi'

export class ApiError extends Error {
  status: number
  constructor(message: string, status: number) {
    super(message)
    this.status = status
  }
}

const unreachableMessage = 'We couldn’t reach Intertwine. Check your connection and try again.'
const sessionEndedMessage = 'Your session has ended. Please sign in again.'

interface RetryConfig extends InternalAxiosRequestConfig {
  _retriedAfterRefresh?: boolean
}

let refreshPromise: Promise<string> | null = null

export const apiClient = axios.create({
  baseURL: apiBaseUrl,
  withCredentials: true,
  timeout: apiTimeoutMs,
})

apiClient.interceptors.request.use((config) => {
  const token = useAuthStore(pinia).accessToken
  if (token) config.headers.set('Authorization', `Bearer ${token}`)
  return config
})

function isAuthenticationEndpoint(url: string): boolean {
  const path = new URL(url, 'https://intertwine.invalid').pathname.toLowerCase()
  return /^\/api\/auth\/(login|register|refresh)\/?$/.test(path)
}

async function renewAccessToken(previousToken: string | null): Promise<string> {
  const auth = useAuthStore(pinia)
  try {
    const result = await refreshSession()
    if (!result.succeeded || !result.token || !result.userId) {
      if (auth.accessToken === previousToken) auth.clearSession()
      throw new ApiError(sessionEndedMessage, 401)
    }
    // A logout or a new login while refresh was pending must not be overwritten.
    if (auth.accessToken !== previousToken) {
      if (auth.accessToken) return auth.accessToken
      throw new ApiError(sessionEndedMessage, 401)
    }
    auth.setSession(result.token, result.userId)
    return result.token
  } catch (cause) {
    if (isAxiosError(cause) && cause.response?.status === 401) {
      if (auth.accessToken === previousToken) auth.clearSession()
      throw new ApiError(sessionEndedMessage, 401)
    }
    throw cause
  }
}

function sharedRefresh(): Promise<string> {
  if (!refreshPromise) {
    const previousToken = useAuthStore(pinia).accessToken
    refreshPromise = renewAccessToken(previousToken).finally(() => {
      refreshPromise = null
    })
  }
  return refreshPromise
}

apiClient.interceptors.response.use(undefined, async (cause: unknown) => {
  if (!isAxiosError(cause) || cause.response?.status !== 401 || !cause.config) {
    return Promise.reject(cause)
  }
  const original = cause.config as RetryConfig
  if (original._retriedAfterRefresh || isAuthenticationEndpoint(original.url ?? '')) {
    return Promise.reject(cause)
  }

  original._retriedAfterRefresh = true
  const auth = useAuthStore(pinia)
  if (!auth.accessToken) return Promise.reject(cause)
  const authorization = original.headers.get('Authorization')
  const sentToken =
    typeof authorization === 'string' && authorization.startsWith('Bearer ')
      ? authorization.slice(7)
      : null

  // A different request may have refreshed the token before this 401 arrived.
  if (sentToken === auth.accessToken) await sharedRefresh()
  if (!auth.accessToken) throw new ApiError(sessionEndedMessage, 401)
  return apiClient.request(original)
})

function responseMessage(data: unknown, status: number): string {
  const payload = data && typeof data === 'object' ? (data as Record<string, unknown>) : {}
  const validation =
    payload.errors && typeof payload.errors === 'object'
      ? Object.values(payload.errors).flat().join(' ')
      : ''
  const fallback =
    status === 401
      ? sessionEndedMessage
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
