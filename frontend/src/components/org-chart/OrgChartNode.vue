<template>
  <div
    class="org-chart-node"
    :class="[
      nodeTypeClass,
      { selected: selected, 'has-children': hasChildren },
    ]"
    @click.stop="$emit('click')"
  >
    <!-- Expand/Collapse Toggle -->
    <button
      v-if="hasChildren"
      class="toggle-btn"
      @click.stop="$emit('toggle')"
      :aria-label="expanded ? 'Collapse' : 'Expand'"
    >
      <span class="toggle-icon">{{ expanded ? "−" : "+" }}</span>
    </button>
    <div v-else class="toggle-spacer"></div>

    <!-- Node Content -->
    <div class="node-content">
      <!-- Avatar or Icon -->
      <div class="avatar" :class="nodeTypeClass">
        <img
          v-if="node.metadata.imageUrl"
          :src="node.metadata.imageUrl"
          :alt="node.name"
        />
        <span v-else class="avatar-initials">{{ initials }}</span>
      </div>

      <!-- Info -->
      <div class="info">
        <div class="name-row">
          <span class="name">{{ node.name }}</span>
          <span v-if="node.metadata.title" class="title-badge">{{
            node.metadata.title
          }}</span>
        </div>

        <div class="meta-row">
          <span v-if="node.nodeType === 'Site'" class="meta-item">
            {{ node.metadata.city }}, {{ node.metadata.state }}
          </span>
          <span v-if="node.nodeType === 'Team'" class="meta-item">
            {{ node.directReportCount }} members
          </span>
          <span
            v-if="isPersonNode && node.metadata.tenureYears != null"
            class="meta-item"
          >
            {{ formatTenure(node.metadata.tenureYears) }}
          </span>
          <span
            v-if="isPersonNode && node.metadata.email"
            class="meta-item email"
          >
            {{ node.metadata.email }}
          </span>
        </div>

        <!-- Tags -->
        <div v-if="node.tags.length > 0" class="tags-row">
          <OrgChartTagPill
            v-for="tag in visibleTags"
            :key="tag.id"
            :tag="tag"
          />
          <span v-if="hiddenTagCount > 0" class="more-tags">
            +{{ hiddenTagCount }}
          </span>
        </div>
      </div>

      <!-- Counts Badge -->
      <div v-if="node.totalReportCount > 0" class="count-badge">
        {{ node.totalReportCount }}
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from "vue";
import type { OrgChartNode } from "@/types/orgChart";
import OrgChartTagPill from "./OrgChartTagPill.vue";

const props = defineProps<{
  node: OrgChartNode;
  expanded: boolean;
  selected: boolean;
}>();

defineEmits<{
  click: [];
  toggle: [];
}>();

const hasChildren = computed(() => props.node.children.length > 0);

const nodeTypeClass = computed(() => props.node.nodeType.toLowerCase());

const isPersonNode = computed(
  () => props.node.nodeType === "Manager" || props.node.nodeType === "Agent",
);

const initials = computed(() => {
  const name = props.node.name;
  const parts = name.split(" ");
  if (parts.length >= 2) {
    return `${parts[0][0]}${parts[parts.length - 1][0]}`.toUpperCase();
  }
  return name.substring(0, 2).toUpperCase();
});

const maxVisibleTags = 3;
const visibleTags = computed(() => props.node.tags.slice(0, maxVisibleTags));
const hiddenTagCount = computed(() =>
  Math.max(0, props.node.tags.length - maxVisibleTags),
);

function formatTenure(years: number): string {
  if (years < 1) {
    const months = Math.round(years * 12);
    return `${months} mo`;
  }
  return `${years.toFixed(1)} yr`;
}
</script>

<style scoped>
.org-chart-node {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 12px 16px;
  background: white;
  border: 1px solid #e5e7eb;
  border-radius: 8px;
  cursor: pointer;
  transition: all 0.15s ease;
}

.org-chart-node:hover {
  border-color: #3b82f6;
  box-shadow: 0 2px 8px rgba(59, 130, 246, 0.15);
}

.org-chart-node.selected {
  border-color: #3b82f6;
  background: #eff6ff;
}

/* Node type specific styles */
.org-chart-node.site {
  background: linear-gradient(135deg, #f0f9ff 0%, #e0f2fe 100%);
  border-left: 4px solid #0ea5e9;
}

.org-chart-node.team {
  background: linear-gradient(135deg, #f0fdf4 0%, #dcfce7 100%);
  border-left: 4px solid #22c55e;
}

.org-chart-node.manager {
  border-left: 4px solid #8b5cf6;
}

.org-chart-node.agent {
  border-left: 4px solid #6b7280;
}

/* Toggle Button */
.toggle-btn {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 24px;
  height: 24px;
  border: 1px solid #d1d5db;
  border-radius: 4px;
  background: white;
  cursor: pointer;
  flex-shrink: 0;
  transition: all 0.15s ease;
}

.toggle-btn:hover {
  background: #f3f4f6;
  border-color: #9ca3af;
}

.toggle-icon {
  font-size: 14px;
  font-weight: bold;
  color: #6b7280;
}

.toggle-spacer {
  width: 24px;
  flex-shrink: 0;
}

/* Node Content */
.node-content {
  display: flex;
  align-items: center;
  gap: 12px;
  flex: 1;
  min-width: 0;
}

/* Avatar */
.avatar {
  width: 40px;
  height: 40px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
  font-size: 14px;
  font-weight: 600;
  color: white;
  background: #6b7280;
}

.avatar.site {
  background: #0ea5e9;
  border-radius: 8px;
}

.avatar.team {
  background: #22c55e;
  border-radius: 8px;
}

.avatar.manager {
  background: #8b5cf6;
}

.avatar.agent {
  background: #6b7280;
}

.avatar img {
  width: 100%;
  height: 100%;
  border-radius: inherit;
  object-fit: cover;
}

/* Info */
.info {
  flex: 1;
  min-width: 0;
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.name-row {
  display: flex;
  align-items: center;
  gap: 8px;
  flex-wrap: wrap;
}

.name {
  font-weight: 600;
  color: #1f2937;
  font-size: 0.95rem;
}

.title-badge {
  font-size: 0.7rem;
  padding: 2px 6px;
  background: #e5e7eb;
  color: #4b5563;
  border-radius: 4px;
  text-transform: uppercase;
  font-weight: 500;
}

.meta-row {
  display: flex;
  align-items: center;
  gap: 12px;
  flex-wrap: wrap;
}

.meta-item {
  font-size: 0.8rem;
  color: #6b7280;
}

.meta-item.email {
  color: #3b82f6;
}

.tags-row {
  display: flex;
  align-items: center;
  gap: 4px;
  flex-wrap: wrap;
  margin-top: 4px;
}

.more-tags {
  font-size: 0.7rem;
  color: #6b7280;
  font-weight: 500;
}

/* Count Badge */
.count-badge {
  display: flex;
  align-items: center;
  justify-content: center;
  min-width: 28px;
  height: 28px;
  padding: 0 8px;
  background: #e5e7eb;
  border-radius: 14px;
  font-size: 0.8rem;
  font-weight: 600;
  color: #4b5563;
  flex-shrink: 0;
}
</style>
