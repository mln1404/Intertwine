export interface Category {
  categoryId: number
  categoryName: string
  color: string
}
export interface Answer {
  answerId: number
  answerText: string
}
export interface QuestionSummary {
  questionId: number
  questionTitle: string
  fullQuestion: string
  categories: Category[]
}
export interface Question extends QuestionSummary {
  answers: Answer[]
}
export interface Profile {
  userProfileId: number
  identityUserId: string
  avatarName: string
  firstName: string
  middleName: string
  lastName: string
  creditBalance: number
}
export type ProfileInput = Pick<Profile, 'avatarName' | 'firstName' | 'middleName' | 'lastName'>
export interface CreditPackage {
  creditPackageId: number
  currencyCode: string
  amount: number
  credits: number
}
export interface AuthResult {
  succeeded: boolean
  error: string | null
  token: string | null
  userId: string | null
}
export interface CurrentAnswer {
  questionId: number
  answerId: number
  questionTitle: string
  fullQuestion: string
  answerText: string
  categories: Category[]
}
export interface PaymentHistory {
  userPaymentId: number
  dateCreated: string
  currencyCode: string
  amount: number
  sparksPurchased: number
  status: string
  paymentProvider: string
}
export interface DailyActivity {
  localDate: string
  dailyQuestionCreateOrUpdateUsed: boolean
  nonDailyQuestionsAnswered: number
  nonDailyQuestionsRemaining: number
}
