<script setup lang="ts">
import { computed, onMounted, onUnmounted, ref } from 'vue'
import AppIcon from './components/AppIcon.vue'
import AuthDialog from './components/AuthDialog.vue'
import QuestionsView from './views/QuestionsView.vue'
import WalletView from './views/WalletView.vue'
import ProfileView from './views/ProfileView.vue'
import PaymentsView from './views/PaymentsView.vue'
import { useIntertwine } from './composables/useIntertwine'
import { localDate } from './utils/answerRequest'
const {
  demo,
  signedIn,
  profile,
  error,
  loading,
  authOpen,
  today,
  activity,
  balance,
  refresh,
  signOut,
  startDemo,
} = useIntertwine()
const navigation = [
  { id: 'today', label: 'For you', icon: 'sun' },
  { id: 'questions', label: 'Explore questions', icon: 'grid' },
  { id: 'wallet', label: 'Get Sparky', icon: 'wallet' },
  { id: 'payments', label: 'Payment history', icon: 'wallet' },
  { id: 'profile', label: 'My profile', icon: 'user' },
]
const route = ref('today')
const initials = computed(() =>
  profile.value
    ? `${profile.value.firstName.slice(0, 1)}${profile.value.lastName.slice(0, 1)}` || 'I'
    : 'I',
)
function syncRoute() {
  const hash = location.hash.slice(1)
  route.value = navigation.some((item) => item.id === hash) ? hash : 'today'
  document.title = `${navigation.find((item) => item.id === route.value)?.label} · Intertwine`
  window.scrollTo(0, 0)
}
function checkDate() {
  if (today.value !== localDate()) {
    activity.value = { daily: false, count: 0, remaining: 2, answers: {} }
    void refresh()
  }
}
let timer: ReturnType<typeof setInterval>
onMounted(() => {
  syncRoute()
  window.addEventListener('hashchange', syncRoute)
  window.addEventListener('focus', checkDate)
  timer = setInterval(checkDate, 30000)
  void refresh()
})
onUnmounted(() => {
  window.removeEventListener('hashchange', syncRoute)
  window.removeEventListener('focus', checkDate)
  clearInterval(timer)
})
</script>
<template>
  <a href="#main-content" class="skip-link" @click.prevent="($refs.main as HTMLElement)?.focus()">
    Skip to content
  </a>
  <div class="app-shell">
    <aside class="sidebar">
      <a class="brand" href="#today" aria-label="Intertwine home">
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
      </a>
      <p class="sidebar-caption">A LITTLE CLOSER, EVERY DAY</p>
      <nav aria-label="Main navigation">
        <a
          v-for="item in navigation"
          :key="item.id"
          :href="`#${item.id}`"
          :class="{ active: route === item.id }"
          :aria-current="route === item.id ? 'page' : undefined"
        >
          <AppIcon :name="item.icon" :size="20" />
          <span>{{ item.label }}</span>
          <span v-if="item.id === 'today'" class="nav-dot"></span>
        </a>
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
        <a v-if="demo || signedIn" class="sidebar-profile" href="#profile">
          <span class="avatar">{{ initials }}</span>
          <span>
            <strong>{{ profile?.firstName || 'Your profile' }} {{ profile?.lastName }}</strong>
            <small>{{ demo ? 'Exploring in demo mode' : 'Your personal space' }}</small>
          </span>
          <AppIcon name="arrow" :size="16" />
        </a>
        <button v-else class="button primary full" @click="authOpen = true">Join Intertwine</button>
      </div>
    </aside>
    <div class="main-shell">
      <header class="topbar">
        <span class="breadcrumb">
          Your space
          <span>/</span>
          <strong>{{ navigation.find((item) => item.id === route)?.label }}</strong>
        </span>
        <div class="topbar-actions">
          <a
            v-if="demo || signedIn"
            class="spark-balance"
            href="#payments"
            aria-label="Spark balance and payment history"
          >
            {{ (balance ?? 0).toLocaleString() }} ✨
          </a>
          <span class="mode-indicator">
            <span></span>
            {{ demo ? 'Demo preview' : signedIn ? 'Connected' : 'Welcome' }}
          </span>
          <button v-if="demo || !signedIn" class="text-button" @click="authOpen = true">
            Sign in
            <AppIcon name="arrow" :size="15" />
          </button>
          <button
            v-else
            class="icon-button"
            aria-label="Sign out"
            :disabled="loading"
            @click="signOut"
          >
            <AppIcon name="logout" :size="19" />
          </button>
          <a href="#profile" class="avatar small-avatar" aria-label="Your profile">
            {{ initials }}
          </a>
        </div>
      </header>
      <main id="main-content" ref="main" tabindex="-1">
        <div v-if="demo" class="demo-banner">
          <span>
            <strong>A little look around.</strong>
            You’re exploring sample data. Changes stay in this preview.
          </span>
          <button class="text-button" @click="authOpen = true">
            Use my account
            <AppIcon name="arrow" :size="15" />
          </button>
        </div>
        <div v-if="error" class="inline-message error global-error" role="alert">
          <span>{{ error }}</span>
          <button class="button secondary" :disabled="loading" @click="refresh">
            {{ loading ? 'Trying…' : 'Try again' }}
          </button>
          <button v-if="!signedIn && !demo" class="text-button" @click="startDemo">
            Explore demo
          </button>
        </div>
        <QuestionsView
          v-if="route === 'today' || route === 'questions'"
          :key="`${route}-${demo}-${signedIn}`"
          :library="route === 'questions'"
        />
        <WalletView v-else-if="route === 'wallet'" :key="`wallet-${demo}-${signedIn}`" />
        <PaymentsView v-else-if="route === 'payments'" :key="`payments-${demo}-${signedIn}`" />
        <ProfileView v-else :key="`profile-${demo}-${signedIn}`" />
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
  <AuthDialog v-if="authOpen" />
</template>
