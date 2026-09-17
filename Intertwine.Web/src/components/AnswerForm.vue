<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import type { Question } from '../models/question'
import { useIntertwine } from '../composables/useIntertwine'
import AppIcon from './AppIcon.vue'
const props = defineProps<{ question: Question; date: string }>()
const { activity, daily, answer, canUseAccount, authOpen, balance } = useIntertwine()
const spendSparks = ref(false)
const needsSparks = computed(() => !isDaily.value && activity.value.remaining === 0)
const selected = ref<number | null>(null)
const busy = ref(false)
const error = ref('')
const success = ref(false)
const isDaily = computed(() => props.question.questionId === daily.value?.questionId)
const locked = computed(() => isDaily.value && activity.value.daily)
watch(
  () => [props.question.questionId, props.date],
  () => {
    selected.value = activity.value.answers[props.question.questionId] ?? null
    error.value = ''
    success.value = false
  },
  { immediate: true },
)
async function submit() {
  if (selected.value === null || busy.value) return
  busy.value = true
  error.value = ''
  success.value = false
  try {
    success.value = await answer(props.question, selected.value, props.date, spendSparks.value)
    if (success.value) spendSparks.value = false
  } catch (cause) {
    error.value = cause instanceof Error ? cause.message : 'Unable to save your answer.'
  } finally {
    busy.value = false
  }
}
</script>
<template>
  <form @submit.prevent="submit">
    <fieldset class="answer-options" :disabled="busy || locked">
      <legend class="sr-only">Choose your answer</legend>
      <label
        v-for="(option, index) in question.answers"
        :key="option.answerId"
        class="answer-option"
        :class="{ selected: selected === option.answerId }"
      >
        <input
          v-model="selected"
          type="radio"
          :name="`question-${question.questionId}`"
          :value="option.answerId"
        />
        <span class="answer-letter">{{ String.fromCharCode(65 + index) }}</span>
        <span>{{ option.answerText }}</span>
        <span class="radio-mark">
          <AppIcon v-if="selected === option.answerId" name="check" :size="13" />
        </span>
      </label>
    </fieldset>
    <p v-if="!question.answers.length" class="inline-message">
      This question doesn’t have any available answers yet.
    </p>
    <p v-if="error" class="inline-message error" role="alert">{{ error }}</p>
    <div v-if="needsSparks && canUseAccount" class="spark-panel">
      <p>Your two free actions are used. Each extra answer or update costs 10 ✨.</p>
      <label v-if="(balance ?? 0) >= 10">
        <input v-model="spendSparks" type="checkbox" />
        Spend 10 Sparks on this answer
      </label>
      <RouterLink v-else class="button primary" to="/wallet">Get more Sparks ✨</RouterLink>
    </div>
    <p v-if="success || locked" class="inline-message success" role="status">
      <AppIcon name="check" :size="18" />
      Your answer is saved.
      {{
        isDaily
          ? 'A little more you, shared. See you tomorrow.'
          : 'Thanks for sharing a little about yourself.'
      }}
    </p>
    <div class="answer-footer">
      <span class="small muted">
        <AppIcon name="lock" :size="14" />
        {{
          isDaily ? 'One answer. Just be yourself.' : 'New or changed answers use a daily action.'
        }}
      </span>
      <button v-if="!canUseAccount" type="button" class="button primary" @click="authOpen = true">
        Sign in to answer
        <AppIcon name="arrow" :size="17" />
      </button>
      <button
        v-else
        class="button primary"
        :disabled="
          selected === null ||
          busy ||
          locked ||
          !question.answers.length ||
          (needsSparks && (!spendSparks || (balance ?? 0) < 10))
        "
      >
        {{
          busy
            ? 'Saving…'
            : locked
              ? 'Answered today'
              : needsSparks
                ? 'Spend 10 ✨ & save'
                : 'Save my answer'
        }}
        <AppIcon :name="locked ? 'check' : 'arrow'" :size="17" />
      </button>
    </div>
  </form>
</template>
