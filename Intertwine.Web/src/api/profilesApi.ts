import type { PublicProfile } from '../models/question'
import { request } from './http'

export const getMyProfilePreview = () => request<PublicProfile>('/api/UserProfile/me/preview')

export const getPublicUserProfile = (userProfileId: number) =>
  request<PublicProfile>(`/api/UserProfile/${userProfileId}`)
