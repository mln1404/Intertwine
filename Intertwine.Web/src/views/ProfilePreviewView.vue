<script setup lang="ts">
import { onBeforeMount } from 'vue'
import { useIntertwine } from '../composables/useIntertwine'
import AppIcon from '../components/AppIcon.vue'
import LoadingState from '../components/LoadingState.vue'
import PublicProfileCard from '../components/PublicProfileCard.vue'

const {
  publicProfile,
  publicProfileLoading,
  publicProfileLoaded,
  publicProfileError,
  loadProfilePreview,
} = useIntertwine()

// Always refetch on entry so recent profile edits or answers are reflected in the preview.
onBeforeMount(() => void loadProfilePreview(true))
</script>

<template>
  <header class="page-heading">
    <div>
      <p class="eyebrow">PROFILE PREVIEW</p>
      <h1>See what others will see.</h1>
      <p class="muted">This preview contains only information intended for your public profile.</p>
    </div>
    <RouterLink class="button secondary" to="/profile">
      <AppIcon name="arrow" :size="17" />
      Back to editing
    </RouterLink>
  </header>

  <LoadingState
    v-if="publicProfileLoading || !publicProfileLoaded"
    message="Preparing your profile preview…"
    panel
  />
  <div v-else-if="publicProfileError" class="empty-state panel">
    <AppIcon name="user" :size="36" />
    <h2>We couldn’t load your profile preview.</h2>
    <p>{{ publicProfileError }}</p>
    <button class="button secondary" @click="loadProfilePreview(true)">Try again</button>
  </div>
  <div v-else-if="!publicProfile" class="empty-state panel">
    <AppIcon name="user" :size="36" />
    <h2>No profile to preview</h2>
    <p>Create your profile before opening its public preview.</p>
    <RouterLink class="button primary" to="/profile">Go to My Profile</RouterLink>
  </div>
  <PublicProfileCard v-else :profile="publicProfile" />
</template>
