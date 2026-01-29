<template>
  <Transition name="slide">
    <div v-if="node" class="org-chart-detail-panel">
      <!-- Header -->
      <div class="panel-header">
        <div class="header-content">
          <div class="avatar" :class="nodeTypeClass">
            <img
              v-if="node.metadata.imageUrl"
              :src="node.metadata.imageUrl"
              :alt="node.name"
            />
            <span v-else class="avatar-initials">{{ initials }}</span>
          </div>
          <div class="header-info">
            <h2 class="name">{{ node.name }}</h2>
            <span v-if="node.metadata.title" class="title">{{
              node.metadata.title
            }}</span>
          </div>
        </div>
        <button class="close-btn" @click="$emit('close')" aria-label="Close">
          <span>×</span>
        </button>
      </div>

      <!-- Tags -->
      <div v-if="node.tags.length > 0" class="tags-section">
        <OrgChartTagPill v-for="tag in node.tags" :key="tag.id" :tag="tag" />
      </div>

      <!-- Details -->
      <div class="details-section">
        <h3 class="section-title">Details</h3>

        <div class="detail-grid">
          <!-- Node Type -->
          <div class="detail-item">
            <span class="detail-label">Type</span>
            <span class="detail-value">{{ node.nodeType }}</span>
          </div>

          <!-- Site -->
          <div v-if="node.metadata.siteName" class="detail-item">
            <span class="detail-label">Site</span>
            <span class="detail-value">{{ node.metadata.siteName }}</span>
          </div>

          <!-- Team -->
          <div v-if="node.metadata.teamName" class="detail-item">
            <span class="detail-label">Team</span>
            <span class="detail-value">{{ node.metadata.teamName }}</span>
          </div>

          <!-- Location -->
          <div v-if="node.metadata.city" class="detail-item">
            <span class="detail-label">Location</span>
            <span class="detail-value"
              >{{ node.metadata.city }}, {{ node.metadata.state }}</span
            >
          </div>

          <!-- Email -->
          <div v-if="node.metadata.email" class="detail-item">
            <span class="detail-label">Email</span>
            <a
              :href="`mailto:${node.metadata.email}`"
              class="detail-value email"
            >
              {{ node.metadata.email }}
            </a>
          </div>

          <!-- Phone -->
          <div v-if="node.metadata.phone" class="detail-item">
            <span class="detail-label">Phone</span>
            <a :href="`tel:${node.metadata.phone}`" class="detail-value">
              {{ node.metadata.phone }}
            </a>
          </div>

          <!-- Agent Status -->
          <div v-if="node.metadata.agentStatus" class="detail-item">
            <span class="detail-label">Status</span>
            <span
              class="status-badge"
              :class="node.metadata.agentStatus.toLowerCase()"
            >
              {{ node.metadata.agentStatus }}
            </span>
          </div>

          <!-- Agent Type -->
          <div v-if="node.metadata.agentType" class="detail-item">
            <span class="detail-label">Agent Type</span>
            <span class="detail-value">{{ node.metadata.agentType }}</span>
          </div>

          <!-- Commission Tier -->
          <div
            v-if="
              node.metadata.commissionTier &&
              node.metadata.commissionTier !== 'None'
            "
            class="detail-item"
          >
            <span class="detail-label">Commission Tier</span>
            <span class="detail-value">{{
              formatTier(node.metadata.commissionTier)
            }}</span>
          </div>

          <!-- Start Date -->
          <div v-if="node.metadata.agencyStartDate" class="detail-item">
            <span class="detail-label">Start Date</span>
            <span class="detail-value">{{
              formatDate(node.metadata.agencyStartDate)
            }}</span>
          </div>

          <!-- Tenure -->
          <div v-if="node.metadata.tenureYears != null" class="detail-item">
            <span class="detail-label">Tenure</span>
            <span class="detail-value">{{
              formatTenure(node.metadata.tenureYears)
            }}</span>
          </div>

          <!-- Direct Reports -->
          <div v-if="node.directReportCount > 0" class="detail-item">
            <span class="detail-label">Direct Reports</span>
            <span class="detail-value">{{ node.directReportCount }}</span>
          </div>

          <!-- Total Reports -->
          <div v-if="node.totalReportCount > 0" class="detail-item">
            <span class="detail-label">Total in Org</span>
            <span class="detail-value">{{ node.totalReportCount }}</span>
          </div>
        </div>
      </div>

      <!-- Actions (for person nodes) -->
      <div v-if="isPersonNode" class="actions-section">
        <h3 class="section-title">Quick Actions</h3>
        <div class="action-buttons">
          <button class="action-btn primary" @click="viewProfile">
            View Full Profile
          </button>
          <button class="action-btn secondary" @click="sendEmail">
            Send Email
          </button>
        </div>
      </div>
    </div>
  </Transition>
</template>

<script setup lang="ts">
import { computed } from "vue";
import type { OrgChartNode } from "@/types/orgChart";
import OrgChartTagPill from "./OrgChartTagPill.vue";

const props = defineProps<{
  node: OrgChartNode | null;
}>();

defineEmits<{
  close: [];
}>();

const nodeTypeClass = computed(() => props.node?.nodeType.toLowerCase() ?? "");

const isPersonNode = computed(
  () => props.node?.nodeType === "Manager" || props.node?.nodeType === "Agent",
);

