import { request } from './http'
import type { CurrentAnswer, DailyActivity, Question, QuestionSummary } from '../models/question'
import { answerRequest } from '../utils/answerRequest'
export const getQuestions = (categoryId?: number) =>
  request<QuestionSummary[]>(
    `/api/questions${categoryId === undefined ? '' : `?categoryId=${categoryId}`}`,
  )
export const getQuestion = (id: number) => request<Question>(`/api/questions/${id}`)
export const getDailyQuestion = (date: string) =>
  request<Question>(`/api/questions/daily?localDate=${date}`)
export const getCurrentAnswers = () => request<CurrentAnswer[]>('/api/user-answers/me')
export const getDailyActivity = (date: string) =>
  request<DailyActivity>(`/api/daily-activity/me?localDate=${encodeURIComponent(date)}`)
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
