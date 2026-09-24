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
  dailyQuestionId?: number | null
  dailyQuestionDate?: string | null
  answers: Answer[]
}
export type DiscoveryAccessState =
  'Included' | 'SubscriptionRequired' | 'SparksRequired' | 'Unavailable'
export interface DiscoveryAnswerPool {
  answerId: number
  answerText: string
  accessState: DiscoveryAccessState
}
export interface DiscoveryDay {
  dailyQuestionId: number
  date: string
  questionTitle: string
  hasAnswered: boolean
  accessState: DiscoveryAccessState
}
export interface DailyQuestionDiscovery {
  dailyQuestionId: number
  date: string
  questionId: number
  questionTitle: string
  fullQuestion: string
  currentUserAnswerId: number
  currentUserAnswerText: string
  dateAccessState: DiscoveryAccessState
  answerPools: DiscoveryAnswerPool[]
  historicalAccess: DiscoveryDay[]
}
export interface DiscoveryUser {
  userProfileId: number
  avatarName: string
  personalityTypeCode: string | null
}
export interface DiscoveryUsersPage {
  accessState: DiscoveryAccessState
  page: number
  pageSize: number
  totalCount: number
  hasMore: boolean
  users: DiscoveryUser[]
}
export interface Profile {
  userProfileId: number
  identityUserId: string
  avatarName: string
  firstName: string
  middleName: string
  lastName: string
  creditBalance: number
  personalityTypeId: number | null
  personalityTypeCode: string | null
}
export type ProfileInput = Pick<
  Profile,
  'avatarName' | 'firstName' | 'middleName' | 'lastName' | 'personalityTypeId'
>
export interface PersonalityType {
  personalityTypeId: number
  code: string
  name: string | null
}
export interface PublicProfileAnswer {
  questionId: number
  questionTitle: string
  fullQuestion: string
  answerId: number
  answerText: string
  categories: Category[]
}
export interface PublicProfile {
  userProfileId: number
  avatarName: string
  personalityTypeCode: string | null
  answeredQuestions: PublicProfileAnswer[]
}
export interface CreditPackage {
  creditPackageId: number
  currencyCode: string
  amount: number
  credits: number
}
export interface Currency {
  code: string
  symbol: string
  name: string
  decimalPlaces: number
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
