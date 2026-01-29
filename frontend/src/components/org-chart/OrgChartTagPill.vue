<template>
  <span class="org-chart-tag-pill" :style="{ backgroundColor: tagColor }">
    {{ tag.name }}
  </span>
</template>

<script setup lang="ts">
import { computed } from "vue";
import type { Tag } from "@/types/orgChart";

const props = defineProps<{
  tag: Tag;
}>();

const tagColor = computed(() => {
  return props.tag.hexColorCode ? `#${props.tag.hexColorCode}` : "#6b7280";
});
</script>

<style scoped>
.org-chart-tag-pill {
  display: inline-flex;
  align-items: center;
  padding: 2px 8px;
  border-radius: 9999px;
  font-size: 0.7rem;
  font-weight: 500;
  color: white;
  text-shadow: 0 1px 2px rgba(0, 0, 0, 0.2);
  white-space: nowrap;
}
</style>
