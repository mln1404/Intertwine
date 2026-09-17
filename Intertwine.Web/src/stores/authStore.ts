import { computed, ref } from 'vue'
import { defineStore } from 'pinia'

const tokenKey = 'intertwine.token'
const userKey = 'intertwine.user'

export const useAuthStore = defineStore('auth', () => {
  const accessToken = ref<string | null>(sessionStorage.getItem(tokenKey))
  const userId = ref<string | null>(sessionStorage.getItem(userKey))
  const signedIn = computed(() => Boolean(accessToken.value))

  function setSession(token: string, id: string | null) {
    accessToken.value = token
    userId.value = id ?? ''
    sessionStorage.setItem(tokenKey, token)
    sessionStorage.setItem(userKey, userId.value)
  }

  function clearSession() {
    accessToken.value = null
    userId.value = null
    sessionStorage.removeItem(tokenKey)
    sessionStorage.removeItem(userKey)
  }

  return { accessToken, userId, signedIn, setSession, clearSession }
})
