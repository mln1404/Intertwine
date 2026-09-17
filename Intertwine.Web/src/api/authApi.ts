import axios from 'axios'
import type { AuthResult } from '../models/question'
import { apiBaseUrl, apiTimeoutMs } from './apiConfig'

export const refreshClient = axios.create({
  baseURL: apiBaseUrl,
  withCredentials: true,
  timeout: apiTimeoutMs,
})

export async function refreshSession(): Promise<AuthResult> {
  const response = await refreshClient.post<AuthResult>('/api/Auth/refresh')
  return response.data
}
