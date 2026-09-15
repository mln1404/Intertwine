import { computed, ref } from 'vue'
import { ApiError, request } from '../api/http'
import {
  getCurrentAnswers,
  getDailyActivity,
  getDailyQuestion,
  getQuestion,
  getQuestions,
  submitAnswer,
} from '../api/questionsApi'
import { demoPackages, demoProfile, demoQuestions } from '../data/demo'
import type {
  AuthResult,
  CreditPackage,
  CurrentAnswer,
  DailyActivity,
  Profile,
  ProfileInput,
  Question,
  QuestionSummary,
  PaymentHistory,
} from '../models/question'
import { localDate } from '../utils/answerRequest'

const demo = ref(
  import.meta.env.VITE_DEMO_MODE !== 'false' && !sessionStorage.getItem('intertwine.token'),
)
const signedIn = ref(Boolean(sessionStorage.getItem('intertwine.token')))
const profile = ref<Profile | null>(null)
const daily = ref<Question | null>(null)
const questions = ref<QuestionSummary[]>([])
const balance = ref<number | null>(null)
const packages = ref<CreditPackage[]>([])
const currentAnswers = ref<CurrentAnswer[]>([])
const demoPayments = ref<PaymentHistory[]>([])
const loading = ref(false)
const error = ref('')
const authOpen = ref(false)
const today = ref(localDate())
const emptyActivity = () => ({
  daily: false,
  count: 0,
  remaining: 2,
  answers: {} as Record<number, number>,
})
const activity = ref(emptyActivity())
const canUseAccount = computed(() => demo.value || signedIn.value)
const displayName = computed(
  () => profile.value?.firstName || (signedIn.value ? 'friend' : 'there'),
)
let generation = 0
const pendingAnswers = new Map<string, string>()

function reportError(cause: unknown) {
  if (cause instanceof ApiError && cause.status === 401) {
    sessionStorage.removeItem('intertwine.token')
    sessionStorage.removeItem('intertwine.user')
    signedIn.value = false
    profile.value = null
    balance.value = null
    questions.value = []
    packages.value = []
    activity.value = emptyActivity()
    currentAnswers.value = []
    demoPayments.value = []
    authOpen.value = true
  }
  return cause instanceof Error ? cause.message : 'Something went wrong. Please try again.'
}

