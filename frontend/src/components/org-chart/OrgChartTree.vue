<template>
  <div
    class="org-chart-tree"
    :class="[`depth-${node.depth}`, node.nodeType.toLowerCase()]"
    :style="{ '--depth': node.depth }"
  >
    <!-- Current Node -->
    <OrgChartNode
      :node="node"
      :expanded="isExpanded"
      :selected="isSelected"
      @click="$emit('select', node.id)"
      @toggle="$emit('toggle', node.id)"
    />

    <!-- Children (with transition) -->
    <Transition name="expand">
      <div v-if="isExpanded && node.children.length > 0" class="children">
        <OrgChartTree
          v-for="child in node.children"
          :key="child.id"
          :node="child"
          :expanded-ids="expandedIds"
          :selected-id="selectedId"
          @toggle="$emit('toggle', $event)"
          @select="$emit('select', $event)"
        />
      </div>
    </Transition>
  </div>
</template>

<script setup lang="ts">
import { computed } from "vue";
import type { OrgChartNode as OrgChartNodeType } from "@/types/orgChart";
import OrgChartNode from "./OrgChartNode.vue";

const props = defineProps<{
  node: OrgChartNodeType;
  expandedIds: Set<string>;
  selectedId: string | null;
}>();

defineEmits<{
  toggle: [id: string];
  select: [id: string];
}>();

const isExpanded = computed(() => props.expandedIds.has(props.node.id));
const isSelected = computed(() => props.selectedId === props.node.id);
</script>

<style scoped>
.org-chart-tree {
  display: flex;
  flex-direction: column;
}

.children {
  margin-left: calc(var(--depth, 0) * 8px + 24px);
  padding-left: 16px;
  border-left: 2px solid #e5e7eb;
  margin-top: 8px;
  display: flex;
  flex-direction: column;
  gap: 8px;
}

/* Depth-based border colors */
.depth-1 > .children {
  border-left-color: #0ea5e9;
}

.depth-2 > .children {
  border-left-color: #22c55e;
}

.depth-3 > .children {
  border-left-color: #8b5cf6;
}

/* Expand/Collapse Animation */
.expand-enter-active,
.expand-leave-active {
  transition: all 0.2s ease;
  overflow: hidden;
}

.expand-enter-from,
.expand-leave-to {
  opacity: 0;
  transform: translateY(-8px);
}
</style>
