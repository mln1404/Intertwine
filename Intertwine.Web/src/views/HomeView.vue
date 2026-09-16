<script setup lang="ts">
import { reactive, ref } from 'vue'
import AppIcon from '../components/AppIcon.vue'
import { useIntertwine } from '../composables/useIntertwine'

const { authenticate, reportError } = useIntertwine()
const register = ref(false)
const busy = ref(false)
const error = ref('')
const message = ref('')
const form = reactive({ email: '', password: '', firstName: '', lastName: '' })

function switchMode() {
  register.value = !register.value
  error.value = ''
  message.value = ''
}

async function submit() {
  if (busy.value) return
  busy.value = true
  error.value = ''
  message.value = ''
  try {
    const loggedIn = await authenticate(form, register.value)
    if (!loggedIn) {
      register.value = false
      form.password = ''
      message.value = 'Your account and profile are ready. Sign in to continue.'
    }
  } catch (cause) {
    error.value = reportError(cause)
  } finally {
    busy.value = false
  }
}
</script>

<template>
  <main class="public-home">
    <section class="public-story">
      <a class="brand public-brand" href="#" aria-label="Intertwine home">
        <AppIcon name="heart" :size="34" />
        <span>
          intertwine
          <span class="brand-dot">.</span>
        </span>
      </a>
      <div>
        <p class="eyebrow">A LITTLE CLOSER, EVERY DAY</p>
        <h1>Meaningful connections start with being yourself.</h1>
        <p>
          Answer thoughtful questions, discover what matters to you, and let your story unfold one
          honest moment at a time.
        </p>
      </div>
      <p class="small">Your questions, answers, profile, and Sparks stay inside your account.</p>
    </section>

    <section class="public-auth" aria-labelledby="auth-heading">
      <div class="auth-card">
        <p class="eyebrow">{{ register ? 'JOIN INTERTWINE' : 'WELCOME BACK' }}</p>
        <h2 id="auth-heading">
          {{ register ? 'Your story starts here.' : 'Sign in to your space.' }}
        </h2>
        <p class="muted">
          {{
            register ? 'Create an account to begin.' : 'Use your Intertwine account to continue.'
          }}
        </p>
        <form class="stack-form" @submit.prevent="submit">
          <div v-if="register" class="form-row">
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
            Email address
            <input v-model.trim="form.email" type="email" autocomplete="email" required />
          </label>
          <label>
            Password
            <input
              v-model="form.password"
              type="password"
              :autocomplete="register ? 'new-password' : 'current-password'"
              :minlength="register ? 6 : undefined"
              required
            />
          </label>
          <p v-if="register" class="small muted">
            Use at least 6 characters, including uppercase, lowercase, a number, and a symbol.
          </p>
          <p v-if="error" class="inline-message error" role="alert">{{ error }}</p>
          <p v-if="message" class="inline-message success" role="status">{{ message }}</p>
          <button class="button primary full" :disabled="busy">
            {{ busy ? 'Please wait…' : register ? 'Create account' : 'Sign in' }}
          </button>
          <button type="button" class="text-button" :disabled="busy" @click="switchMode">
            {{ register ? 'Already have an account? Sign in' : 'New here? Create an account' }}
          </button>
        </form>
      </div>
    </section>
  </main>
</template>
