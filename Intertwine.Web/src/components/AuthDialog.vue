<script setup lang="ts">
import { reactive, ref } from 'vue'
import AppModal from './AppModal.vue'
import { useIntertwine } from '../composables/useIntertwine'
const { authenticate, authOpen, reportError } = useIntertwine()
const register = ref(false)
const busy = ref(false)
const error = ref('')
const message = ref('')
const form = reactive({ email: '', password: '', firstName: '', lastName: '' })

function toggleRegistration() {
  register.value = !register.value
  error.value = ''
  message.value = ''
}

async function submit() {
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
  <AppModal
    :title="register ? 'Your story starts here.' : 'A little closer, every day.'"
    :busy="busy"
    @close="authOpen = false"
  >
    <p class="muted">
      {{ register ? 'Create your Intertwine account.' : 'Sign in to your Intertwine account.' }}
    </p>
    <form class="stack-form" @submit.prevent="submit">
      <div v-if="register" class="form-row">
        <label>
          First name
          <input v-model.trim="form.firstName" autocomplete="given-name" required maxlength="100" />
        </label>
        <label>
          Last name
          <input v-model.trim="form.lastName" autocomplete="family-name" required maxlength="100" />
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
      <button type="button" class="text-button" :disabled="busy" @click="toggleRegistration">
        {{ register ? 'Already have an account? Sign in' : 'New here? Create an account' }}
      </button>
    </form>
  </AppModal>
</template>
