<script setup lang="ts">
import { computed, onMounted, onUnmounted, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import AppIcon from './components/AppIcon.vue'
import { useIntertwine } from './composables/useIntertwine'
import { localDate } from './utils/answerRequest'
const route = useRoute()
const router = useRouter()
const {
  signedIn,
  profile,
  profileLoading,
  profileLoaded,
  error,
  loading,
  today,
  activity,
  balance,
  refresh,
  loadProfile,
  loadCurrentAnswers,
  signOut,
} = useIntertwine()
const navigation = [
  { id: 'today', path: '/today', label: 'For you', icon: 'sun' },
  { id: 'questions', path: '/questions', label: 'Explore questions', icon: 'grid' },
  { id: 'wallet', path: '/wallet', label: 'Get Sparky', icon: 'wallet' },
  { id: 'payments', path: '/payments', label: 'Payment history', icon: 'wallet' },
  { id: 'answers', path: '/answers', label: 'My answers', icon: 'book' },
  { id: 'profile', path: '/profile', label: 'My profile', icon: 'user' },
]
const activeNavigationId = computed(() =>
  typeof route.meta.navigation === 'string' ? route.meta.navigation : route.name,
)
const currentNavigation = computed(() =>
  navigation.find((item) => item.id === activeNavigationId.value),
)
const initials = computed(() =>
  profile.value
    ? `${profile.value.firstName.slice(0, 1)}${profile.value.lastName.slice(0, 1)}` || 'I'
    : 'I',
)
watch(
  [() => route.fullPath, signedIn],
  () => {
    error.value = ''
    document.title = signedIn.value
      ? `${currentNavigation.value?.label ?? 'Intertwine'} · Intertwine`
      : 'Welcome · Intertwine'
    window.scrollTo(0, 0)
  },
  { immediate: true },
)
watch(signedIn, (authenticated) => {
  if (!authenticated && route.meta.requiresAuth) {
    void router.replace({ name: 'login', query: { redirect: route.fullPath } })
  }
})
function checkDate() {
  if (today.value !== localDate()) {
    activity.value = { daily: false, count: 0, remaining: 2, answers: {} }
    today.value = localDate()
    if (route.name === 'today' || route.name === 'questions') void refresh()
  }
}
function retry() {
  if (route.name === 'today' || route.name === 'questions') void refresh()
  else if (route.name === 'answers') void loadCurrentAnswers(undefined, true)
  else void loadProfile(true)
}
async function logout() {
  try {
    await signOut()
  } catch (cause) {
    console.error('Server-side logout failed; the local session was cleared.', cause)
  } finally {
    await router.replace({ name: 'login' })
  }
}
let timer: ReturnType<typeof setInterval>
onMounted(() => {
  window.addEventListener('focus', checkDate)
  timer = setInterval(checkDate, 30000)
  if (signedIn.value) void loadProfile()
})
onUnmounted(() => {
  window.removeEventListener('focus', checkDate)
  clearInterval(timer)
})
</script>
<template>
  <RouterView v-if="!route.meta.requiresAuth" />
  <template v-else>
    <a href="#main-content" class="skip-link" @click.prevent="($refs.main as HTMLElement)?.focus()">
      Skip to content
    </a>
    <div class="app-shell">
      <aside class="sidebar">
        <RouterLink class="brand" to="/today" aria-label="Intertwine home">
          <svg width="36" height="36" viewBox="0 0 36 36" fill="none" aria-hidden="true">
            <rect
              x="5"
              y="7"
              width="16"
              height="23"
              rx="8"
              transform="rotate(-26 5 7)"
              stroke="currentColor"
              stroke-width="2.5"
            />
            <rect
              x="17"
              y="3"
              width="16"
              height="23"
              rx="8"
              transform="rotate(26 17 3)"
              stroke="currentColor"
              stroke-width="2.5"
            />
          </svg>
          <span>
            intertwine
            <span class="brand-dot">.</span>
          </span>
        </RouterLink>
        <p class="sidebar-caption">A LITTLE CLOSER, EVERY DAY</p>
        <nav aria-label="Main navigation">
          <RouterLink
            v-for="item in navigation"
            :key="item.id"
            :to="item.path"
            :class="{ active: activeNavigationId === item.id }"
            :aria-current="activeNavigationId === item.id ? 'page' : undefined"
          >
            <AppIcon :name="item.icon" :size="20" />
            <span>{{ item.label }}</span>
            <span v-if="item.id === 'today'" class="nav-dot"></span>
          </RouterLink>
        </nav>
        <div class="sidebar-bottom">
          <div class="sidebar-note">
            <AppIcon name="heart" :size="23" />
            <p>
              Good connections
              <br />
              start with being you.
            </p>
            <span>Take your time. You belong here.</span>
          </div>
          <RouterLink class="sidebar-profile" to="/profile">
            <span v-if="profileLoading || !profileLoaded" class="spinner compact-spinner"></span>
            <span v-else class="avatar">{{ initials }}</span>
            <span v-if="profileLoading || !profileLoaded">Loading profile…</span>
            <span v-else>
              <strong>{{ profile?.firstName || 'Your profile' }} {{ profile?.lastName }}</strong>
              <small>Your personal space</small>
            </span>
            <AppIcon name="arrow" :size="16" />
          </RouterLink>
        </div>
      </aside>
      <div class="main-shell">
        <header class="topbar">
          <span class="breadcrumb">
            Your space
            <span>/</span>
            <strong>{{ currentNavigation?.label }}</strong>
          </span>
          <div class="topbar-actions">
            <span v-if="profileLoading || !profileLoaded" class="spinner compact-spinner"></span>
            <RouterLink
              v-else
              class="spark-balance"
              to="/payments"
              aria-label="Spark balance and payment history"
            >
              {{ (balance ?? 0).toLocaleString() }} ✨
            </RouterLink>
            <span class="mode-indicator">
              <span></span>
              Connected
            </span>
            <button class="icon-button" aria-label="Sign out" :disabled="loading" @click="logout">
              <AppIcon name="logout" :size="19" />
            </button>
            <span v-if="profileLoading || !profileLoaded" class="spinner compact-spinner"></span>
            <RouterLink v-else to="/profile" class="avatar small-avatar" aria-label="Your profile">
              {{ initials }}
            </RouterLink>
          </div>
        </header>
        <main id="main-content" ref="main" tabindex="-1">
          <div v-if="error" class="inline-message error global-error" role="alert">
            <span>{{ error }}</span>
            <button class="button secondary" :disabled="loading" @click="retry">
              {{ loading ? 'Trying…' : 'Try again' }}
            </button>
          </div>
          <RouterView v-slot="{ Component }">
            <component :is="Component" :key="route.name ?? route.path" />
          </RouterView>
          <footer class="page-footer">
            <span>Thoughtfully connected. Unmistakably you.</span>
            <span>
              Made for meaningful moments
              <AppIcon name="spark" :size="13" />
            </span>
          </footer>
        </main>
      </div>
    </div>
  </template>
</template>
