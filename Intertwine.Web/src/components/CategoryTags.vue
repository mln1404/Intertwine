<script setup lang="ts">
import type { Category } from '../models/question'
defineProps<{ categories: Category[] }>()
function color(value: string) {
  return /^#[0-9a-f]{6}$/i.test(value) ? value : '#3E5947'
}
</script>
<template>
  <span class="category-tags" aria-label="Question categories">
    <span v-for="category in categories" :key="category.categoryId" class="category-tag">
      <span
        class="category-dot"
        :style="{ backgroundColor: color(category.color) }"
        aria-hidden="true"
      ></span>
      {{ category.categoryName }}
    </span>
  </span>
</template>
