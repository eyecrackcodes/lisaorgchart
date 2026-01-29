<template>
  <Transition name="slide">
    <div v-if="detail" class="graph-detail-panel">
      <!-- Header -->
      <div class="panel-header" :style="headerStyle">
        <button class="close-btn" @click="$emit('close')" aria-label="Close">
          <span>×</span>
        </button>
        
        <div class="header-content">
          <div class="avatar" :style="avatarStyle">
            <span class="initials">{{ initials }}</span>
          </div>
          <h2 class="name">{{ detail.name }}</h2>
          <p class="type">{{ detail.nodeType }}</p>
        </div>

        <!-- Health Score Ring -->
        <div class="health-ring" :class="healthClass">
          <svg viewBox="0 0 36 36" class="circular-chart">
            <path
              class="circle-bg"
              d="M18 2.0845
                a 15.9155 15.9155 0 0 1 0 31.831
                a 15.9155 15.9155 0 0 1 0 -31.831"
            />
            <path
              class="circle"
              :stroke-dasharray="`${healthPercent}, 100`"
              d="M18 2.0845
                a 15.9155 15.9155 0 0 1 0 31.831
                a 15.9155 15.9155 0 0 1 0 -31.831"
            />
          </svg>
          <span class="health-value">{{ healthPercent }}%</span>
        </div>
      </div>

      <!-- Tags -->
      <div v-if="detail.tags.length > 0" class="tags-section">
        <span 
          v-for="tag in detail.tags" 
          :key="tag.id"
          class="tag-pill"
          :class="`category-${tag.category}`"
          :style="{ backgroundColor: `#${tag.hexColor}20`, color: `#${tag.hexColor}` }"
        >
          {{ tag.name }}
        </span>
      </div>

      <!-- Quick Stats -->
      <div class="stats-section">
        <div class="stat-card active">
          <span class="stat-icon">👥</span>
          <span class="stat-value">{{ detail.stats.activeAgents }}</span>
          <span class="stat-label">Active</span>
        </div>
        <div v-if="detail.stats.trainingAgents > 0" class="stat-card training">
          <span class="stat-icon">🎓</span>
          <span class="stat-value">{{ detail.stats.trainingAgents }}</span>
          <span class="stat-label">Training</span>
        </div>
        <div class="stat-card tenure">
          <span class="stat-icon">📅</span>
          <span class="stat-value">{{ formatTenure(detail.stats.averageTenureYears) }}</span>
          <span class="stat-label">Avg Tenure</span>
        </div>
        <div v-if="detail.stats.managerCount > 0" class="stat-card managers">
          <span class="stat-icon">👔</span>
          <span class="stat-value">{{ detail.stats.managerCount }}</span>
          <span class="stat-label">Managers</span>
        </div>
      </div>

      <!-- Tier Distribution -->
      <div v-if="showTierChart" class="tier-section">
        <h3 class="section-title">Tier Distribution</h3>
        <div class="tier-chart">
          <div 
            v-for="(count, tier) in detail.stats.tierDistribution" 
            :key="tier as string"
            class="tier-bar-group"
          >
            <div class="tier-info">
              <span class="tier-name">{{ tier }}</span>
              <span class="tier-count">{{ count }}</span>
            </div>
            <div class="tier-bar-track">
              <div 
                class="tier-bar"
                :style="{ 
                  width: `${getTierPercent(count as number)}%`,
                  backgroundColor: getTierColor(tier as string)
                }"
              />
            </div>
          </div>
        </div>
      </div>

      <!-- Metadata Details -->
      <div class="details-section">
        <h3 class="section-title">Details</h3>
        <div class="detail-list">
          <div v-if="detail.metadata.siteName" class="detail-item">
            <span class="detail-label">Site</span>
            <span class="detail-value">{{ detail.metadata.siteName }}</span>
          </div>
          <div v-if="detail.metadata.teamName" class="detail-item">
            <span class="detail-label">Team</span>
            <span class="detail-value">{{ detail.metadata.teamName }}</span>
          </div>
          <div v-if="detail.metadata.title" class="detail-item">
            <span class="detail-label">Title</span>
            <span class="detail-value">{{ detail.metadata.title }}</span>
          </div>
          <div v-if="detail.metadata.email" class="detail-item">
            <span class="detail-label">Email</span>
            <a :href="`mailto:${detail.metadata.email}`" class="detail-value link">
              {{ detail.metadata.email }}
            </a>
          </div>
          <div v-if="detail.metadata.phone" class="detail-item">
            <span class="detail-label">Phone</span>
            <a :href="`tel:${detail.metadata.phone}`" class="detail-value link">
              {{ detail.metadata.phone }}
            </a>
          </div>
          <div v-if="detail.metadata.commissionTier && detail.metadata.commissionTier !== 'None'" class="detail-item">
            <span class="detail-label">Commission Tier</span>
            <span class="detail-value tier" :class="tierClass">
              {{ formatTier(detail.metadata.commissionTier) }}
            </span>
          </div>
          <div v-if="detail.metadata.agentStatus" class="detail-item">
            <span class="detail-label">Status</span>
            <span class="detail-value status" :class="detail.metadata.agentStatus.toLowerCase()">
              {{ detail.metadata.agentStatus }}
            </span>
          </div>
          <div v-if="detail.metadata.startDate" class="detail-item">
            <span class="detail-label">Start Date</span>
            <span class="detail-value">{{ formatDate(detail.metadata.startDate) }}</span>
          </div>
          <div v-if="detail.metadata.tenureYears != null" class="detail-item">
            <span class="detail-label">Tenure</span>
            <span class="detail-value">{{ formatFullTenure(detail.metadata.tenureYears) }}</span>
          </div>
        </div>
      </div>

      <!-- Direct Reports / Peers Preview -->
      <div v-if="hasPeople" class="people-section">
        <h3 class="section-title">
          {{ isPersonNode ? 'Peers' : 'Team Members' }}
          <span class="count">({{ peopleList.length }})</span>
        </h3>
        <div class="people-list">
          <div 
            v-for="person in peopleList.slice(0, 5)" 
            :key="person.id"
            class="person-item"
            @click="$emit('select', person.id)"
          >
            <div class="person-avatar" :style="{ backgroundColor: person.visuals.primaryColor }">
              {{ getInitials(person.name) }}
            </div>
            <div class="person-info">
              <span class="person-name">{{ person.name }}</span>
              <span class="person-type">{{ person.nodeType }}</span>
            </div>
            <div 
              class="person-status"
              :class="person.visuals.statusIndicator"
            />
          </div>
        </div>
        <button 
          v-if="peopleList.length > 5" 
          class="view-all-btn"
          @click="$emit('drillDown', detail.id)"
        >
          View all {{ peopleList.length }} members →
        </button>
      </div>

      <!-- Manager -->
      <div v-if="detail.manager" class="manager-section">
        <h3 class="section-title">Reports To</h3>
        <div 
          class="person-item manager"
          @click="$emit('select', detail.manager!.id)"
        >
          <div class="person-avatar" :style="{ backgroundColor: detail.manager.visuals.primaryColor }">
            {{ getInitials(detail.manager.name) }}
          </div>
          <div class="person-info">
            <span class="person-name">{{ detail.manager.name }}</span>
            <span class="person-type">Manager</span>
          </div>
        </div>
      </div>

      <!-- Actions -->
      <div class="actions-section">
        <button class="action-btn primary" @click="$emit('drillDown', detail.id)">
          <span class="icon">📊</span>
          View Full Details
        </button>
        <button v-if="isPersonNode && detail.metadata.email" class="action-btn secondary" @click="sendEmail">
          <span class="icon">📧</span>
          Send Email
        </button>
      </div>
    </div>
  </Transition>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import type { GraphNodeDetail } from '@/types/graph';
