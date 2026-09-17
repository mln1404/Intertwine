import { computed, ref } from 'vue'
import { storeToRefs } from 'pinia'
import { ApiError, request } from '../api/http'
import { useAuthStore } from '../stores/authStore'
import { pinia } from '../stores/pinia'
import {
  getCurrentAnswers,
  getDailyActivity,
  getDailyQuestion,
  getQuestion,
  getQuestions,
  submitAnswer,
} from '../api/questionsApi'
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
  Currency,
} from '../models/question'
import { localDate } from '../utils/answerRequest'

const auth = useAuthStore(pinia)
const { signedIn } = storeToRefs(auth)
const profile = ref<Profile | null>(null)
const profileLoading = ref(false)
const profileLoaded = ref(false)
const profileError = ref('')
const currentAnswersLoading = ref(false)
const currentAnswersLoaded = ref(false)
const currentAnswersError = ref('')
const daily = ref<Question | null>(null)
const questions = ref<QuestionSummary[]>([])
const balance = ref<number | null>(null)
const packages = ref<CreditPackage[]>([])
const currencies = ref<Currency[]>([])
const currentAnswers = ref<CurrentAnswer[]>([])
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
const canUseAccount = signedIn
const displayName = computed(
  () => profile.value?.firstName || (signedIn.value ? 'friend' : 'there'),
)
let generation = 0
let questionGeneration = 0
let profileRequest: Promise<Profile | null> | null = null
const pendingAnswers = new Map<string, string>()

function reportError(cause: unknown) {
  if (cause instanceof ApiError && cause.status === 401) {
    // Protected requests reach this point only after Axios has attempted refresh and one retry.
    auth.clearSession()
    profile.value = null
    profileLoaded.value = false
    profileError.value = ''
    balance.value = null
    questions.value = []
    packages.value = []
    activity.value = emptyActivity()
    currentAnswers.value = []
    currentAnswersLoading.value = false
    currentAnswersLoaded.value = false
    currentAnswersError.value = ''
    currencies.value = []
    authOpen.value = true
  }
  return cause instanceof Error ? cause.message : 'Something went wrong. Please try again.'
}

async function loadProfile(force = false) {
  if (!auth.accessToken) {
    profile.value = null
    profileLoaded.value = true
    profileError.value = ''
    return null
  }
  if (!force && profileLoaded.value) return profile.value
  if (profileRequest) return profileRequest

  const version = generation
  profileLoading.value = true
  profileError.value = ''
  let pending!: Promise<Profile | null>
  pending = (async () => {
    try {
      const result = await request<Profile>('/api/UserProfile/me')
      if (version !== generation) return null
      profile.value = result
      balance.value = result.creditBalance
      profileLoaded.value = true
      return result
    } catch (cause) {
      if (version !== generation) return null
      profile.value = null
      balance.value = null
      profileLoaded.value = true
      if (!(cause instanceof ApiError && cause.status === 404)) {
        profileError.value = reportError(cause)
        error.value = profileError.value
      }
      return null
    } finally {
      if (version === generation) profileLoading.value = false
      if (profileRequest === pending) profileRequest = null
    }
  })()
  profileRequest = pending
  return pending
}

async function loadCurrentAnswers(signal?: AbortSignal, force = false) {
  await loadProfile()
  if (signal?.aborted || !profile.value) return
  if (!force && currentAnswersLoaded.value) return

  currentAnswersLoading.value = true
  currentAnswersError.value = ''
  try {
    setCurrentAnswers(await getCurrentAnswers(signal))
    currentAnswersLoaded.value = true
  } catch (cause) {
    if (!signal?.aborted) {
      currentAnswersError.value = reportError(cause)
      currentAnswersLoaded.value = true
      error.value = currentAnswersError.value
    }
  } finally {
    currentAnswersLoading.value = false
  }
}

