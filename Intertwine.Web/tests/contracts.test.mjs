import { test, after } from 'node:test'
import assert from 'node:assert/strict'
import { mkdtempSync, readFileSync, mkdirSync, writeFileSync, rmSync } from 'node:fs'
import { tmpdir } from 'node:os'
import { basename, dirname, join, resolve } from 'node:path'
import { pathToFileURL } from 'node:url'
import ts from 'typescript'
import { AxiosError, AxiosHeaders } from 'axios'

// Exercise the real TypeScript API and composable modules with isolated network
// responses. No database, real account, Redis, or payment provider is contacted.
const runtime = mkdtempSync(join(tmpdir(), 'intertwine-tests-'))
const files = [
  'models/question',
  'utils/answerRequest',
  'utils/profilePresentation',
  'stores/pinia',
  'stores/authStore',
  'api/apiConfig',
  'api/authApi',
  'api/http',
  'api/questionsApi',
  'composables/useIntertwine',
]
const vueUrl = pathToFileURL(resolve('node_modules/vue/dist/vue.runtime.esm-bundler.js')).href
const piniaUrl = pathToFileURL(resolve('node_modules/pinia/dist/pinia.js')).href
const axiosUrl = pathToFileURL(resolve('node_modules/axios/index.js')).href
for (const file of files) {
  const source = readFileSync(resolve(`src/${file}.ts`), 'utf8').replaceAll(
    'import.meta.env',
    '({ VITE_API_URL: "" })',
  )
  let code = ts.transpileModule(source, {
    compilerOptions: { target: ts.ScriptTarget.ES2022, module: ts.ModuleKind.ESNext },
  }).outputText
  code = code
    .replace(/from ['"](\.[^'"]+)['"]/g, "from '$1.mjs'")
    .replace(/from ['"]vue['"]/g, `from '${vueUrl}'`)
    .replace(/from ['"]pinia['"]/g, `from '${piniaUrl}'`)
    .replace(/from ['"]axios['"]/g, `from '${axiosUrl}'`)
  const destination = join(runtime, `${file}.mjs`)
  mkdirSync(dirname(destination), { recursive: true })
  writeFileSync(destination, code)
}
const storage = new Map()
globalThis.sessionStorage = {
  getItem: (key) => storage.get(key) ?? null,
  setItem: (key, value) => storage.set(key, value),
  removeItem: (key) => storage.delete(key),
}
const calls = []
let handler
const json = (data, status = 200) =>
  new Response(JSON.stringify(data), { status, headers: { 'Content-Type': 'application/json' } })
const { localDate, answerRequest } = await import(
  pathToFileURL(join(runtime, 'utils/answerRequest.mjs'))
)
const { avatarInitials, categoryAccent } = await import(
  pathToFileURL(join(runtime, 'utils/profilePresentation.mjs'))
)
const { useIntertwine } = await import(
  pathToFileURL(join(runtime, 'composables/useIntertwine.mjs'))
)
const { getDailyQuestionDiscovery, getDiscoveryUsers } = await import(
  pathToFileURL(join(runtime, 'api/questionsApi.mjs'))
)
const { request, ApiError, apiClient } = await import(pathToFileURL(join(runtime, 'api/http.mjs')))
const { refreshClient, refreshSession } = await import(
  pathToFileURL(join(runtime, 'api/authApi.mjs'))
)
const { useAuthStore } = await import(pathToFileURL(join(runtime, 'stores/authStore.mjs')))
const { pinia } = await import(pathToFileURL(join(runtime, 'stores/pinia.mjs')))
const { createPinia } = await import('pinia')
async function mockAdapter(config) {
  const call = {
    path: config.url,
    method: (config.method || 'GET').toUpperCase(),
    headers: config.headers,
    body: config.data,
    signal: config.signal,
    withCredentials: config.withCredentials,
    timeout: config.timeout,
  }
  calls.push(call)
  const response = await handler(call)
  const contentType = response.headers.get('Content-Type')
  const result = {
    data: contentType?.includes('json') ? await response.json() : await response.text(),
    status: response.status,
    statusText: response.statusText,
    headers: new AxiosHeaders(contentType ? { 'content-type': contentType } : {}),
    config,
    request: null,
  }
  if (!response.ok)
    throw new AxiosError(
      `Request failed with status code ${response.status}`,
      undefined,
      config,
      null,
      result,
    )
  return result
}
apiClient.defaults.adapter = mockAdapter
refreshClient.defaults.adapter = mockAdapter
const app = useIntertwine()
const question = {
  questionId: 7,
  questionTitle: 'Connection',
  fullQuestion: 'What matters?',
  categories: [],
  answers: [{ answerId: 70, answerText: 'Kindness' }],
}
const profile = {
  userProfileId: 3,
  identityUserId: 'test-user',
  firstName: 'Test',
  middleName: '',
  lastName: 'Person',
  avatarName: 'test',
  creditBalance: 45,
  personalityTypeId: null,
  personalityTypeCode: null,
}
const credentials = {
  email: 'test@example.invalid',
  password: 'Test-only1!',
  firstName: 'Test',
  lastName: 'Person',
}
function standard(call) {
  if (call.path === '/api/Auth/login')
    return json({ succeeded: true, token: 'test-token', userId: 'test-user' })
  if (call.path === '/api/Auth/logout') return new Response(null, { status: 204 })
  if (call.path.startsWith('/api/questions/daily')) return json(question)
  if (call.path === '/api/questions') return json([{ ...question, answers: undefined }])
  if (call.path === '/api/UserProfile/me') return json(profile)
  if (call.path === '/api/user-answers/me')
    return json([
      {
        questionId: 8,
        answerId: 80,
        questionTitle: 'A saved question',
        fullQuestion: 'What did you choose?',
        answerText: 'The saved answer',
        categories: [],
      },
    ])
  if (call.path.startsWith('/api/daily-activity/me'))
    return json({
      localDate: localDate(),
      dailyQuestionCreateOrUpdateUsed: false,
      nonDailyQuestionsAnswered: 0,
      nonDailyQuestionsRemaining: 2,
    })
  throw new Error(`Unexpected request: ${call.method || 'GET'} ${call.path}`)
}

test('local calendar date is correct on both sides of the UTC boundary', () => {
  const previous = process.env.TZ
  try {
    process.env.TZ = 'Pacific/Auckland'
    assert.equal(localDate(new Date('2026-09-15T12:30:00Z')), '2026-09-16')
    process.env.TZ = 'America/Los_Angeles'
    assert.equal(localDate(new Date('2026-09-15T02:30:00Z')), '2026-09-14')
  } finally {
    if (previous === undefined) delete process.env.TZ
    else process.env.TZ = previous
  }
})
test('answer request puts the date in the query and key in its header', () => {
  assert.deepEqual(answerRequest(7, 70, '2026-09-15', 'retry-key'), {
    path: '/api/questions/7/answer?localDate=2026-09-15',
    options: {
      method: 'POST',
      headers: { 'Idempotency-Key': 'retry-key' },
      body: '{"answerId":70}',
    },
  })
})
test('login loads only the profile before the question screen requests question data', async () => {
  handler = () => json({ succeeded: true, userId: 'test-user', token: null })
  assert.equal(await app.authenticate(credentials, true), false)
  assert.equal(sessionStorage.getItem('intertwine.token'), null)
  handler = standard
  assert.equal(await app.authenticate(credentials, false), true)
  assert.equal(app.profile.value.firstName, 'Test')
  assert.equal(app.profile.value.creditBalance, 45)
  assert.equal(app.balance.value, 45)
  assert.equal(
    calls.some((call) => call.path === '/api/questions'),
    false,
  )
  assert.equal(
    calls.some((call) => call.path.startsWith('/api/questions/daily')),
    false,
  )

  await app.refresh()

  assert.equal(app.questions.value[0].answers, undefined)
  assert.equal(app.activity.value.answers[8], 80)
  assert.equal(app.activity.value.daily, false)
  assert.equal(app.activity.value.count, 0)
  assert.equal(app.activity.value.remaining, 2)
  assert.equal(
    calls.find((call) => call.path === '/api/questions').headers.get('Authorization'),
    'Bearer test-token',
  )
  assert.equal(apiClient.defaults.withCredentials, true)
  assert.equal(apiClient.defaults.timeout, 15000)
})
test('legacy account can create its missing profile without loading profile-dependent data first', async () => {
  await app.signOut()
  calls.length = 0
  let hasProfile = false
  handler = (call) => {
    if (call.path === '/api/Auth/login')
      return json({ succeeded: true, token: 'legacy-token', userId: 'legacy-user' })
    if (call.path === '/api/Auth/logout') return new Response(null, { status: 204 })
    if (call.path.startsWith('/api/questions/daily')) return json(question)
    if (call.path === '/api/questions') return json([{ ...question, answers: undefined }])
    if (call.path === '/api/UserProfile/me' && call.method === 'POST') {
      hasProfile = true
      assert.equal(JSON.parse(call.body).avatarName, 'legacy')
      return json({ ...profile, identityUserId: 'legacy-user', avatarName: 'legacy' }, 201)
    }
    if (call.path === '/api/UserProfile/me')
      return hasProfile
        ? json({ ...profile, identityUserId: 'legacy-user', avatarName: 'legacy' })
        : json({}, 404)
    if (call.path === '/api/user-answers/me') return json([])
    if (call.path.startsWith('/api/daily-activity/me'))
      return json({
        localDate: localDate(),
        dailyQuestionCreateOrUpdateUsed: false,
        nonDailyQuestionsAnswered: 0,
        nonDailyQuestionsRemaining: 2,
      })
    throw new Error(`Unexpected request: ${call.method || 'GET'} ${call.path}`)
  }

  assert.equal(await app.authenticate(credentials, false), true)
  assert.equal(app.profile.value, null)
  assert.equal(app.error.value, '')
  assert.equal(
    calls.some((call) => call.path === '/api/user-answers/me'),
    false,
  )
  assert.equal(
    calls.some((call) => call.path.startsWith('/api/daily-activity/me')),
    false,
  )

  await app.createProfile({
    firstName: 'Legacy',
    lastName: 'Person',
    middleName: '',
    avatarName: 'legacy',
    personalityTypeId: null,
  })

  assert.equal(app.profile.value.avatarName, 'legacy')
  assert.equal(
    calls.some((call) => call.path === '/api/user-answers/me'),
    false,
  )

  await app.loadCurrentAnswers()

  assert.equal(
    calls.some((call) => call.path === '/api/user-answers/me'),
    false,
  )
  assert.equal(
    calls.some((call) => call.path === '/api/questions'),
    false,
  )
  assert.equal(
    calls.some((call) => call.path.startsWith('/api/questions/daily')),
    false,
  )
})
test('question answers are fetched from the detail endpoint', async () => {
  handler = (call) => {
    assert.equal(call.path, '/api/questions/7')
    return json(question)
  }
  assert.equal((await app.detail(7)).answers[0].answerId, 70)
})
test('ambiguous answer failures retain one idempotency key across retries', async () => {
  const submissions = []
  handler = (call) => {
    if (call.path.startsWith('/api/daily-activity/me'))
      return json({
        dailyQuestionCreateOrUpdateUsed: true,
        nonDailyQuestionsAnswered: 0,
        nonDailyQuestionsRemaining: 2,
      })
    if (call.method !== 'POST') return standard(call)
    submissions.push(call)
    return submissions.length === 1
      ? json({ message: 'Temporary failure' }, 500)
      : submissions.length === 2
        ? json({ message: 'Still processing' }, 409)
        : json({ message: 'Saved' })
  }
  await assert.rejects(app.answer(question, 70, localDate()), /Temporary failure/)
  await assert.rejects(app.answer(question, 70, localDate()), /Still processing/)
  assert.equal(app.activity.value.daily, false)
  assert.equal(await app.answer(question, 70, localDate()), true)
  assert.equal(app.activity.value.daily, true)
  const keys = submissions.map((call) => call.headers.get('Idempotency-Key'))
  assert.ok(keys[0])
  assert.equal(new Set(keys).size, 1)
  assert.equal(submissions[0].body, '{"answerId":70}')
  assert.ok(submissions[0].path.endsWith(`?localDate=${localDate()}`))
  assert.equal(
    calls.some((call) => call.path === '/api/wallet'),
    false,
  )
})
test('successful paid answer refetches and displays the authoritative wallet balance', async () => {
  const auth = useAuthStore(pinia)
  auth.setSession('test-token', 'test-user')
  app.profile.value = { ...profile }
  app.balance.value = 45
  calls.length = 0
  handler = (call) => {
    if (call.path === '/api/wallet') return json({ creditBalance: 35 })
    if (call.method === 'POST' && call.path.startsWith('/api/questions/8/answer'))
      return json({ message: 'Answer submitted successfully.' })
    return standard(call)
  }
  const paidQuestion = {
    ...question,
    questionId: 8,
    answers: [{ answerId: 80, answerText: 'Yes' }],
  }

  assert.equal(await app.answer(paidQuestion, 80, localDate(), true), true)

  const submissionIndex = calls.findIndex((call) => call.path.startsWith('/api/questions/8/answer'))
  const walletIndex = calls.findIndex((call) => call.path === '/api/wallet')
  assert.ok(submissionIndex >= 0 && walletIndex > submissionIndex)
  assert.equal(calls.filter((call) => call.path === '/api/wallet').length, 1)
  assert.equal(JSON.parse(calls[submissionIndex].body).spendSparks, true)
  assert.ok(calls[submissionIndex].headers.get('Idempotency-Key'))
  assert.equal(app.balance.value, 35)
  assert.equal(app.profile.value.creditBalance, 35)
})
test('failed paid answer does not refetch or locally deduct Sparks', async () => {
  app.balance.value = 45
  calls.length = 0
  handler = (call) =>
    call.method === 'POST' ? json({ message: 'Daily limit reached.' }, 400) : standard(call)
  const paidQuestion = {
    ...question,
    questionId: 8,
    answers: [{ answerId: 80, answerText: 'Yes' }],
  }

  await assert.rejects(app.answer(paidQuestion, 80, localDate(), true), /Daily limit reached/)

  assert.equal(
    calls.some((call) => call.path === '/api/wallet'),
    false,
  )
  assert.equal(app.balance.value, 45)
})
test('wallet refetch failure does not turn a saved paid answer into a failed submission', async () => {
  app.balance.value = 45
  calls.length = 0
  handler = (call) => {
    if (call.path === '/api/wallet') return json({}, 503)
    if (call.method === 'POST' && call.path.startsWith('/api/questions/8/answer'))
      return json({ message: 'Answer submitted successfully.' })
    return standard(call)
  }
  const paidQuestion = {
    ...question,
    questionId: 8,
    answers: [{ answerId: 80, answerText: 'Yes' }],
  }

  assert.equal(await app.answer(paidQuestion, 80, localDate(), true), true)

  assert.equal(app.balance.value, 45)
  assert.match(app.error.value, /answer was saved.*couldn’t refresh your Spark balance/)
})
test('server limit messages are shown without incrementing local activity', async () => {
  handler = () => json({ message: 'Daily limit reached.' }, 400)
  await assert.rejects(
    app.answer({ ...question, questionId: 8 }, 80, localDate()),
    /Daily limit reached/,
  )
  assert.equal(app.activity.value.count, 0)
})
test('wallet and profile mutations follow current endpoint contracts', async () => {
  const offer = { creditPackageId: 2, currencyCode: 'AUD', credits: 120, amount: 9.99 }
  handler = (call) => {
    if (call.path === '/api/wallet') return json({ creditBalance: 10 })
    if (call.path === '/api/currencies')
      return json([{ code: 'AUD', symbol: '$', name: 'Australian Dollar', decimalPlaces: 2 }])
    if (call.path === '/api/credit-packages?currencyCode=AUD') return json([offer])
    if (call.path === '/api/wallet/top-up') {
      assert.equal(call.body, '{"creditPackageId":2}')
      return json({ creditBalance: 130 })
    }
    if (call.path === '/api/UserProfile/me') {
      assert.equal(call.method, 'PUT')
      const input = JSON.parse(call.body)
      assert.equal(input.personalityTypeId, 8)
      return json({ ...profile, ...input, personalityTypeCode: 'ENFP' })
    }
    if (call.path === '/api/UserProfile/me/deactivate') {
      assert.equal(call.method, 'POST')
      return new Response(null, { status: 204 })
    }
    if (call.path === '/api/Auth/logout') return new Response(null, { status: 204 })
    if (call.path === '/api/wallet/payments?page=1')
      return json([
        {
          userPaymentId: 4,
          dateCreated: '2026-09-16T00:00:00Z',
          currencyCode: 'AUD',
          amount: 9.99,
          sparksPurchased: 120,
          status: 'Completed',
          paymentProvider: 'IntertwineDemo',
        },
      ])
    throw new Error(`Unexpected request ${call.path}`)
  }
  assert.equal(await app.loadWallet(), 'AUD')
  assert.equal(app.currencies.value[0].name, 'Australian Dollar')
  assert.equal(app.balance.value, 10)
  await app.topUp(offer)
  assert.equal(app.balance.value, 130)
  await app.saveProfile({
    firstName: 'Updated',
    lastName: 'Person',
    middleName: '',
    avatarName: 'updated',
    personalityTypeId: 8,
  })
  assert.equal(app.profile.value.firstName, 'Updated')
  assert.equal(app.profile.value.personalityTypeId, 8)
  assert.equal(app.profile.value.personalityTypeCode, 'ENFP')
  assert.equal((await app.loadPayments())[0].sparksPurchased, 120)
  await app.deactivateProfile()
  assert.equal(sessionStorage.getItem('intertwine.token'), null)
  assert.equal(app.signedIn.value, false)
})
test('personality types are loaded from the API rather than hardcoded', async () => {
  const auth = useAuthStore(pinia)
  auth.setSession('test-token', 'test-user')
  calls.length = 0
  handler = (call) => {
    assert.equal(call.path, '/api/personality-types')
    assert.equal(call.method, 'GET')
    return json([
      { personalityTypeId: 8, code: 'ENFP', name: null },
      { personalityTypeId: 1, code: 'INTJ', name: null },
    ])
  }

  const result = await app.loadPersonalityTypes(true)

  assert.deepEqual(
    result.map((item) => item.code),
    ['ENFP', 'INTJ'],
  )
  assert.deepEqual(app.personalityTypes.value, result)
})
test('profile preview navigation uses a dedicated protected route', () => {
  const routerSource = readFileSync(resolve('src/router/index.ts'), 'utf8')
  const editProfileSource = readFileSync(resolve('src/views/MyProfileView.vue'), 'utf8')

  assert.match(routerSource, /path:\s*['"]\/profile\/preview['"]/)
  assert.match(routerSource, /name:\s*['"]profile-preview['"]/)
  assert.match(routerSource, /requiresAuth:\s*true/)
  assert.match(editProfileSource, /to="\/profile\/preview"/)
  assert.match(editProfileSource, />Preview profile</)
})
test('public profile presentation derives initials and deterministic category accents', () => {
  assert.equal(avatarInitials('Lance'), 'L')
  assert.equal(avatarInitials('Merrick Lance'), 'ML')
  assert.equal(avatarInitials('  '), 'I')
  assert.equal(
    categoryAccent([{ categoryId: 1, categoryName: 'Relationships', color: '#E84393' }]),
    '#E84393',
  )
  assert.equal(
    categoryAccent([{ categoryId: 1, categoryName: 'Relationships', color: 'invalid' }]),
    '#3E5947',
  )
})
test('profile preview loads the public-safe contract with answers and category colors', async () => {
  const auth = useAuthStore(pinia)
  auth.setSession('test-token', 'test-user')
  calls.length = 0
  const preview = {
    userProfileId: 3,
    avatarName: 'Merrick Lance',
    personalityTypeCode: 'ENFP',
    answeredQuestions: [
      {
        questionId: 7,
        questionTitle: 'Connection',
        fullQuestion: 'What matters?',
        answerId: 70,
        answerText: 'Kindness',
        categories: [{ categoryId: 2, categoryName: 'Relationships', color: '#E84393' }],
      },
    ],
  }
  handler = (call) => {
    assert.equal(call.path, '/api/UserProfile/me/preview')
    return json(preview)
  }

  const result = await app.loadProfilePreview(true)

  assert.deepEqual(result, preview)
  assert.equal(app.publicProfile.value.personalityTypeCode, 'ENFP')
  assert.equal(app.publicProfile.value.answeredQuestions[0].answerText, 'Kindness')
  assert.equal(app.publicProfile.value.answeredQuestions[0].categories[0].color, '#E84393')
})
test('public profile component handles null personality and empty answers without private names', () => {
  const source = readFileSync(resolve('src/components/PublicProfileCard.vue'), 'utf8')

  assert.match(source, /profile\.avatarName/)
  assert.match(source, /profile\.personalityTypeCode/)
  assert.match(source, /profile\.answeredQuestions/)
  assert.match(source, /Personality type not shared/)
  assert.match(source, /No answered questions yet/)
  assert.match(source, /CategoryTags/)
  assert.match(source, /borderTopColor:\s*categoryAccent/)
  assert.doesNotMatch(source, /firstName|middleName|lastName|identityUserId|email/i)
})
test('Daily Question UI exposes discovery only after the Daily Question is answered', () => {
  const source = readFileSync(resolve('src/views/QuestionsView.vue'), 'utf8')

  assert.match(source, /activity\.daily\s*&&\s*daily\.dailyQuestionId/)
  assert.match(source, /See who answered like you/)
  assert.match(source, /DailyQuestionDiscovery/)
})
test('discovery API requests use the Daily Question id, local date, and pagination', async () => {
  calls.length = 0
  handler = (call) => {
    if (call.path === '/api/daily-questions/4/discovery?localDate=2026-09-24')
      return json({ dailyQuestionId: 4, answerPools: [], historicalAccess: [] })
    if (
      call.path ===
      '/api/daily-questions/4/discovery/users?answerId=10&localDate=2026-09-24&page=2&pageSize=20'
    )
      return json({ accessState: 'Included', page: 2, pageSize: 20, users: [] })
    throw new Error(`Unexpected request ${call.path}`)
  }

  assert.equal((await getDailyQuestionDiscovery(4, '2026-09-24')).dailyQuestionId, 4)
  assert.equal((await getDiscoveryUsers(4, 10, '2026-09-24', 2)).accessState, 'Included')
})
test('discovery presentation includes public users, access locks, and no fake price', () => {
  const source = readFileSync(resolve('src/components/DailyQuestionDiscovery.vue'), 'utf8')

  assert.match(source, /avatarInitials\(user\.avatarName\)/)
  assert.match(source, /user\.avatarName/)
  assert.match(source, /user\.personalityTypeCode/)
  assert.match(source, /No matching users yet/)
  assert.match(source, /Subscription required/)
  assert.match(source, /Sparks required/)
  assert.match(source, /LoadingState/)
  assert.doesNotMatch(source, /\d+\s*(Sparks|✨)/)
  assert.doesNotMatch(source, /firstName|lastName|email|identityUserId/i)
})
test('401 clears private state and prompts sign-in', async () => {
  handler = () => json({}, 401)
  let failure
  try {
    await request('/api/wallet')
  } catch (cause) {
    failure = cause
  }
  assert.ok(failure instanceof ApiError)
  assert.match(app.reportError(failure), /sign in again/)
  assert.equal(app.signedIn.value, false)
  assert.equal(app.profile.value, null)
  assert.equal(app.balance.value, null)
  assert.deepEqual(app.questions.value, [])
  assert.equal(app.authOpen.value, true)
})
test('HTTP client preserves non-authentication API errors', async () => {
  const auth = useAuthStore(pinia)
  auth.setSession('temporary-token', 'temporary-user')
  calls.length = 0
  handler = () => json({}, 403)
  await assert.rejects(request('/api/private'), (error) => {
    assert.ok(error instanceof ApiError)
    assert.equal(error.status, 403)
    return true
  })
  assert.equal(auth.signedIn, true)
  assert.equal(
    calls.some((call) => call.path === '/api/Auth/refresh'),
    false,
  )

  handler = () => json({ errors: { Email: ['Email is required.'] } }, 400)
  await assert.rejects(request('/api/validation'), /Email is required/)
  handler = () => json({}, 429)
  await assert.rejects(request('/api/limited'), /Too many attempts/)
  handler = () => {
    throw new Error('Connection unavailable')
  }
  await assert.rejects(request('/api/offline'), /couldn’t reach Intertwine/)
  handler = () => new Response('not JSON', { headers: { 'Content-Type': 'text/plain' } })
  await assert.rejects(request('/api/unexpected'), /unexpected response/)
  auth.clearSession()
})
test('successful request does not refresh', async () => {
  const auth = useAuthStore(pinia)
  auth.setSession('valid-token', 'test-user')
  calls.length = 0
  handler = (call) => {
    assert.equal(call.path, '/api/private')
    assert.equal(call.headers.get('Authorization'), 'Bearer valid-token')
    return json({ ok: true })
  }
  assert.deepEqual(await request('/api/private'), { ok: true })
  assert.equal(calls.length, 1)
  auth.clearSession()
})
test('one 401 refreshes through the cookie client and retries with the new access token', async () => {
  const auth = useAuthStore(pinia)
  auth.setSession('expired-token', 'test-user')
  calls.length = 0
  handler = (call) => {
    if (call.path === '/api/Auth/refresh') {
      assert.equal(call.method, 'POST')
      assert.equal(call.body, undefined)
      assert.equal(call.withCredentials, true)
      assert.equal(call.timeout, 15000)
      assert.equal(call.headers.get('Authorization'), undefined)
      return json({ succeeded: true, token: 'new-token', userId: 'new-user' })
    }
    assert.equal(call.path, '/api/private')
    return call.headers.get('Authorization') === 'Bearer expired-token'
      ? json({}, 401)
      : json({ ok: true })
  }
  assert.deepEqual(await request('/api/private'), { ok: true })
  assert.equal(calls.filter((call) => call.path === '/api/Auth/refresh').length, 1)
  assert.equal(calls.filter((call) => call.path === '/api/private').length, 2)
  assert.equal(calls.at(-1).headers.get('Authorization'), 'Bearer new-token')
  assert.equal(auth.accessToken, 'new-token')
  assert.equal(auth.userId, 'new-user')
  assert.equal(sessionStorage.getItem('intertwine.token'), 'new-token')
  auth.clearSession()
})
test('failed refresh clears the Pinia session and returns a session-ended error', async () => {
  const auth = useAuthStore(pinia)
  auth.setSession('expired-token', 'test-user')
  calls.length = 0
  handler = () => json({}, 401)
  await assert.rejects(request('/api/private'), (error) => {
    assert.ok(error instanceof ApiError)
    assert.equal(error.status, 401)
    assert.match(error.message, /session has ended/)
    return true
  })
  assert.equal(calls.filter((call) => call.path === '/api/Auth/refresh').length, 1)
  assert.equal(auth.signedIn, false)
  assert.equal(sessionStorage.getItem('intertwine.token'), null)
})
test('the refresh endpoint 401 never attempts another refresh', async () => {
  calls.length = 0
  handler = () => json({}, 401)
  await assert.rejects(refreshSession(), (error) => {
    assert.equal(error.response?.status, 401)
    return true
  })
  assert.equal(calls.filter((call) => call.path === '/api/Auth/refresh').length, 1)
})
test('a retried 401 stops after one refresh', async () => {
  const auth = useAuthStore(pinia)
  auth.setSession('expired-token', 'test-user')
  calls.length = 0
  handler = (call) =>
    call.path === '/api/Auth/refresh'
      ? json({ succeeded: true, token: 'new-token', userId: 'test-user' })
      : json({}, 401)
  await assert.rejects(request('/api/private'), (error) => {
    assert.ok(error instanceof ApiError)
    assert.equal(error.status, 401)
    return true
  })
  assert.equal(calls.filter((call) => call.path === '/api/Auth/refresh').length, 1)
  assert.equal(calls.filter((call) => call.path === '/api/private').length, 2)
  auth.clearSession()
})
test('concurrent 401 responses share one rotating refresh operation', async () => {
  const auth = useAuthStore(pinia)
  auth.setSession('expired-token', 'test-user')
  calls.length = 0
  let finishRefresh = () => {}
  const refreshGate = new Promise((resolve) => {
    finishRefresh = resolve
  })
  handler = (call) => {
    if (call.path === '/api/Auth/refresh')
      return refreshGate.then(() =>
        json({ succeeded: true, token: 'new-token', userId: 'test-user' }),
      )
    return call.headers.get('Authorization') === 'Bearer expired-token'
      ? json({}, 401)
      : json({ ok: true })
  }
  const pending = Array.from({ length: 5 }, (_, index) => request(`/api/private/${index}`))
  await new Promise((resolve) => setTimeout(resolve, 0))
  assert.equal(calls.filter((call) => call.path === '/api/Auth/refresh').length, 1)
  finishRefresh()
  assert.deepEqual(
    await Promise.all(pending),
    Array.from({ length: 5 }, () => ({ ok: true })),
  )
  assert.equal(calls.filter((call) => call.path === '/api/Auth/refresh').length, 1)
  assert.equal(calls.filter((call) => call.path.startsWith('/api/private/')).length, 10)
  auth.clearSession()
})
test('a late 401 from the old token retries without rotating again', async () => {
  const auth = useAuthStore(pinia)
  auth.setSession('expired-token', 'test-user')
  calls.length = 0
  let releaseOldResponse = () => {}
  const oldResponseGate = new Promise((resolve) => {
    releaseOldResponse = resolve
  })
  handler = (call) => {
    if (call.path === '/api/Auth/refresh')
      return json({ succeeded: true, token: 'new-token', userId: 'test-user' })
    if (call.headers.get('Authorization') === 'Bearer new-token') return json({ ok: true })
    return call.path === '/api/slow' ? oldResponseGate.then(() => json({}, 401)) : json({}, 401)
  }
  const slow = request('/api/slow')
  assert.deepEqual(await request('/api/fast'), { ok: true })
  releaseOldResponse()
  assert.deepEqual(await slow, { ok: true })
  assert.equal(calls.filter((call) => call.path === '/api/Auth/refresh').length, 1)
  auth.clearSession()
})
test('sign-out while refresh is pending does not restore the session', async () => {
  const auth = useAuthStore(pinia)
  auth.setSession('expired-token', 'test-user')
  calls.length = 0
  let finishRefresh = () => {}
  const refreshGate = new Promise((resolve) => {
    finishRefresh = resolve
  })
  handler = (call) =>
    call.path === '/api/Auth/refresh'
      ? refreshGate.then(() => json({ succeeded: true, token: 'new-token', userId: 'test-user' }))
      : json({}, 401)
  const pending = request('/api/private')
  await new Promise((resolve) => setTimeout(resolve, 0))
  assert.equal(calls.filter((call) => call.path === '/api/Auth/refresh').length, 1)
  auth.clearSession()
  finishRefresh()
  await assert.rejects(pending, { status: 401 })
  assert.equal(auth.signedIn, false)
  assert.equal(sessionStorage.getItem('intertwine.token'), null)
})
test('logout sends a credentialed empty POST before clearing the local session', async () => {
  const auth = useAuthStore(pinia)
  auth.setSession('current-token', 'test-user')
  calls.length = 0
  handler = (call) => {
    assert.equal(call.path, '/api/Auth/logout')
    assert.equal(call.method, 'POST')
    assert.equal(call.body, undefined)
    assert.equal(call.withCredentials, true)
    assert.equal(auth.accessToken, 'current-token')
    return new Response(null, { status: 204 })
  }

  await app.signOut()

  assert.equal(calls.length, 1)
  assert.equal(auth.signedIn, false)
  assert.equal(sessionStorage.getItem('intertwine.token'), null)
  assert.equal(sessionStorage.getItem('intertwine.user'), null)
})
test('failed backend logout still clears local authentication and reports the failure', async () => {
  const auth = useAuthStore(pinia)
  auth.setSession('current-token', 'test-user')
  calls.length = 0
  handler = () => json({ message: 'Database unavailable' }, 500)

  await assert.rejects(app.signOut())

  assert.equal(calls.filter((call) => call.path === '/api/Auth/logout').length, 1)
  assert.equal(
    calls.some((call) => call.path === '/api/Auth/refresh'),
    false,
  )
  assert.equal(auth.signedIn, false)
  assert.equal(sessionStorage.getItem('intertwine.token'), null)
})
test('logout waits for a pending refresh and then revokes its replacement cookie', async () => {
  const auth = useAuthStore(pinia)
  auth.setSession('expired-token', 'test-user')
  calls.length = 0
  let finishRefresh = () => {}
  const refreshGate = new Promise((resolve) => {
    finishRefresh = resolve
  })
  handler = (call) => {
    if (call.path === '/api/Auth/refresh')
      return refreshGate.then(() =>
        json({ succeeded: true, token: 'new-token', userId: 'test-user' }),
      )
    if (call.path === '/api/Auth/logout') {
      assert.equal(auth.accessToken, 'new-token')
      return new Response(null, { status: 204 })
    }
    return call.headers.get('Authorization') === 'Bearer new-token'
      ? json({ ok: true })
      : json({}, 401)
  }
  const pendingRequest = request('/api/private')
  await new Promise((resolve) => setTimeout(resolve, 0))
  assert.equal(calls.filter((call) => call.path === '/api/Auth/refresh').length, 1)

  const signOut = app.signOut()
  assert.equal(
    calls.some((call) => call.path === '/api/Auth/logout'),
    false,
  )
  finishRefresh()
  await Promise.allSettled([pendingRequest, signOut])

  assert.equal(calls.filter((call) => call.path === '/api/Auth/refresh').length, 1)
  assert.equal(calls.filter((call) => call.path === '/api/Auth/logout').length, 1)
  assert.equal(auth.signedIn, false)
})
test('failed pending refresh does not clear local state before logout is attempted', async () => {
  const auth = useAuthStore(pinia)
  auth.setSession('expired-token', 'test-user')
  calls.length = 0
  let finishRefresh = () => {}
  const refreshGate = new Promise((resolve) => {
    finishRefresh = resolve
  })
  handler = (call) => {
    if (call.path === '/api/Auth/refresh') return refreshGate.then(() => json({}, 401))
    if (call.path === '/api/Auth/logout') {
      assert.equal(auth.accessToken, 'expired-token')
      return new Response(null, { status: 204 })
    }
    return json({}, 401)
  }
  const pendingRequest = request('/api/private')
  await new Promise((resolve) => setTimeout(resolve, 0))

  const signOut = app.signOut()
  finishRefresh()
  await Promise.allSettled([pendingRequest, signOut])

  assert.equal(calls.filter((call) => call.path === '/api/Auth/logout').length, 1)
  assert.equal(auth.signedIn, false)
})
test('login and registration 401 responses do not refresh', async () => {
  const auth = useAuthStore(pinia)
  auth.setSession('old-token', 'test-user')
  calls.length = 0
  handler = () => json({}, 401)
  await assert.rejects(request('/api/Auth/login', { method: 'POST' }), { status: 401 })
  await assert.rejects(request('/api/Auth/register', { method: 'POST' }), { status: 401 })
  await assert.rejects(request('/api/Auth/logout', { method: 'POST' }), { status: 401 })
  assert.equal(
    calls.some((call) => call.path === '/api/Auth/refresh'),
    false,
  )
  auth.clearSession()
})
test('HTTP client forwards request cancellation signals', async () => {
  const controller = new AbortController()
  handler = (call) => {
    assert.equal(call.signal, controller.signal)
    return json({ ok: true })
  }
  assert.deepEqual(await request('/api/ping', { signal: controller.signal }), { ok: true })
})
test('signed-out refresh does not request account or sample data', async () => {
  handler = () => {
    throw new Error('Signed-out home must not contact account APIs')
  }
  await app.refresh()
  assert.equal(app.signedIn.value, false)
  assert.equal(app.daily.value, null)
  assert.deepEqual(app.questions.value, [])
})
test('paid request includes explicit consent in its body', () => {
  assert.deepEqual(JSON.parse(answerRequest(7, 70, '2026-09-15', 'paid-key', true).options.body), {
    answerId: 70,
    spendSparks: true,
  })
})
test('auth store restores a tab session and owns its persistence', () => {
  const priorToken = sessionStorage.getItem('intertwine.token')
  const priorUser = sessionStorage.getItem('intertwine.user')
  try {
    sessionStorage.setItem('intertwine.token', 'restored-token')
    sessionStorage.setItem('intertwine.user', 'restored-user')
    const restored = useAuthStore(createPinia())
    assert.equal(restored.accessToken, 'restored-token')
    assert.equal(restored.userId, 'restored-user')
    assert.equal(restored.signedIn, true)

    restored.setSession('replacement-token', 'replacement-user')
    assert.equal(sessionStorage.getItem('intertwine.token'), 'replacement-token')
    assert.equal(restored.userId, 'replacement-user')

    restored.clearSession()
    assert.equal(restored.signedIn, false)
    assert.equal(sessionStorage.getItem('intertwine.token'), null)
    assert.equal(sessionStorage.getItem('intertwine.user'), null)
  } finally {
    if (priorToken === null) sessionStorage.removeItem('intertwine.token')
    else sessionStorage.setItem('intertwine.token', priorToken)
    if (priorUser === null) sessionStorage.removeItem('intertwine.user')
    else sessionStorage.setItem('intertwine.user', priorUser)
  }
})
after(() => {
  assert.equal(dirname(resolve(runtime)), resolve(tmpdir()))
  assert.ok(basename(runtime).startsWith('intertwine-tests-'))
  rmSync(runtime, { recursive: true, force: true })
})
