<script setup lang="ts">
import { computed, onBeforeMount, reactive, ref, watch } from 'vue'
import { useIntertwine } from '../composables/useIntertwine'
import AppIcon from '../components/AppIcon.vue'
import AppModal from '../components/AppModal.vue'
import LoadingState from '../components/LoadingState.vue'
import type { ProfileInput } from '../models/question'
const {
  profile,
  profileLoading,
  profileLoaded,
  profileError,
  personalityTypes,
  personalityTypesLoading,
  personalityTypesLoaded,
  personalityTypesError,
  canUseAccount,
  authOpen,
  createProfile,
  saveProfile,
  deactivateProfile,
  reportError,
  loadProfile,
  loadPersonalityTypes,
} = useIntertwine()
const form = reactive<ProfileInput>({
  firstName: '',
  middleName: '',
  lastName: '',
  avatarName: '',
  personalityTypeId: null,
})
const busy = ref(false)
const error = ref('')
const message = ref('')
const deactivating = ref(false)
watch(
  profile,
  (value) =>
    Object.assign(form, {
      firstName: value?.firstName || '',
      middleName: value?.middleName || '',
      lastName: value?.lastName || '',
      avatarName: value?.avatarName || '',
      personalityTypeId: value?.personalityTypeId ?? null,
    }),
  { immediate: true },
)
const dirty = computed(() =>
  profile.value
    ? Object.entries(form).some(
        ([key, value]) => value !== profile.value?.[key as keyof typeof form],
      )
    : Boolean(form.firstName && form.lastName && form.avatarName),
)

function beginDeactivate() {
  deactivating.value = true
}

async function loadPage(force = false) {
  await Promise.all([loadProfile(force), loadPersonalityTypes(force)])
}

async function save() {
  busy.value = true
  error.value = ''
  message.value = ''
  try {
    const creating = !profile.value
    if (creating) await createProfile({ ...form })
    else await saveProfile({ ...form })
    message.value = creating ? 'Your profile is ready.' : 'Your profile has been updated.'
  } catch (cause) {
    error.value = reportError(cause)
  } finally {
    busy.value = false
  }
}
async function deactivate() {
  busy.value = true
  error.value = ''
  try {
    await deactivateProfile()
    deactivating.value = false
  } catch (cause) {
    error.value = reportError(cause)
    deactivating.value = false
  } finally {
    busy.value = false
  }
}
onBeforeMount(() => void loadPage())
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
  <LoadingState
    v-else-if="
      profileLoading || !profileLoaded || personalityTypesLoading || !personalityTypesLoaded
    "
    message="Loading your profile…"
    panel
  />
  <div v-else-if="profileError || personalityTypesError" class="empty-state panel">
    <AppIcon name="user" :size="36" />
    <h2>We couldn’t load your profile.</h2>
    <p>{{ profileError || personalityTypesError }}</p>
    <button class="button secondary" @click="loadPage(true)">Try again</button>
  </div>
  <div v-else class="profile-layout">
    <section class="panel profile-form">
      <div v-if="profile" class="profile-summary">
        <span class="avatar large">
          {{ form.firstName.slice(0, 1) }}{{ form.lastName.slice(0, 1) }}
        </span>
        <div>
          <h2>{{ profile.firstName }} {{ profile.lastName }}</h2>
          <p class="muted">
            {{ profile.avatarName ? `@${profile.avatarName}` : 'Your Intertwine profile' }}
          </p>
        </div>
        <span class="pill">MBTI · {{ profile.personalityTypeCode || 'Not selected' }}</span>
      </div>
      <div v-else class="profile-introduction">
        <p class="eyebrow">ONE LAST STEP</p>
        <h2>Create your profile</h2>
        <p class="muted">
          This account was created before profiles were added. Tell us what you would like people to
          call you.
        </p>
      </div>
      <h2 v-if="profile" class="form-title">Edit profile</h2>
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
        <label>
          MBTI personality type
          <span class="muted">(optional)</span>
          <select v-model="form.personalityTypeId" :disabled="busy">
            <option :value="null">Not selected</option>
            <option
              v-for="personalityType in personalityTypes"
              :key="personalityType.personalityTypeId"
              :value="personalityType.personalityTypeId"
            >
              {{ personalityType.code
              }}{{ personalityType.name ? ` — ${personalityType.name}` : '' }}
            </option>
          </select>
        </label>
        <p v-if="error" class="inline-message error" role="alert">{{ error }}</p>
        <p v-if="message" class="inline-message success" role="status">{{ message }}</p>
        <div class="form-actions">
          <button class="button primary" :disabled="busy || !dirty">
            {{ busy ? 'Saving…' : profile ? 'Save changes' : 'Create profile' }}
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
    <section v-if="profile" class="danger-zone">
      <div>
        <h3>Deactivate profile</h3>
        <p class="small muted">
          Step away without losing your information. Signing in again will reactivate your profile.
        </p>
      </div>
      <button class="button secondary" :disabled="busy" @click="beginDeactivate">Deactivate</button>
    </section>
  </div>
  <AppModal
    v-if="deactivating"
    title="Deactivate your profile?"
    :busy="busy"
    @close="deactivating = false"
  >
    <p class="muted">
      Your questions, answers, Sparks, and payment history will be kept. You will be signed out now,
      and signing in again will reactivate your profile automatically.
    </p>
    <form class="stack-form" @submit.prevent="deactivate">
      <div class="form-actions">
        <button
          type="button"
          class="button secondary"
          :disabled="busy"
          @click="deactivating = false"
        >
          Keep profile active
        </button>
        <button class="button danger" :disabled="busy">
          {{ busy ? 'Deactivating…' : 'Yes, deactivate' }}
        </button>
      </div>
    </form>
  </AppModal>
</template>
