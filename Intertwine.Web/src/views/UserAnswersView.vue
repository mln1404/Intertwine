<script setup lang="ts">
import { onBeforeMount, onUnmounted } from 'vue'
import { useIntertwine } from '../composables/useIntertwine'
import AppIcon from '../components/AppIcon.vue'
import CategoryTags from '../components/CategoryTags.vue'
import LoadingState from '../components/LoadingState.vue'

const {
  currentAnswers,
  currentAnswersLoading,
  currentAnswersLoaded,
  currentAnswersError,
  canUseAccount,
  authOpen,
  loadCurrentAnswers,
} = useIntertwine()
const pageRequests = new AbortController()

onBeforeMount(() => void loadCurrentAnswers(pageRequests.signal))
onUnmounted(() => pageRequests.abort())
</script>

<template>
  <header class="page-heading">
    <div>
      <p class="eyebrow">THE THREADS YOU’VE SHARED</p>
      <h1>Your questions and answers.</h1>
      <p class="muted">A colorful record of what matters to you.</p>
    </div>
    <span class="heading-emblem">
      <AppIcon name="book" :size="28" />
    </span>
  </header>

  <div v-if="!canUseAccount" class="empty-state panel">
    <AppIcon name="book" :size="36" />
    <h2>Your answers live here.</h2>
    <p>Sign in to see the questions you have answered.</p>
    <button class="button primary" @click="authOpen = true">Sign in</button>
  </div>
  <LoadingState
    v-else-if="currentAnswersLoading || !currentAnswersLoaded"
    message="Loading your questions and answers…"
    panel
  />
  <div v-else-if="currentAnswersError" class="empty-state panel">
    <AppIcon name="book" :size="36" />
    <h2>We couldn’t load your answers.</h2>
    <p>{{ currentAnswersError }}</p>
    <button class="button secondary" @click="loadCurrentAnswers(undefined, true)">Try again</button>
  </div>
  <section v-else class="answered-section panel">
    <div class="section-heading">
      <h2>
        Your answers
        <span class="muted">({{ currentAnswers.length }})</span>
      </h2>
      <a class="button secondary" href="#questions">Explore questions</a>
    </div>
    <div v-if="!currentAnswers.length" class="empty-state">
      <AppIcon name="book" :size="36" />
      <h3>No answered questions yet</h3>
      <p>Your selected answers and their categories will appear here.</p>
      <a class="button primary" href="#questions">Answer a question</a>
    </div>
    <div v-else class="question-grid">
      <article v-for="answer in currentAnswers" :key="answer.questionId" class="answered-card">
        <CategoryTags :categories="answer.categories" />
        <h3>{{ answer.questionTitle }}</h3>
        <p>{{ answer.fullQuestion }}</p>
        <p class="saved-answer">
          <strong>Your answer:</strong>
          {{ answer.answerText }}
        </p>
      </article>
    </div>
  </section>
</template>