const initials = computed(() => {
  if (!props.node) return "";
  const name = props.node.name;
  const parts = name.split(" ");
  if (parts.length >= 2) {
    return `${parts[0][0]}${parts[parts.length - 1][0]}`.toUpperCase();
  }
  return name.substring(0, 2).toUpperCase();
});

function formatTier(tier: string): string {
  return tier.replace("Tier", "Tier ");
}

function formatDate(dateStr: string): string {
  const date = new Date(dateStr);
  return date.toLocaleDateString("en-US", {
    year: "numeric",
    month: "short",
    day: "numeric",
  });
}

function formatTenure(years: number): string {
  if (years < 1) {
    const months = Math.round(years * 12);
    return `${months} month${months !== 1 ? "s" : ""}`;
  }
  const y = Math.floor(years);
  const m = Math.round((years - y) * 12);
  if (m === 0) {
    return `${y} year${y !== 1 ? "s" : ""}`;
  }
  return `${y} year${y !== 1 ? "s" : ""}, ${m} month${m !== 1 ? "s" : ""}`;
}

function viewProfile() {
  if (props.node) {
    console.log("View profile:", props.node.id);
    // Could navigate to a dedicated profile page
  }
}

function sendEmail() {
  if (props.node?.metadata.email) {
    window.location.href = `mailto:${props.node.metadata.email}`;
  }
}
</script>

<style scoped>
.org-chart-detail-panel {
  position: fixed;
  top: 0;
  right: 0;
  width: 400px;
  max-width: 100vw;
  height: 100vh;
  background: white;
  border-left: 1px solid #e5e7eb;
  box-shadow: -4px 0 24px rgba(0, 0, 0, 0.1);
  overflow-y: auto;
  z-index: 1000;
  display: flex;
  flex-direction: column;
}

/* Slide Transition */
.slide-enter-active,
.slide-leave-active {
  transition: transform 0.3s ease;
}

.slide-enter-from,
.slide-leave-to {
  transform: translateX(100%);
}

/* Header */
.panel-header {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  padding: 24px;
  border-bottom: 1px solid #e5e7eb;
  background: linear-gradient(135deg, #f9fafb 0%, #f3f4f6 100%);
}

.header-content {
  display: flex;
  gap: 16px;
  align-items: center;
}

.avatar {
  width: 64px;
  height: 64px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 20px;
  font-weight: 600;
  color: white;
  background: #6b7280;
  flex-shrink: 0;
}

.avatar.site {
  background: #0ea5e9;
  border-radius: 12px;
}

.avatar.team {
  background: #22c55e;
  border-radius: 12px;
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

.header-info {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.name {
  font-size: 1.25rem;
  font-weight: 700;
  color: #1f2937;
  margin: 0;
}

.title {
  font-size: 0.875rem;
  color: #6b7280;
  text-transform: capitalize;
}

.close-btn {
  width: 32px;
  height: 32px;
  display: flex;
  align-items: center;
  justify-content: center;
  background: white;
  border: 1px solid #d1d5db;
  border-radius: 6px;
  cursor: pointer;
  font-size: 20px;
  color: #6b7280;
  transition: all 0.15s ease;
}

.close-btn:hover {
  background: #f3f4f6;
  border-color: #9ca3af;
}

/* Tags */
.tags-section {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
  padding: 16px 24px;
  border-bottom: 1px solid #e5e7eb;
}

/* Details */
.details-section {
  padding: 24px;
  flex: 1;
}

.section-title {
  font-size: 0.75rem;
  font-weight: 600;
  color: #6b7280;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  margin: 0 0 16px 0;
}

.detail-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 16px;
}

.detail-item {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.detail-label {
  font-size: 0.75rem;
  color: #9ca3af;
  text-transform: uppercase;
  letter-spacing: 0.03em;
}

.detail-value {
  font-size: 0.9rem;
  color: #1f2937;
  font-weight: 500;
}

.detail-value.email {
  color: #3b82f6;
  text-decoration: none;
}

.detail-value.email:hover {
  text-decoration: underline;
}

.status-badge {
  display: inline-flex;
  align-items: center;
  padding: 4px 10px;
  border-radius: 12px;
  font-size: 0.75rem;
  font-weight: 600;
  text-transform: uppercase;
  width: fit-content;
}

.status-badge.active {
  background: #dcfce7;
  color: #166534;
}

.status-badge.inactive {
  background: #f3f4f6;
  color: #6b7280;
}

/* Actions */
.actions-section {
  padding: 24px;
  border-top: 1px solid #e5e7eb;
  background: #f9fafb;
}

.action-buttons {
  display: flex;
  gap: 12px;
}

.action-btn {
  flex: 1;
  padding: 12px 16px;
  border-radius: 8px;
  font-size: 0.875rem;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.15s ease;
}

.action-btn.primary {
  background: #3b82f6;
  color: white;
  border: none;
}

.action-btn.primary:hover {
  background: #2563eb;
}

.action-btn.secondary {
  background: white;
  color: #4b5563;
  border: 1px solid #d1d5db;
}

.action-btn.secondary:hover {
  background: #f3f4f6;
  border-color: #9ca3af;
}

@media (max-width: 500px) {
  .org-chart-detail-panel {
    width: 100vw;
  }

  .detail-grid {
    grid-template-columns: 1fr;
  }
}
</style>