import { TIER_COLORS } from '@/types/graph';

const props = defineProps<{
  detail: GraphNodeDetail | null;
}>();

const emit = defineEmits<{
  close: [];
  select: [id: string];
  drillDown: [id: string];
}>();

const isPersonNode = computed(() => 
  props.detail?.nodeType === 'Manager' || props.detail?.nodeType === 'Agent'
);

const peopleList = computed(() => 
  isPersonNode.value ? props.detail?.peers ?? [] : props.detail?.directReports ?? []
);

const hasPeople = computed(() => peopleList.value.length > 0);

const showTierChart = computed(() => 
  !isPersonNode.value && Object.keys(props.detail?.stats.tierDistribution ?? {}).length > 0
);

const initials = computed(() => {
  if (!props.detail) return '';
  return getInitials(props.detail.name);
});

const healthPercent = computed(() => 
  Math.round((props.detail?.visuals.healthScore ?? 0) * 100)
);

const healthClass = computed(() => {
  const score = props.detail?.visuals.healthScore ?? 0;
  if (score >= 0.9) return 'good';
  if (score >= 0.7) return 'warning';
  return 'critical';
});

const tierClass = computed(() => 
  (props.detail?.metadata.commissionTier ?? 'None').toLowerCase()
);

const headerStyle = computed(() => ({
  '--accent-color': props.detail?.visuals.primaryColor ?? '#6b7280',
  background: `linear-gradient(135deg, ${props.detail?.visuals.primaryColor}15 0%, ${props.detail?.visuals.primaryColor}05 100%)`
}));

