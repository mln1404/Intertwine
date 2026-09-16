<script setup lang="ts">
import { computed, reactive, ref, watch } from 'vue'
import { useIntertwine } from '../composables/useIntertwine'
import AppIcon from '../components/AppIcon.vue'
import AppModal from '../components/AppModal.vue'
import CategoryTags from '../components/CategoryTags.vue'
const {
  profile,
  canUseAccount,
  authOpen,
  saveProfile,
  deleteProfile,
  reportError,
  currentAnswers,
  balance,
} = useIntertwine()
const form = reactive({ firstName: '', middleName: '', lastName: '', avatarName: '' })
const busy = ref(false)
const error = ref('')
const message = ref('')
const deleting = ref(false)
const confirmation = ref('')
watch(
  profile,
  (value) =>
    Object.assign(form, {
      firstName: value?.firstName || '',
      middleName: value?.middleName || '',
      lastName: value?.lastName || '',
      avatarName: value?.avatarName || '',
    }),
  { immediate: true },
)
const dirty = computed(() =>
  Object.entries(form).some(([key, value]) => value !== profile.value?.[key as keyof typeof form]),
)

function beginDelete() {
  deleting.value = true
  confirmation.value = ''
}

async function save() {
  busy.value = true
  error.value = ''
  message.value = ''
  try {
    await saveProfile({ ...form })
    message.value = 'Your profile has been updated.'
  } catch (cause) {
    error.value = reportError(cause)
  } finally {
    busy.value = false
  }
}
async function remove() {
  if (confirmation.value !== 'DELETE') return
  busy.value = true
  error.value = ''
  try {
    await deleteProfile()
    deleting.value = false
    message.value = 'Profile deleted.'
  } catch (cause) {
    error.value = reportError(cause)
    deleting.value = false
  } finally {
    busy.value = false
  }
}
</script>
<template>
  <header class="page-heading">
    <div>
      <p class="eyebrow">THE PERSON BEHIND THE ANSWERS</p>
      <h1>A space to be yourself.</h1>
      <p class="muted">Make your profile feel a little more like you.</p>
    </div>
    <span class="heading-emblem">
      <AppIcon name="user" :size="28" />
    </span>
  </header>
  <div v-if="!canUseAccount" class="empty-state panel">
    <AppIcon name="user" :size="36" />
    <h2>Let’s get to know you.</h2>
    <p>Sign in to view and update your profile.</p>
    <button class="button primary" @click="authOpen = true">Sign in</button>
  </div>
  <div v-else-if="!profile" class="empty-state panel">
    <AppIcon name="user" :size="36" />
    <h2>Your profile isn’t available yet.</h2>
    <p>
      We couldn’t find profile information for this account. Sign out and contact the Intertwine
      team if the problem continues.
    </p>
  </div>
  <div v-else class="profile-layout">
    <section class="panel profile-form">
      <div class="profile-summary">
        <span class="avatar large">
          {{ form.firstName.slice(0, 1) }}{{ form.lastName.slice(0, 1) }}
        </span>
        <div>
          <h2>{{ profile.firstName }} {{ profile.lastName }}</h2>
          <p class="muted">
            {{ profile.avatarName ? `@${profile.avatarName}` : 'Your Intertwine profile' }}
          </p>
        </div>
      </div>
      <form class="stack-form" @submit.prevent="save">
        <div class="form-row">
          <label>
            First name
            <input
              v-model.trim="form.firstName"
              autocomplete="given-name"
              required
              maxlength="100"
            />
          </label>
          <label>
            Last name
            <input
              v-model.trim="form.lastName"
              autocomplete="family-name"
              required
              maxlength="100"
            />
          </label>
        </div>
        <label>
          Middle name
          <span class="muted">(optional)</span>
          <input v-model.trim="form.middleName" autocomplete="additional-name" maxlength="100" />
        </label>
        <label>
          Avatar name
          <input v-model.trim="form.avatarName" autocomplete="nickname" required maxlength="100" />
        </label>
        <p class="small muted">The name associated with your Intertwine avatar.</p>
        <p v-if="error" class="inline-message error" role="alert">{{ error }}</p>
        <p v-if="message" class="inline-message success" role="status">{{ message }}</p>
        <div class="form-actions">
          <button class="button primary" :disabled="busy || !dirty">
            {{ busy ? 'Saving…' : 'Save changes' }}
            <AppIcon name="check" :size="17" />
          </button>
        </div>
      </form>
    </section>
    <aside class="profile-note">
      <span class="tile-icon tone-0">
        <AppIcon name="leaf" :size="25" />
      </span>
      <h2>Unmistakably you.</h2>
      <p>Big dreams, little quirks, and everything in between. There’s room for all of it here.</p>
      <div class="note-divider"></div>
      <p class="small">
        Your answers can evolve with you. Two library answers or updates are free each day. Each
        extra action costs 10 Sparks ✨.
      </p>
    </aside>
    <section class="answered-section panel">
      <div class="section-heading">
        <h2>
          Your answers
          <span class="muted">({{ currentAnswers.length }})</span>
        </h2>
        <a class="spark-balance" href="#payments">
          Spark balance · {{ (balance ?? 0).toLocaleString() }} ✨
        </a>
      </div>
      <p v-if="!currentAnswers.length" class="muted">
        User has no Answered Questions yet.
        <a href="#questions">Explore questions →</a>
      </p>
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
    <section class="danger-zone">
      <div>
        <h3>Delete profile</h3>
        <p class="small muted">
          Remove your Intertwine profile. This does not delete your sign-in account.
        </p>
      </div>
      <button class="button danger" :disabled="busy" @click="beginDelete">Delete profile</button>
    </section>
  </div>
  <AppModal v-if="deleting" title="Delete your profile?" :busy="busy" @close="deleting = false">
    <p class="muted">
      This removes your profile. You may lose access to questions and your wallet until a profile is
      restored. Your sign-in account remains.
    </p>
    <form class="stack-form" @submit.prevent="remove">
      <label>
        Type DELETE to confirm
        <input v-model="confirmation" autocomplete="off" required pattern="DELETE" />
      </label>
      <button class="button danger full" :disabled="confirmation !== 'DELETE' || busy">
        {{ busy ? 'Deleting…' : 'Delete my profile' }}
      </button>
    </form>
  </AppModal>
</template>
