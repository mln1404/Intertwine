import { request } from './http'
import type { CurrentAnswer, DailyActivity, Question, QuestionSummary } from '../models/question'
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
