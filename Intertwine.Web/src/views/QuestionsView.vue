<script setup lang="ts">
import { computed, onBeforeMount, onUnmounted, ref } from 'vue'
import { useIntertwine } from '../composables/useIntertwine'
import type { Question, QuestionSummary } from '../models/question'
import AppIcon from '../components/AppIcon.vue'
import AppModal from '../components/AppModal.vue'
import AnswerForm from '../components/AnswerForm.vue'
import CategoryTags from '../components/CategoryTags.vue'
import LoadingState from '../components/LoadingState.vue'
import DailyQuestionDiscovery from '../components/DailyQuestionDiscovery.vue'
const props = defineProps<{ library?: boolean }>()
const {
  daily,
  questions,
  loading,
  error,
  today,
  activity,
  canUseAccount,
  authOpen,
  displayName,
  refresh,
  detail,
  reportError,
} = useIntertwine()
const search = ref('')
const category = ref(0)
const selectedQuestion = ref<Question | null>(null)
const detailLoading = ref(false)
const detailError = ref('')
const openedId = ref<number | null>(null)
const openedDate = ref('')
const discoveryOpen = ref(false)
let detailVersion = 0
const pageRequests = new AbortController()
const categories = computed(() => [
  ...new Map(questions.value.flatMap((q) => q.categories).map((c) => [c.categoryId, c])).values(),
])
const filtered = computed(() =>
  questions.value.filter(
    (q) =>
      (!category.value || q.categories.some((c) => c.categoryId === category.value)) &&
      `${q.questionTitle} ${q.fullQuestion}`.toLowerCase().includes(search.value.toLowerCase()),
  ),
)
const featured = computed(() =>
  questions.value.filter((q) => q.questionId !== daily.value?.questionId).slice(0, 3),
)
const dateLabel = computed(() =>
  new Date(`${today.value}T12:00:00`).toLocaleDateString(undefined, {
    weekday: 'long',
    month: 'long',
    day: 'numeric',
  }),
)
async function openQuestion(question: QuestionSummary) {
  const version = ++detailVersion
  openedId.value = question.questionId
  openedDate.value = today.value
  selectedQuestion.value = null
  detailLoading.value = true
  detailError.value = ''
  try {
    const result = await detail(question.questionId)
    if (version === detailVersion) selectedQuestion.value = result
  } catch (cause) {
    if (version === detailVersion) detailError.value = reportError(cause)
  } finally {
    if (version === detailVersion) detailLoading.value = false
  }
}
function closeQuestion() {
  detailVersion++
  openedId.value = null
}