const avatarStyle = computed(() => ({
  backgroundColor: props.detail?.visuals.primaryColor ?? '#6b7280'
}));

function getInitials(name: string): string {
  const parts = name.split(' ');
  if (parts.length >= 2) {
    return `${parts[0][0]}${parts[parts.length - 1][0]}`.toUpperCase();
  }
  return name.substring(0, 2).toUpperCase();
}

function formatTenure(years: number): string {
  if (years < 1) {
    return `${Math.round(years * 12)}mo`;
  }
  return `${years.toFixed(1)}yr`;
}

function formatFullTenure(years: number): string {
  if (years < 1) {
    const months = Math.round(years * 12);
    return `${months} month${months !== 1 ? 's' : ''}`;
  }
  const y = Math.floor(years);
  const m = Math.round((years - y) * 12);
  if (m === 0) {
    return `${y} year${y !== 1 ? 's' : ''}`;
  }
  return `${y} year${y !== 1 ? 's' : ''}, ${m} month${m !== 1 ? 's' : ''}`;
}

function formatTier(tier: string): string {
  return tier.replace('Tier', 'Tier ');
}

function formatDate(dateStr: string): string {
  return new Date(dateStr).toLocaleDateString('en-US', {
    year: 'numeric',
    month: 'short',
    day: 'numeric'
  });
}

function getTierPercent(count: number): number {
  if (!props.detail) return 0;
  const total = Object.values(props.detail.stats.tierDistribution).reduce((a, b) => a + b, 0);
  return total > 0 ? (count / total) * 100 : 0;
}

function getTierColor(tier: string): string {
  return TIER_COLORS[tier] ?? '#6b7280';
}

function sendEmail() {
  if (props.detail?.metadata.email) {
    window.location.href = `mailto:${props.detail.metadata.email}`;
  }
}
</script>

<style scoped>
.graph-detail-panel {
  position: fixed;
  top: 0;
  right: 0;
  width: 420px;
  max-width: 100vw;
  height: 100vh;
  background: white;
  border-left: 1px solid #e5e7eb;
  box-shadow: -8px 0 32px rgba(0, 0, 0, 0.1);
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
  position: relative;
  padding: 24px;
  border-bottom: 1px solid #e5e7eb;
}

.close-btn {
  position: absolute;
  top: 16px;
  right: 16px;
  width: 32px;
  height: 32px;
  display: flex;
  align-items: center;
  justify-content: center;
  background: white;
  border: 1px solid #d1d5db;
  border-radius: 8px;
  cursor: pointer;
  font-size: 20px;
  color: #6b7280;
  transition: all 0.15s ease;
  z-index: 1;
}

.close-btn:hover {
  background: #f3f4f6;
  border-color: #9ca3af;
}

.header-content {
  display: flex;
  flex-direction: column;
  align-items: center;
  text-align: center;
}

.avatar {
  width: 72px;
  height: 72px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 24px;
  font-weight: 600;
  color: white;
  margin-bottom: 12px;
}

.name {
  font-size: 1.35rem;
  font-weight: 700;
  color: #1f2937;
  margin: 0 0 4px 0;
}

.type {
  font-size: 0.875rem;
  color: #6b7280;
  margin: 0;
}

/* Health Ring */
.health-ring {
  position: absolute;
  top: 24px;
  left: 24px;
  width: 48px;
  height: 48px;
}

.circular-chart {
  width: 100%;
  height: 100%;
}

.circle-bg {
  fill: none;
  stroke: #e5e7eb;
  stroke-width: 3;
}

.circle {
  fill: none;
  stroke-width: 3;
  stroke-linecap: round;
  animation: progress 1s ease-out forwards;
}

