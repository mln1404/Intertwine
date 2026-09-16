<script setup lang="ts">
withDefaults(
  defineProps<{
    message?: string
    panel?: boolean
  }>(),
  {
    message: 'Loading…',
    panel: false,
  },
)
</script>

<template>
  <div class="loading-block" :class="{ panel }" role="status" aria-live="polite">
    <span class="spinner" aria-hidden="true"></span>
    {{ message }}
  </div>
</template>