async function refresh() {
  const version = ++generation
  loading.value = true
  error.value = ''
  const date = localDate()
  if (date !== today.value) activity.value = emptyActivity()
  today.value = date
  if (demo.value) {
    profile.value ??= { ...demoProfile }
    daily.value = demoQuestions[0]!
    questions.value = demoQuestions
    balance.value ??= 120
    packages.value = demoPackages
    loading.value = false
    return
  }
  const results = await Promise.allSettled([
    getDailyQuestion(today.value),
    ...(signedIn.value
      ? [
          getQuestions(),
          request<Profile>('/api/UserProfile/me'),
          getCurrentAnswers(),
          getDailyActivity(today.value),
        ]
      : []),
  ])
  if (version !== generation) return
  const dailyResult = results[0]!
  if (dailyResult.status === 'fulfilled') daily.value = dailyResult.value as Question
  else if (!(dailyResult.reason instanceof ApiError && dailyResult.reason.status === 404)) {
    daily.value = null
    error.value = reportError(dailyResult.reason)
  } else daily.value = null
  const listResult = results[1]
  if (listResult?.status === 'fulfilled' && signedIn.value)
    questions.value = listResult.value as QuestionSummary[]
  else if (listResult?.status === 'rejected') error.value = reportError(listResult.reason)
  const profileResult = results[2]
  if (profileResult?.status === 'fulfilled' && signedIn.value) {
    profile.value = profileResult.value as Profile
    balance.value = profile.value.creditBalance
  } else if (profileResult?.status === 'rejected') {
    if (profileResult.reason instanceof ApiError && profileResult.reason.status === 404)
      profile.value = null
    else error.value = reportError(profileResult.reason)
  }
  const answersResult = results[3]
  if (answersResult?.status === 'fulfilled' && signedIn.value) {
    const currentAnswers = answersResult.value as CurrentAnswer[]
    activity.value.answers = Object.fromEntries(
      currentAnswers.map((answer) => [answer.questionId, answer.answerId]),
    )
    setCurrentAnswers(currentAnswers)
  } else if (answersResult?.status === 'rejected') error.value = reportError(answersResult.reason)
  const activityResult = results[4]
  if (activityResult?.status === 'fulfilled' && signedIn.value) {
    const dailyActivity = activityResult.value as DailyActivity
    activity.value.daily = dailyActivity.dailyQuestionCreateOrUpdateUsed
    activity.value.count = dailyActivity.nonDailyQuestionsAnswered
    activity.value.remaining = dailyActivity.nonDailyQuestionsRemaining
  } else if (activityResult?.status === 'rejected') error.value = reportError(activityResult.reason)
  loading.value = false
}
async function detail(id: number) {
  if (demo.value) return demoQuestions.find((q) => q.questionId === id)!
  return getQuestion(id)
}
function setCurrentAnswers(answers: CurrentAnswer[]) {
  currentAnswers.value = answers
}
async function answer(question: Question, answerId: number, date: string, spendSparks = false) {
  if (!canUseAccount.value) {
    authOpen.value = true
    return false
  }
  if (date !== localDate()) {
    activity.value = emptyActivity()
    await refresh()
    throw new Error('A new day has started. Please reopen the question before answering.')
  }
  const isDaily = question.questionId === daily.value?.questionId
  if (demo.value) {
    if (isDaily && activity.value.daily)
      throw new Error('You’ve already answered today’s Daily Question. Come back tomorrow.')
    if (!isDaily && activity.value.count >= 2 && !spendSparks)
      throw new Error('You’ve used your two question actions today. Come back tomorrow.')
    if (!isDaily && activity.value.answers[question.questionId] === answerId)
      throw new Error('This is already your current answer.')
    if (!isDaily && activity.value.count >= 2) {
      if ((balance.value ?? 0) < 10)
        throw new Error('You need 10 Sparks. Get more Sparks and try again.')
      balance.value = (balance.value ?? 0) - 10
      if (profile.value) profile.value.creditBalance = balance.value
    }
  } else {
    const fingerprint = `${sessionStorage.getItem('intertwine.user')}:${question.questionId}:${answerId}:${date}:${spendSparks}`
    const key = pendingAnswers.get(fingerprint) ?? crypto.randomUUID()
    pendingAnswers.set(fingerprint, key)
    try {
      await submitAnswer(question.questionId, answerId, date, key, spendSparks)
      pendingAnswers.delete(fingerprint)
    } catch (cause) {
      // Preserve keys for ambiguous network/server/conflict retries.
      if (cause instanceof ApiError && [400, 401, 403, 404, 429].includes(cause.status))
        pendingAnswers.delete(fingerprint)
      throw new Error(reportError(cause))
    }
  }
  currentAnswers.value = [
    ...currentAnswers.value.filter((x) => x.questionId !== question.questionId),
    {
      questionId: question.questionId,
      answerId,
      questionTitle: question.questionTitle,
      fullQuestion: question.fullQuestion,
      categories: question.categories,
      answerText: question.answers.find((x) => x.answerId === answerId)?.answerText ?? '',
    },
  ]
  activity.value.answers[question.questionId] = answerId
  if (isDaily) activity.value.daily = true
  else {
    activity.value.count++
    activity.value.remaining = Math.max(0, activity.value.remaining - 1)
  }
  if (!demo.value) await refresh()
  return true
}
async function authenticate(
  input: { email: string; password: string; firstName: string; lastName: string },
  register: boolean,
) {
  const result = await request<AuthResult>(`/api/Auth/${register ? 'register' : 'login'}`, {
    method: 'POST',
    body: JSON.stringify(input),
  })
  if (!result.succeeded) throw new Error(result.error || 'Unable to sign in.')
  if (register) return false
  if (!result.token) throw new Error('No session was returned. Please sign in again.')
  sessionStorage.setItem('intertwine.token', result.token)
  sessionStorage.setItem('intertwine.user', result.userId || '')
  resetState()
  demo.value = false
  signedIn.value = true
  authOpen.value = false
  await refresh()
  return true
}
function resetState() {
  generation++
  profile.value = null
  daily.value = null
  questions.value = []
  packages.value = []
  balance.value = null
  activity.value = emptyActivity()
  pendingAnswers.clear()
  currentAnswers.value = []
  demoPayments.value = []
  error.value = ''
}
async function signOut() {
  sessionStorage.removeItem('intertwine.token')
  sessionStorage.removeItem('intertwine.user')
  signedIn.value = false
  demo.value = false
  resetState()
  await refresh()
}
async function startDemo() {
  sessionStorage.removeItem('intertwine.token')
  sessionStorage.removeItem('intertwine.user')
  signedIn.value = false
  resetState()
  demo.value = true
  authOpen.value = false
  await refresh()
}
async function saveProfile(input: ProfileInput) {
  if (demo.value) profile.value = { ...demoProfile, ...input, creditBalance: balance.value ?? 0 }
  else {
    profile.value = await request<Profile>('/api/UserProfile/me', {
      method: 'PUT',
      body: JSON.stringify(input),
    })
    balance.value = profile.value.creditBalance
  }
}
async function deleteProfile() {
  if (demo.value) {
    profile.value = null
    return
  }
  await request<void>('/api/UserProfile/me', { method: 'DELETE' })
  await signOut()
}
async function loadWallet(currency: string) {
  if (demo.value) {
    packages.value = currency === 'AUD' ? demoPackages : []
    return
  }
  const version = generation
  const [wallet, offers] = await Promise.all([
    request<{ creditBalance: number }>('/api/wallet'),
    request<CreditPackage[]>(`/api/credit-packages?currencyCode=${encodeURIComponent(currency)}`),
  ])
  if (version !== generation) return
  balance.value = wallet.creditBalance
  if (profile.value) profile.value.creditBalance = wallet.creditBalance
  packages.value = offers
}
async function topUp(offer: CreditPackage) {
  if (demo.value) {
    balance.value = (balance.value ?? 0) + offer.credits
    if (profile.value) profile.value.creditBalance = balance.value
    demoPayments.value.unshift({
      userPaymentId: Date.now(),
      dateCreated: new Date().toISOString(),
      currencyCode: offer.currencyCode,
      amount: offer.amount,
      sparksPurchased: offer.credits,
      status: 'Completed',
      paymentProvider: 'IntertwineDemo',
    })
    return
  }
  const result = await request<{ creditBalance: number }>('/api/wallet/top-up', {
    method: 'POST',
    body: JSON.stringify({ creditPackageId: offer.creditPackageId }),
  })
  balance.value = result.creditBalance
  if (profile.value) profile.value.creditBalance = result.creditBalance
}
async function loadPayments(page = 1): Promise<PaymentHistory[]> {
  if (demo.value) return demoPayments.value.slice((page - 1) * 20, page * 20)
  return request<PaymentHistory[]>(`/api/wallet/payments?page=${page}`)
}
export function useIntertwine() {
  return {
    demo,
    signedIn,
    profile,
    daily,
    questions,
    balance,
    packages,
    currentAnswers,
    loadPayments,
    loading,
    error,
    authOpen,
    today,
    activity,
    canUseAccount,
    displayName,
    refresh,
    detail,
    answer,
    authenticate,
    signOut,
    startDemo,
    saveProfile,
    deleteProfile,
    loadWallet,
    topUp,
    reportError,
  }
}