.health-ring.good .circle { stroke: #22c55e; }
.health-ring.warning .circle { stroke: #f59e0b; }
.health-ring.critical .circle { stroke: #ef4444; }

@keyframes progress {
  0% { stroke-dasharray: 0 100; }
}

.health-value {
  position: absolute;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%);
  font-size: 0.65rem;
  font-weight: 700;
  color: #4b5563;
}

/* Tags */
.tags-section {
  display: flex;
  flex-wrap: wrap;
  gap: 6px;
  padding: 16px 24px;
  border-bottom: 1px solid #e5e7eb;
}

.tag-pill {
  padding: 4px 10px;
  border-radius: 12px;
  font-size: 0.75rem;
  font-weight: 600;
}

/* Stats */
.stats-section {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 12px;
  padding: 20px 24px;
  border-bottom: 1px solid #e5e7eb;
}

.stat-card {
  display: flex;
  flex-direction: column;
  align-items: center;
  padding: 16px;
  background: #f9fafb;
  border-radius: 12px;
  gap: 4px;
}

.stat-icon {
  font-size: 20px;
  margin-bottom: 4px;
}

.stat-value {
  font-size: 1.25rem;
  font-weight: 700;
  color: #1f2937;
}

.stat-label {
  font-size: 0.7rem;
  color: #6b7280;
  text-transform: uppercase;
}

.stat-card.active { background: #dcfce7; }
.stat-card.training { background: #fef3c7; }

/* Tier Chart */
.tier-section {
  padding: 20px 24px;
  border-bottom: 1px solid #e5e7eb;
}

.section-title {
  font-size: 0.75rem;
  font-weight: 600;
  color: #6b7280;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  margin: 0 0 16px 0;
  display: flex;
  align-items: center;
  gap: 8px;
}

.section-title .count {
  color: #9ca3af;
}

.tier-chart {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.tier-bar-group {
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.tier-info {
  display: flex;
  justify-content: space-between;
  font-size: 0.8rem;
}

.tier-name {
  color: #4b5563;
}

.tier-count {
  font-weight: 600;
  color: #1f2937;
}

.tier-bar-track {
  height: 8px;
  background: #e5e7eb;
  border-radius: 4px;
  overflow: hidden;
}

.tier-bar {
  height: 100%;
  border-radius: 4px;
  transition: width 0.5s ease;
}

/* Details */
.details-section {
  padding: 20px 24px;
  border-bottom: 1px solid #e5e7eb;
}

.detail-list {
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
  font-size: 0.7rem;
  color: #9ca3af;
  text-transform: uppercase;
}

.detail-value {
  font-size: 0.85rem;
  color: #1f2937;
  font-weight: 500;
}

.detail-value.link {
  color: #3b82f6;
  text-decoration: none;
}

.detail-value.link:hover {
  text-decoration: underline;
}

.detail-value.tier,
.detail-value.status {
  padding: 3px 8px;
  border-radius: 4px;
  width: fit-content;
  font-size: 0.75rem;
}

.detail-value.tier.tier1 { background: #dcfce7; color: #166534; }
.detail-value.tier.tier2 { background: #dbeafe; color: #1e40af; }
.detail-value.tier.tier3 { background: #f3e8ff; color: #6b21a8; }

.detail-value.status.active { background: #dcfce7; color: #166534; }
.detail-value.status.inactive { background: #f3f4f6; color: #6b7280; }

/* People Section */
.people-section,
.manager-section {
  padding: 20px 24px;
  border-bottom: 1px solid #e5e7eb;
}

.people-list {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.person-item {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 10px 12px;
  border-radius: 8px;
  cursor: pointer;
  transition: all 0.15s ease;
}

.person-item:hover {
  background: #f3f4f6;
}

.person-item.manager {
  background: #f3e8ff;
}

.person-avatar {
  width: 36px;
  height: 36px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 12px;
  font-weight: 600;
  color: white;
  flex-shrink: 0;
}

.person-info {
  flex: 1;
  min-width: 0;
}

.person-name {
  display: block;
  font-size: 0.85rem;
  font-weight: 500;
  color: #1f2937;
}

.person-type {
  font-size: 0.7rem;
  color: #6b7280;
}

.person-status {
  width: 8px;
  height: 8px;
  border-radius: 50%;
  flex-shrink: 0;
}

.person-status.active,
.person-status.healthy { background: #22c55e; }
.person-status.warning,
.person-status.training { background: #f59e0b; }
.person-status.inactive,
.person-status.critical { background: #6b7280; }

.view-all-btn {
  display: block;
  width: 100%;
  padding: 12px;
  margin-top: 12px;
  background: #f9fafb;
  border: 1px solid #e5e7eb;
  border-radius: 8px;
  color: #3b82f6;
  font-size: 0.85rem;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.15s ease;
}

.view-all-btn:hover {
  background: #eff6ff;
  border-color: #3b82f6;
}

/* Actions */
.actions-section {
  padding: 20px 24px;
  margin-top: auto;
  background: #f9fafb;
  display: flex;
  gap: 12px;
}

.action-btn {
  flex: 1;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  padding: 14px 16px;
  border-radius: 10px;
  font-size: 0.875rem;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.15s ease;
}

.action-btn.primary {
  background: var(--accent-color, #3b82f6);
  color: white;
  border: none;
}

.action-btn.primary:hover {
  filter: brightness(0.9);
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

.action-btn .icon {
  font-size: 1rem;
}

/* Responsive */
@media (max-width: 500px) {
  .graph-detail-panel {
    width: 100vw;
  }
  
  .detail-list {
    grid-template-columns: 1fr;
  }
}
</style>
