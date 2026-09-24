<script setup lang="ts">
import type { PublicProfile } from '../models/question'
import { avatarInitials, categoryAccent } from '../utils/profilePresentation'
import AppIcon from './AppIcon.vue'
import CategoryTags from './CategoryTags.vue'

defineProps<{ profile: PublicProfile }>()
</script>

<template>
  <article class="public-profile panel">
    <header class="public-profile-hero">
      <span class="public-profile-avatar" aria-hidden="true">
        {{ avatarInitials(profile.avatarName) }}
      </span>
      <div class="public-profile-identity">
        <p class="eyebrow">INTERTWINE PROFILE</p>
        <h1>{{ profile.avatarName || 'Intertwine member' }}</h1>
        <span v-if="profile.personalityTypeCode" class="pill personality-pill">
          MBTI · {{ profile.personalityTypeCode }}
        </span>
        <p v-else class="small muted">Personality type not shared</p>
      </div>
    </header>

    <section class="public-profile-answers" aria-labelledby="profile-answers-title">
      <div class="section-heading">
        <div>
          <p class="eyebrow">THE THREADS THEY’VE SHARED</p>
          <h2 id="profile-answers-title">
            Questions and answers
            <span class="muted">({{ profile.answeredQuestions.length }})</span>
          </h2>
        </div>
      </div>

      <div v-if="!profile.answeredQuestions.length" class="empty-state">
        <AppIcon name="book" :size="36" />
        <h3>No answered questions yet</h3>
        <p>Their selected answers will appear here.</p>
      </div>
      <div v-else class="public-answer-grid">
        <article
          v-for="answer in profile.answeredQuestions"
          :key="answer.questionId"
          class="public-answer-card"
          :style="{ borderTopColor: categoryAccent(answer.categories) }"
        >
          <CategoryTags :categories="answer.categories" />
          <h3>{{ answer.questionTitle }}</h3>
          <p>{{ answer.fullQuestion }}</p>
          <p class="saved-answer">
            <strong>Their answer:</strong>
            {{ answer.answerText }}
          </p>
        </article>
      </div>
    </section>
  </article>
</template>
