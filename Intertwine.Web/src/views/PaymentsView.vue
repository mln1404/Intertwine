<script setup lang="ts">
import { onBeforeMount, ref } from 'vue'
import { useIntertwine } from '../composables/useIntertwine'
import type { PaymentHistory } from '../models/question'
import LoadingState from '../components/LoadingState.vue'
const { loadPayments, reportError, canUseAccount, authOpen } = useIntertwine()
const payments = ref<PaymentHistory[]>([])
const page = ref(1)
const busy = ref(false)
const initialized = ref(false)
const error = ref('')
async function load(nextPage = page.value) {
  if (!canUseAccount.value || busy.value) return
  busy.value = true
  error.value = ''
  try {
    payments.value = await loadPayments(nextPage)
    page.value = nextPage
  } catch (cause) {
    error.value = reportError(cause)
  } finally {
    busy.value = false
    initialized.value = true
  }
}
function date(value: string) {
  return new Date(/[zZ]|[+-]\d\d:\d\d$/.test(value) ? value : `${value}Z`).toLocaleString()
}
onBeforeMount(() => void load())
</script>
<template>
  <header class="page-heading">
    <div>
      <p class="eyebrow">YOUR SPARKS</p>
      <h1>Payment history</h1>
      <p class="muted">Your Spark top-ups, all in one place.</p>
    </div>
    <a class="button primary" href="#wallet">Get Sparky ✨</a>
  </header>
  <div v-if="!canUseAccount" class="empty-state panel">
    <p>Sign in to see your payments.</p>
    <button class="button primary" @click="authOpen = true">Sign in</button>
  </div>
  <LoadingState v-else-if="busy || !initialized" message="Loading payment history…" panel />
  <section v-else class="panel payment-history">
    <div v-if="error" class="inline-message error" role="alert">
      {{ error }}
      <button class="text-button" @click="load()">Try again</button>
    </div>
    <p v-if="!error && !payments.length" class="muted">No Payment History found</p>
    <article v-for="payment in payments" :key="payment.userPaymentId" class="payment-row">
      <div>
        <strong>+{{ payment.sparksPurchased.toLocaleString() }} ✨</strong>
        <p class="small muted">{{ date(payment.dateCreated) }} · #{{ payment.userPaymentId }}</p>
      </div>
      <div>
        <strong>
          {{
            new Intl.NumberFormat(undefined, {
              style: 'currency',
              currency: payment.currencyCode,
            }).format(payment.amount)
          }}
        </strong>
        <p class="small">
          {{ payment.status
          }}{{ payment.paymentProvider === 'IntertwineDemo' ? ' · Simulated, no charge' : '' }}
        </p>
      </div>
    </article>
    <nav class="form-actions" aria-label="Payment pages">
      <button class="button secondary" :disabled="busy || page === 1" @click="load(page - 1)">
        Previous
      </button>
      <span>Page {{ page }}</span>
      <button
        class="button secondary"
        :disabled="busy || payments.length < 20"
        @click="load(page + 1)"
      >
        Next
      </button>
    </nav>
  </section>
</template>
