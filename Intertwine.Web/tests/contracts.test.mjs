import { test, after } from 'node:test'
import assert from 'node:assert/strict'
import { mkdtempSync, readFileSync, mkdirSync, writeFileSync, rmSync } from 'node:fs'
import { tmpdir } from 'node:os'
import { basename, dirname, join, resolve } from 'node:path'
import { pathToFileURL } from 'node:url'
import ts from 'typescript'

// Exercise the real TypeScript API and composable modules with isolated network
// responses. No database, real account, Redis, or payment provider is contacted.
const runtime = mkdtempSync(join(tmpdir(), 'intertwine-tests-'))
const files = [
  'models/question',
  'utils/answerRequest',
  'api/http',
  'api/questionsApi',
  'data/demo',
  'composables/useIntertwine',
]
const vueUrl = pathToFileURL(resolve('node_modules/vue/dist/vue.runtime.esm-bundler.js')).href
for (const file of files) {
  const source = readFileSync(resolve(`src/${file}.ts`), 'utf8').replaceAll(
    'import.meta.env',
    '({ VITE_DEMO_MODE: "false" })',
  )
  let code = ts.transpileModule(source, {
    compilerOptions: { target: ts.ScriptTarget.ES2022, module: ts.ModuleKind.ESNext },
  }).outputText
  code = code
    .replace(/from ['"](\.[^'"]+)['"]/g, "from '$1.mjs'")
    .replace(/from ['"]vue['"]/g, `from '${vueUrl}'`)
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
const originalFetch = globalThis.fetch
const calls = []
let handler
globalThis.fetch = async (path, options) => {
  const call = { path, ...options }
  calls.push(call)
  return handler(call)
}
const json = (data, status = 200) =>
  new Response(JSON.stringify(data), { status, headers: { 'Content-Type': 'application/json' } })
const { localDate, answerRequest } = await import(
  pathToFileURL(join(runtime, 'utils/answerRequest.mjs'))
)
const { useIntertwine } = await import(
  pathToFileURL(join(runtime, 'composables/useIntertwine.mjs'))
)
const { request, ApiError } = await import(pathToFileURL(join(runtime, 'api/http.mjs')))
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
  if (call.path.startsWith('/api/questions/daily')) return json(question)
  if (call.path === '/api/questions') return json([{ ...question, answers: undefined }])
  if (call.path === '/api/UserProfile/me') return json(profile)
  if (call.path === '/api/user-answers/me') return json([{ questionId: 8, answerId: 80 }])
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
test('registration provisions a profile; login restores profile, answers, and daily activity', async () => {
  handler = () => json({ succeeded: true, userId: 'test-user', token: null })
  assert.equal(await app.authenticate(credentials, true), false)
  assert.equal(sessionStorage.getItem('intertwine.token'), null)
  handler = standard
  assert.equal(await app.authenticate(credentials, false), true)
  assert.equal(app.profile.value.firstName, 'Test')
  assert.equal(app.profile.value.creditBalance, 45)
  assert.equal(app.balance.value, 45)
  assert.equal(app.questions.value[0].answers, undefined)
  assert.equal(app.activity.value.answers[8], 80)
  assert.equal(app.activity.value.daily, false)
  assert.equal(app.activity.value.count, 0)
  assert.equal(app.activity.value.remaining, 2)
  assert.equal(
    calls.find((call) => call.path === '/api/questions').headers.get('Authorization'),
    'Bearer test-token',
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
    if (call.path.startsWith('/api/daily-activity/me')) return json({ dailyQuestionCreateOrUpdateUsed: true, nonDailyQuestionsAnswered: 0, nonDailyQuestionsRemaining: 2 })
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
    if (call.path === '/api/credit-packages?currencyCode=AUD') return json([offer])
    if (call.path === '/api/wallet/top-up') {
      assert.equal(call.body, '{"creditPackageId":2}')
      return json({ creditBalance: 130 })
    }
    if (call.path === '/api/UserProfile/me') {
      assert.equal(call.method, 'PUT')
      return json({ ...profile, ...JSON.parse(call.body) })
    }
    throw new Error(`Unexpected request ${call.path}`)
  }
  await app.loadWallet('AUD')
  assert.equal(app.balance.value, 10)
  await app.topUp(offer)
  assert.equal(app.balance.value, 130)
  await app.saveProfile({
    firstName: 'Updated',
    lastName: 'Person',
    middleName: '',
    avatarName: 'updated',
  })
  assert.equal(app.profile.value.firstName, 'Updated')
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
test('demo enforces one daily answer and two new-or-changed library actions without API calls', async () => {
  handler = () => {
    throw new Error('Demo must not contact the API')
  }
  await app.startDemo()
  const daily = app.daily.value
  await app.answer(daily, daily.answers[0].answerId, localDate())
  await assert.rejects(
    app.answer(daily, daily.answers[1].answerId, localDate()),
    /already answered/,
  )
  const library = await app.detail(2)
  await app.answer(library, library.answers[0].answerId, localDate())
  await assert.rejects(
    app.answer(library, library.answers[0].answerId, localDate()),
    /already your current answer/,
  )
  await app.answer(library, library.answers[1].answerId, localDate())
  await assert.rejects(
    app.answer(library, library.answers[2].answerId, localDate()),
    /two question actions/,
  )
  assert.equal(app.activity.value.count, 2)
})
test('paid demo answers debit ten Sparks, preserve current answers and reject repeats or insufficient funds', async () => {
  const library = await app.detail(2)
  const before = app.balance.value
  await app.answer(library, library.answers[2].answerId, localDate(), true)
  assert.equal(app.balance.value, before - 10)
  assert.equal(app.profile.value.creditBalance, before - 10)
  assert.equal(app.activity.value.count, 3)
  const saved = app.currentAnswers.value.find(x => x.questionId === library.questionId)
  assert.equal(saved.answerText, library.answers[2].answerText)
  assert.deepEqual(saved.categories, library.categories)
  await assert.rejects(app.answer(library, library.answers[2].answerId, localDate(), true), /already your current answer/)
  assert.equal(app.balance.value, before - 10)
  app.balance.value = 9
  await assert.rejects(app.answer(library, library.answers[0].answerId, localDate(), true), /need 10 Sparks/)
  assert.equal(app.balance.value, 9)
  assert.equal(app.activity.value.count, 3)
})
test('simulated top-up updates balance and payment history', async () => {
  const offer = app.packages.value[0]
  await app.topUp(offer)
  const payments = await app.loadPayments()
  assert.equal(payments[0].sparksPurchased, offer.credits)
  assert.equal(payments[0].status, 'Completed')
  assert.equal(app.balance.value, 9 + offer.credits)
  assert.equal(app.profile.value.creditBalance, app.balance.value)
})
test('paid request includes explicit consent in its body', () => {
  assert.deepEqual(JSON.parse(answerRequest(7, 70, '2026-09-15', 'paid-key', true).options.body), {answerId: 70, spendSparks: true})
})
after(() => {
  globalThis.fetch = originalFetch
  assert.equal(dirname(resolve(runtime)), resolve(tmpdir()))
  assert.ok(basename(runtime).startsWith('intertwine-tests-'))
  rmSync(runtime, { recursive: true, force: true })
})
