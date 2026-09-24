import { request } from './http'
import type {
  CurrentAnswer,
  DailyActivity,
  DailyQuestionDiscovery,
  DiscoveryUsersPage,
  Question,
  QuestionSummary,
} from '../models/question'
import { answerRequest } from '../utils/answerRequest'
export const getQuestions = (categoryId?: number, signal?: AbortSignal) =>
  request<QuestionSummary[]>(
    `/api/questions${categoryId === undefined ? '' : `?categoryId=${categoryId}`}`,
    { signal },
  )
export const getQuestion = (id: number) => request<Question>(`/api/questions/${id}`)
export const getDailyQuestion = (date: string, signal?: AbortSignal) =>
  request<Question>(`/api/questions/daily?localDate=${date}`, { signal })
export const getCurrentAnswers = (signal?: AbortSignal) =>
  request<CurrentAnswer[]>('/api/user-answers/me', { signal })
export const getDailyActivity = (date: string, signal?: AbortSignal) =>
  request<DailyActivity>(`/api/daily-activity/me?localDate=${encodeURIComponent(date)}`, { signal })
export const getDailyQuestionDiscovery = (dailyQuestionId: number, date: string) =>
  request<DailyQuestionDiscovery>(
    `/api/daily-questions/${dailyQuestionId}/discovery?localDate=${encodeURIComponent(date)}`,
  )
export const getDiscoveryUsers = (
  dailyQuestionId: number,
  answerId: number,
  date: string,
  page: number,
  pageSize = 20,
) =>
  request<DiscoveryUsersPage>(
    `/api/daily-questions/${dailyQuestionId}/discovery/users?answerId=${answerId}` +
      `&localDate=${encodeURIComponent(date)}&page=${page}&pageSize=${pageSize}`,
  )
export function submitAnswer(
  questionId: number,
  answerId: number,
  date: string,
  key: string,
  spendSparks = false,
) {
  const submission = answerRequest(questionId, answerId, date, key, spendSparks)
  return request<{ message: string }>(submission.path, submission.options)
}