function clearFilters() {
  search.value = ''
  category.value = 0
}
onBeforeMount(() => void refresh(pageRequests.signal))
onUnmounted(() => pageRequests.abort())
</script>
<template>
  <header class="page-heading">
    <div>
      <p class="eyebrow">{{ library ? 'A little curiosity goes a long way' : dateLabel }}</p>
      <h1>{{ library ? 'So many ways to be you.' : `A little more you, ${displayName}.` }}</h1>
      <p class="muted">
        {{
          library
            ? 'Explore the questions that make us think, feel, and connect.'
            : 'Small questions. Honest answers.More meaningful connections.'
        }}
      </p>
    </div>
    <span class="heading-emblem">
      <AppIcon :name="library ? 'book' : 'sun'" :size="31" />
    </span>
  </header>

  <LoadingState v-if="loading" message="Finding a little inspiration…" panel />
  <template v-else-if="!library">
    <div class="daily-layout">
      <section class="daily-card">
        <div class="daily-card-heading">
          <span class="eyebrow">
            <AppIcon name="sun" :size="17" />
            THE DAILY QUESTION
          </span>
          <span class="pill">{{ activity.daily ? 'Answered today' : 'A moment for you' }}</span>
        </div>
        <template v-if="daily">
          <CategoryTags :categories="daily.categories" />
          <div class="question-intro">
            <h2>{{ daily.fullQuestion || daily.questionTitle }}</h2>
            <p class="muted">There’s no right answer. Just the one that feels like you.</p>
          </div>
          <AnswerForm :key="`${daily.questionId}-${today}`" :question="daily" :date="today" />
          <button
            v-if="activity.daily && daily.dailyQuestionId"
            class="button secondary discovery-action"
            @click="discoveryOpen = true"
          >
            See who answered like you
            <AppIcon name="user" :size="17" />
          </button>
        </template>
        <div v-else class="empty-state">
          <AppIcon name="sun" :size="38" />
          <h2>{{ error ? 'Unable to load the Daily Question' : 'No Daily Question' }}</h2>
          <p>
            {{
              error
                ? 'We couldn’t load today’s question. Use the retry button above to reconnect.'
                : 'There isn’t a Daily Question assigned for today yet.Explore the library in the meantime.'
            }}
          </p>
          <RouterLink to="/questions" class="button secondary">
            Explore questions
            <AppIcon name="arrow" :size="16" />
          </RouterLink>
        </div>
      </section>
      <aside class="daily-aside">
        <section class="connection-card">
          <span class="eyebrow">THE BEAUTY OF CONNECTION</span>
          <svg class="connection-art" viewBox="0 0 260 165" fill="none" aria-hidden="true">
            <path
              d="M30 138C75 90 65 27 124 36s79 79 20 87S41 70 100 51s138 40 132 93"
              stroke="#b5bf9e"
              stroke-width="24"
              stroke-linecap="round"
            />
            <path
              d="M37 116C94 161 175 132 185 75S124 19 108 76s29 72 72 63"
              stroke="#eedfc4"
              stroke-width="24"
              stroke-linecap="round"
            />
            <path
              d="M30 138c22-23 31-48 42-69"
              stroke="#b5bf9e"
              stroke-width="24"
              stroke-linecap="round"
            />
            <circle cx="227" cy="34" r="4" fill="#dccdac" />
            <path d="M28 46v12m-6-6h12" stroke="#b5bf9e" stroke-width="2" />
          </svg>
          <h2>
            It starts with
            <br />
            something small.
          </h2>
          <p>
            Every answer is a thread.
            <br />
            Together, they tell your story.
          </p>
        </section>
        <section class="rhythm-card">
          <div class="section-title">
            <AppIcon name="leaf" :size="18" />
            <h3>Your daily rhythm</h3>
          </div>
          <div class="rhythm-row">
            <span>Daily question</span>
            <span class="status-dot" :class="{ complete: activity.daily }">
              {{ activity.daily ? 'Done' : '1 moment' }}
            </span>
          </div>
          <div class="rhythm-row">
            <span>Explore & reflect</span>
            <strong>
              {{ activity.remaining }}
              <span class="muted">left</span>
            </strong>
          </div>
          <div class="progress-track">
            <span :style="{ width: `${Math.min(activity.count / 2, 1) * 100}%` }"></span>
          </div>
          <p class="small muted">Synced with your account for this local date.</p>
        </section>
      </aside>
    </div>
    <section class="explore-section">
      <div class="section-heading">
        <div>
          <p class="eyebrow">FOLLOW YOUR CURIOSITY</p>
          <h2>A little more to explore</h2>
        </div>
        <RouterLink to="/questions" class="text-link">
          All questions
          <AppIcon name="arrow" :size="16" />
        </RouterLink>
      </div>
      <div v-if="featured.length" class="question-grid">
        <button
          v-for="(question, index) in featured"
          :key="question.questionId"
          class="discovery-card"
          @click="openQuestion(question)"
        >
          <CategoryTags :categories="question.categories" />
          <span class="tile-icon" :class="`tone-${index % 3}`">
            <AppIcon :name="['sun', 'heart', 'spark'][index % 3]!" :size="22" />
          </span>
          <h3>{{ question.questionTitle }}</h3>
          <p>{{ question.fullQuestion }}</p>
          <span class="card-footer">
            {{
              activity.answers[question.questionId] !== undefined
                ? 'Revisit your answer'
                : 'Take a moment'
            }}
            <AppIcon name="arrow" :size="17" />
          </span>
        </button>
      </div>
      <div v-else class="soft-panel">
        <p>No Questions found</p>
        <button v-if="!canUseAccount" class="text-button" @click="authOpen = true">
          Sign in to explore →
        </button>
      </div>
    </section>
  </template>

  <section v-else class="library-section">
    <div class="library-toolbar">
      <label class="search-field">
        <AppIcon name="search" :size="18" />
        <input
          v-model="search"
          aria-label="Search questions"
          placeholder="Find a question that speaks to you…"
          type="search"
        />
      </label>
      <span class="small muted">
        {{ filtered.length }} {{ filtered.length === 1 ? 'question' : 'questions' }}
      </span>
    </div>
    <div class="category-filters" aria-label="Filter questions by category">
      <button
        class="filter-chip"
        :class="{ active: category === 0 }"
        :aria-pressed="category === 0"
        @click="category = 0"
      >
        All questions
      </button>
      <button
        v-for="item in categories"
        :key="item.categoryId"
        class="filter-chip"
        :class="{ active: category === item.categoryId }"
        :aria-pressed="category === item.categoryId"
        @click="category = item.categoryId"
      >
        {{ item.categoryName }}
      </button>
    </div>
    <p class="library-note">
      <AppIcon name="leaf" :size="16" />
      Two free answers or updates each day, plus the Daily Question. Extra library actions cost 10
      ✨ each.
    </p>
    <div v-if="filtered.length" class="question-grid library-grid">
      <button
        v-for="(question, index) in filtered"
        :key="question.questionId"
        class="discovery-card"
        @click="openQuestion(question)"
      >
        <CategoryTags :categories="question.categories" />
        <span class="tile-icon" :class="`tone-${index % 3}`">
          <AppIcon :name="['heart', 'sun', 'spark'][index % 3]!" :size="23" />
        </span>
        <h3>{{ question.questionTitle }}</h3>
        <p>{{ question.fullQuestion }}</p>
        <span class="card-footer">
          {{
            question.questionId === daily?.questionId
              ? 'Today’s daily question'
              : activity.answers[question.questionId] !== undefined
                ? 'Revisit your answer'
                : 'Explore question'
          }}
          <AppIcon name="arrow" :size="17" />
        </span>
      </button>
    </div>
    <div v-else class="empty-state panel">
      <AppIcon name="search" :size="32" />
      <h2>{{ questions.length ? 'No Questions match your filters' : 'No Questions found' }}</h2>
      <p v-if="questions.length">Try another search or category.</p>
      <button v-if="!canUseAccount" class="button primary" @click="authOpen = true">Sign in</button>
      <button v-else-if="search || category" class="button secondary" @click="clearFilters">
        Clear filters
      </button>
    </div>
  </section>
  <AppModal v-if="openedId !== null" title="A little moment of reflection" @close="closeQuestion">
    <LoadingState v-if="detailLoading" message="Loading your question…" />
    <div v-else-if="detailError">
      <p class="inline-message error" role="alert">{{ detailError }}</p>
      <button
        class="button secondary"
        @click="openQuestion({ questionId: openedId } as QuestionSummary)"
      >
        Try again
      </button>
    </div>
    <template v-else-if="selectedQuestion">
      <CategoryTags :categories="selectedQuestion.categories" />
      <h3 class="modal-question">
        {{ selectedQuestion.fullQuestion || selectedQuestion.questionTitle }}
      </h3>
      <AnswerForm :question="selectedQuestion" :date="openedDate" />
    </template>
  </AppModal>
  <AppModal
    v-if="discoveryOpen && daily?.dailyQuestionId"
    title="See who answered like you"
    @close="discoveryOpen = false"
  >
    <DailyQuestionDiscovery :daily-question-id="daily.dailyQuestionId" :local-date="today" />
  </AppModal>
</template>
