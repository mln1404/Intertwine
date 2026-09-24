<script setup lang="ts">
import { computed, onBeforeMount, ref } from 'vue'
import { getDailyQuestionDiscovery, getDiscoveryUsers } from '../api/questionsApi'
import type { DailyQuestionDiscovery, DiscoveryDay, DiscoveryUser } from '../models/question'
import { useIntertwine } from '../composables/useIntertwine'
import { avatarInitials } from '../utils/profilePresentation'
import AppIcon from './AppIcon.vue'
import LoadingState from './LoadingState.vue'

const props = defineProps<{ dailyQuestionId: number; localDate: string }>()
const { reportError } = useIntertwine()
const context = ref<DailyQuestionDiscovery | null>(null)
const users = ref<DiscoveryUser[]>([])
const loading = ref(false)
const usersLoading = ref(false)
const error = ref('')
const page = ref(1)
const hasMore = ref(false)
const activeDailyQuestionId = ref(props.dailyQuestionId)
const currentPool = computed(() =>
  context.value?.answerPools.find((pool) => pool.answerId === context.value?.currentUserAnswerId),
)

async function loadUsers(nextPage = 1) {
  if (!context.value || currentPool.value?.accessState !== 'Included') return
  usersLoading.value = true
  try {
    const result = await getDiscoveryUsers(
      context.value.dailyQuestionId,
      context.value.currentUserAnswerId,
      props.localDate,
      nextPage,
    )
    users.value = nextPage === 1 ? result.users : [...users.value, ...result.users]
    page.value = result.page
    hasMore.value = result.hasMore
  } catch (cause) {
    error.value = reportError(cause)
  } finally {
    usersLoading.value = false
  }
}

async function loadContext(dailyQuestionId = activeDailyQuestionId.value) {
  loading.value = true
  error.value = ''
  users.value = []
  hasMore.value = false
  try {
    const result = await getDailyQuestionDiscovery(dailyQuestionId, props.localDate)
    context.value = result
    activeDailyQuestionId.value = result.dailyQuestionId
    await loadUsers()
  } catch (cause) {
    context.value = null
    error.value = reportError(cause)
  } finally {
    loading.value = false
  }
}

function canOpen(day: DiscoveryDay) {
  return day.hasAnswered && day.accessState === 'Included'
}

function dayLabel(date: string) {
  const difference =
    (new Date(`${props.localDate}T12:00:00`).getTime() - new Date(`${date}T12:00:00`).getTime()) /
    86_400_000
  if (difference === 0) return 'Today'
  if (difference === 1) return 'Yesterday'
  return new Date(`${date}T12:00:00`).toLocaleDateString(undefined, {
    month: 'short',
    day: 'numeric',
  })
}

onBeforeMount(() => void loadContext())
</script>

<template>
  <LoadingState v-if="loading" message="Finding people who answered like you…" />
  <div v-else-if="error" class="empty-state">
    <AppIcon name="user" :size="32" />
    <h3>We couldn’t load discovery.</h3>
    <p>{{ error }}</p>
    <button class="button secondary" @click="loadContext()">Try again</button>
  </div>
  <div v-else-if="context" class="daily-discovery">
    <section class="discovery-history" aria-label="Daily Question discovery history">
      <button
        v-for="day in context.historicalAccess"
        :key="day.dailyQuestionId"
        class="discovery-day"
        :class="{
          active: day.dailyQuestionId === context.dailyQuestionId,
          locked: day.accessState !== 'Included',
        }"
        :disabled="!canOpen(day)"
        @click="loadContext(day.dailyQuestionId)"
      >
        <span>{{ dayLabel(day.date) }}</span>
        <small v-if="day.accessState === 'SubscriptionRequired'">Subscription required</small>
        <small v-else-if="day.accessState === 'Unavailable'">Unavailable</small>
        <small v-else-if="!day.hasAnswered">Not answered</small>
        <small v-else>Included</small>
      </button>
    </section>

    <section class="discovery-question">
      <p class="eyebrow">{{ dayLabel(context.date) }}’s Daily Question</p>
      <h3>{{ context.fullQuestion || context.questionTitle }}</h3>
      <p class="saved-answer">
        <strong>Your current answer:</strong>
        {{ context.currentUserAnswerText }}
      </p>
    </section>

    <section class="discovery-pools" aria-label="Answer groups">
      <div
        v-for="pool in context.answerPools"
        :key="pool.answerId"
        class="discovery-pool"
        :class="{ selected: pool.answerId === context.currentUserAnswerId }"
      >
        <span>{{ pool.answerText }}</span>
        <small v-if="pool.accessState === 'Included'">Included</small>
        <small v-else-if="pool.accessState === 'SparksRequired'">
          <AppIcon name="lock" :size="13" />
          Sparks required
        </small>
        <small v-else-if="pool.accessState === 'SubscriptionRequired'">
          <AppIcon name="lock" :size="13" />
          Subscription required
        </small>
        <small v-else>Unavailable</small>
      </div>
    </section>

    <section v-if="currentPool?.accessState === 'Included'" class="discovery-results">
      <h3>People who answered like you</h3>
      <LoadingState v-if="usersLoading && !users.length" message="Finding shared answers…" />
      <div v-else-if="!users.length" class="empty-state compact-empty-state">
        <AppIcon name="user" :size="30" />
        <h3>No matching users yet</h3>
        <p>You’re the first thread in this answer group.</p>
      </div>
      <div v-else class="discovery-users">
        <RouterLink
          v-for="user in users"
          :key="user.userProfileId"
          class="discovery-user"
          :to="{ name: 'user-profile', params: { userProfileId: user.userProfileId } }"
          :aria-label="`View ${user.avatarName || 'Intertwine member'}'s profile`"
        >
          <span class="discovery-avatar">{{ avatarInitials(user.avatarName) }}</span>
          <strong>{{ user.avatarName || 'Intertwine member' }}</strong>
          <small>{{ user.personalityTypeCode || 'Personality type not shared' }}</small>
        </RouterLink>
      </div>
      <button
        v-if="hasMore"
        class="button secondary discovery-more"
        :disabled="usersLoading"
        @click="loadUsers(page + 1)"
      >
        {{ usersLoading ? 'Loading…' : 'Load more' }}
      </button>
    </section>
    <div v-else class="empty-state compact-empty-state">
      <AppIcon name="lock" :size="30" />
      <h3>
        {{
          context.dateAccessState === 'SubscriptionRequired'
            ? 'Subscription required for this day'
            : 'Discovery is unavailable for this day'
        }}
      </h3>
    </div>
  </div>
</template>
