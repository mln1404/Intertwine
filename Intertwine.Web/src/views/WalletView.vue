<script setup lang="ts">
import { onMounted, ref, watch } from 'vue'
import { useIntertwine } from '../composables/useIntertwine'
import type { CreditPackage } from '../models/question'
import AppIcon from '../components/AppIcon.vue'
const { balance, packages, currencies, canUseAccount, authOpen, loadWallet, topUp, reportError } =
  useIntertwine()
const currency = ref('')
const initializing = ref(true)
const loading = ref(false)
const busy = ref(false)
const error = ref('')
const message = ref('')
const uncertain = ref(false)
const money = (offer: CreditPackage) =>
  new Intl.NumberFormat(undefined, { style: 'currency', currency: offer.currencyCode }).format(
    offer.amount,
  )

async function load() {
  if (!canUseAccount.value) return
  loading.value = true
  error.value = ''
  packages.value = []
  try {
    const selectedCurrency = await loadWallet(currency.value)
    if (selectedCurrency) currency.value = selectedCurrency
  } catch (cause) {
    error.value = reportError(cause)
  } finally {
    loading.value = false
  }
}
async function purchase(offer: CreditPackage) {
  if (busy.value || uncertain.value) return
  busy.value = true
  error.value = ''
  message.value = ''
  try {
    await topUp(offer)
    message.value = `${offer.credits.toLocaleString()} Sparks added ✨. No payment was taken.`
  } catch (cause) {
    error.value = reportError(cause)
    uncertain.value = true
  } finally {
    busy.value = false
  }
}
watch(currency, () => {
  if (!initializing.value) void load()
})
onMounted(async () => {
  await load()
  initializing.value = false
})
</script>
<template>
  <header class="page-heading">
    <div>
      <p class="eyebrow">A LITTLE POSSIBILITY</p>
      <h1>Get Sparky ✨</h1>
      <p class="muted">A few more possibilities. Get more Sparks.</p>
    </div>
    <span class="heading-emblem">
      <AppIcon name="wallet" :size="28" />
    </span>
  </header>
  <div v-if="!canUseAccount" class="empty-state panel">
    <AppIcon name="wallet" :size="36" />
    <h2>A space for your Sparks.</h2>
    <p>Sign in to view your Spark balance and available packages.</p>
    <button class="button primary" @click="authOpen = true">Sign in</button>
  </div>
  <template v-else>
    <section class="wallet-banner">
      <div>
        <span class="eyebrow">SPARK BALANCE</span>
        <a class="balance" href="#payments" aria-label="Spark balance and payment history">
          {{ balance === null ? '—' : balance.toLocaleString() }}
          <span>✨</span>
        </a>
        <p>Simulated top-ups: click a package to add Sparks immediately. No payment is taken.</p>
        <a class="text-link" href="#payments">View payment history →</a>
      </div>
      <AppIcon name="wallet" :size="76" />
    </section>
    <p v-if="error" class="inline-message error" role="alert">
      {{ error }}
      <button class="text-button" :disabled="loading" @click="load">Refresh wallet</button>
    </p>
    <p v-if="message" class="inline-message success" role="status">{{ message }}</p>
    <div v-if="uncertain" class="soft-panel">
      <h3>Check your balance before another top-up.</h3>
      <p>
        The previous request may have completed. Refresh your wallet to review the balance. Top-ups
        are never automatically retried.
      </p>
      <button class="button secondary" :disabled="loading" @click="load">Refresh balance</button>
      <button class="text-button" @click="uncertain = false">I’ve checked my balance</button>
    </div>
    <section class="explore-section">
      <div class="section-heading">
        <div>
          <p class="eyebrow">ROOM FOR MORE</p>
          <h2>Add a few possibilities</h2>
        </div>
        <label class="currency-label">
          Currency
          <select v-model="currency" :disabled="loading || busy || !currencies.length">
            <option v-for="item in currencies" :key="item.code" :value="item.code">
              {{ item.code }} · {{ item.name }}
            </option>
          </select>
        </label>
      </div>
      <div v-if="loading" class="loading-block" role="status">Loading your wallet…</div>
      <div v-else-if="packages.length" class="question-grid">
        <article
          v-for="(offer, index) in packages"
          :key="offer.creditPackageId"
          class="package-card"
          :class="{ featured: index === 1 }"
        >
          <span class="tile-icon tone-0">
            <AppIcon name="spark" :size="23" />
          </span>
          <h3>{{ offer.credits.toLocaleString() }} Sparks ✨</h3>
          <p class="package-price">
            {{ money(offer) }}
            <span>{{ offer.currencyCode }}</span>
          </p>
          <button
            class="button full"
            :class="index === 1 ? 'primary' : 'secondary'"
            :disabled="busy || uncertain"
            @click="purchase(offer)"
          >
            {{ busy ? 'Adding Sparks…' : 'Get Sparks' }}
            <AppIcon name="arrow" :size="16" />
          </button>
        </article>
      </div>
      <div v-else class="empty-state panel">
        <h3>No Credit Packages found</h3>
        <p v-if="currencies.length">No active packages are connected to {{ currency }}.</p>
        <p v-else>No active currencies were returned by the API.</p>
      </div>
      <p class="small muted wallet-footnote">
        <AppIcon name="lock" :size="14" />
        This records a simulated payment. No card details are collected and no money is charged.
      </p>
    </section>
  </template>
</template>