async function refresh(signal?: AbortSignal) {
  const version = ++questionGeneration
  loading.value = true
  error.value = ''
  const date = localDate()
  if (date !== today.value) activity.value = emptyActivity()
  today.value = date
  if (!signedIn.value) {
    daily.value = null
    questions.value = []
    profile.value = null
    balance.value = null
    currentAnswers.value = []
    activity.value = emptyActivity()
    loading.value = false
    return
  }

  await loadProfile()
  if (signal?.aborted || version !== questionGeneration) {
    if (version === questionGeneration) loading.value = false
    return
  }
  if (!profile.value) {
    loading.value = false
    return
  }

  const results = await Promise.allSettled([
    getDailyQuestion(today.value, signal),
    getQuestions(undefined, signal),
    getCurrentAnswers(signal),
    getDailyActivity(today.value, signal),
  ])
  if (signal?.aborted || version !== questionGeneration) {
    if (version === questionGeneration) loading.value = false
    return
  }
  const dailyResult = results[0]!
  if (dailyResult.status === 'fulfilled') daily.value = dailyResult.value as Question
  else if (!(dailyResult.reason instanceof ApiError && dailyResult.reason.status === 404)) {
    daily.value = null
    error.value = reportError(dailyResult.reason)
  } else daily.value = null
  const listResult = results[1]
  if (listResult?.status === 'fulfilled') questions.value = listResult.value as QuestionSummary[]
  else if (listResult?.status === 'rejected') error.value = reportError(listResult.reason)
  const answersResult = results[2]
  if (answersResult?.status === 'fulfilled') {
    const currentAnswers = answersResult.value as CurrentAnswer[]
    activity.value.answers = Object.fromEntries(
      currentAnswers.map((answer) => [answer.questionId, answer.answerId]),
    )
    setCurrentAnswers(currentAnswers)
    currentAnswersLoaded.value = true
    currentAnswersError.value = ''
  } else if (answersResult?.status === 'rejected') {
    currentAnswersError.value = reportError(answersResult.reason)
    currentAnswersLoaded.value = true
    error.value = currentAnswersError.value
  }
  const activityResult = results[3]
  if (activityResult?.status === 'fulfilled') {
    const dailyActivity = activityResult.value as DailyActivity
    activity.value.daily = dailyActivity.dailyQuestionCreateOrUpdateUsed
    activity.value.count = dailyActivity.nonDailyQuestionsAnswered
    activity.value.remaining = dailyActivity.nonDailyQuestionsRemaining
  } else if (activityResult?.status === 'rejected') error.value = reportError(activityResult.reason)
  loading.value = false
}
async function detail(id: number) {
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
  const fingerprint = `${auth.userId ?? ''}:${question.questionId}:${answerId}:${date}:${spendSparks}`
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
  await refresh()
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
  auth.setSession(result.token, result.userId)
  resetState()
  await loadProfile()
  authOpen.value = false
  return true
}
function resetState() {
  generation++
  questionGeneration++
  profile.value = null
  profileLoading.value = false
  profileLoaded.value = false
  profileError.value = ''
  currentAnswersLoading.value = false
  currentAnswersLoaded.value = false
  currentAnswersError.value = ''
  profileRequest = null
  daily.value = null
  questions.value = []
  packages.value = []
  currencies.value = []
  balance.value = null
  activity.value = emptyActivity()
  pendingAnswers.clear()
  currentAnswers.value = []
  error.value = ''
}
async function signOut() {
  auth.clearSession()
  resetState()
}
async function saveProfile(input: ProfileInput) {
  profile.value = await request<Profile>('/api/UserProfile/me', {
    method: 'PUT',
    body: JSON.stringify(input),
  })
  profileLoaded.value = true
  profileError.value = ''
  balance.value = profile.value.creditBalance
}
async function createProfile(input: ProfileInput) {
  profile.value = await request<Profile>('/api/UserProfile/me', {
    method: 'POST',
    body: JSON.stringify(input),
  })
  profileLoaded.value = true
  profileError.value = ''
  balance.value = profile.value.creditBalance
  currentAnswers.value = []
  currentAnswersLoaded.value = true
  currentAnswersError.value = ''
}
async function deactivateProfile() {
  await request<void>('/api/UserProfile/me/deactivate', { method: 'POST' })
  await signOut()
}
async function loadWallet(currencyCode = '') {
  const version = generation
  const [wallet, availableCurrencies] = await Promise.all([
    request<{ creditBalance: number }>('/api/wallet'),
    request<Currency[]>('/api/currencies'),
  ])
  if (version !== generation) return
  currencies.value = availableCurrencies
  const selectedCurrency = availableCurrencies.some((x) => x.code === currencyCode)
    ? currencyCode
    : (availableCurrencies[0]?.code ?? '')
  const offers = selectedCurrency
    ? await request<CreditPackage[]>(
        `/api/credit-packages?currencyCode=${encodeURIComponent(selectedCurrency)}`,
      )
    : []
  if (version !== generation) return
  balance.value = wallet.creditBalance
  if (profile.value) profile.value.creditBalance = wallet.creditBalance
  packages.value = offers
  return selectedCurrency
}
async function topUp(offer: CreditPackage) {
  const result = await request<{ creditBalance: number }>('/api/wallet/top-up', {
    method: 'POST',
    body: JSON.stringify({ creditPackageId: offer.creditPackageId }),
  })
  balance.value = result.creditBalance
  if (profile.value) profile.value.creditBalance = result.creditBalance
}
async function loadPayments(page = 1): Promise<PaymentHistory[]> {
  return request<PaymentHistory[]>(`/api/wallet/payments?page=${page}`)
}
export function useIntertwine() {
  return {
    signedIn,
    profile,
    profileLoading,
    profileLoaded,
    profileError,
    currentAnswersLoading,
    currentAnswersLoaded,
    currentAnswersError,
    daily,
    questions,
    balance,
    packages,
    currencies,
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
    loadProfile,
    loadCurrentAnswers,
    detail,
    answer,
    authenticate,
    signOut,
    createProfile,
    saveProfile,
    deactivateProfile,
    loadWallet,
    topUp,
    reportError,
  }
}
