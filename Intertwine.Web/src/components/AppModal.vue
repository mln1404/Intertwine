<script setup lang="ts">
import { onMounted, onUnmounted, ref } from 'vue'
import AppIcon from './AppIcon.vue'
const props = defineProps<{ title: string; busy?: boolean }>()
const emit = defineEmits<{ close: [] }>()
const dialog = ref<HTMLDialogElement>()
let previous: HTMLElement | null = null
function close() {
  if (!props.busy) emit('close')
}
onMounted(() => {
  previous = document.activeElement as HTMLElement
  dialog.value?.showModal()
})
onUnmounted(() => {
  previous?.focus()
})
</script>
<template>
  <dialog
    ref="dialog"
    class="modal"
    aria-labelledby="modal-title"
    @cancel.prevent="close"
    @click="$event.target === dialog && close()"
  >
    <div class="modal-body">
      <button
        class="icon-button modal-close"
        aria-label="Close dialog"
        :disabled="busy"
        @click="close"
      >
        <AppIcon name="close" />
      </button>
      <h2 id="modal-title">{{ title }}</h2>
      <slot />
    </div>
  </dialog>
</template>
