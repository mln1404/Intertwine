<script setup lang="ts">
import { onBeforeMount, ref } from 'vue'
import { useRoute } from 'vue-router'
import { ApiError } from '../api/http'
import { getPublicUserProfile } from '../api/profilesApi'
import AppIcon from '../components/AppIcon.vue'
import LoadingState from '../components/LoadingState.vue'
import PublicProfileCard from '../components/PublicProfileCard.vue'
import type { PublicProfile } from '../models/question'

const route = useRoute()
const profile = ref<PublicProfile | null>(null)
const loading = ref(true)
const notFound = ref(false)
const error = ref('')

async function loadPublicProfile() {
  loading.value = true
  notFound.value = false
  error.value = ''
  profile.value = null

  const routeValue = Array.isArray(route.params.userProfileId)
    ? route.params.userProfileId[0]
    : route.params.userProfileId
  const userProfileId = Number(routeValue)

  if (!Number.isInteger(userProfileId) || userProfileId <= 0) {
    notFound.value = true
    loading.value = false
    return
  }

  try {
    profile.value = await getPublicUserProfile(userProfileId)
  } catch (cause) {
    if (cause instanceof ApiError && cause.status === 404) {
      notFound.value = true
    } else {
      error.value =
        cause instanceof Error ? cause.message : 'Something went wrong. Please try again.'
    }
  } finally {
    loading.value = false
  }
}

onBeforeMount(() => void loadPublicProfile())
</script>

<template>
  <LoadingState v-if="loading" message="Loading this Intertwine profile…" panel />
  <div v-else-if="notFound" class="empty-state panel">
    <AppIcon name="user" :size="36" />
    <h1>Profile not found</h1>
    <p>This profile does not exist or is no longer active.</p>
    <RouterLink class="button secondary" to="/today">Back to Today</RouterLink>
  </div>
  <div v-else-if="error" class="empty-state panel">
    <AppIcon name="user" :size="36" />
    <h1>We couldn’t load this profile.</h1>
    <p>{{ error }}</p>
    <button class="button secondary" @click="loadPublicProfile">Try again</button>
  </div>
  <PublicProfileCard v-else-if="profile" :profile="profile" />
</template>
