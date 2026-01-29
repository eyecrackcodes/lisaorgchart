<template>
  <div
    class="graph-node-card"
    :class="[
      nodeTypeClass,
      statusClass,
      { 
        selected: selected, 
        'has-children': hasChildren,
        loading: isLoading 
      },
    ]"
    :style="cardStyle"
    @click.stop="$emit('click')"
  >
    <!-- Visual Health Indicator Bar -->
    <div 
      class="health-bar"
      :style="{ 
        width: `${healthPercent}%`,
        backgroundColor: visuals.primaryColor 
      }"
    />

    <!-- Expand/Collapse Toggle -->
    <button
      v-if="hasChildren"
      class="toggle-btn"
      @click.stop="$emit('toggle')"
      :aria-label="expanded ? 'Collapse' : 'Expand'"
    >
      <span class="toggle-icon" :class="{ rotated: expanded }">›</span>
    </button>
    <div v-else class="toggle-spacer"></div>

    <!-- Node Content -->
    <div class="node-content">
      <!-- Avatar with tier color ring -->
      <div class="avatar-container">
        <div 
          class="avatar" 
          :class="nodeTypeClass"
          :style="avatarStyle"
        >
          <span class="avatar-initials">{{ initials }}</span>
        </div>
        <!-- Status dot -->
        <div 
          v-if="statusDot" 
          class="status-dot"
          :class="statusDot"
        />
      </div>

      <!-- Info -->
      <div class="info">
        <div class="name-row">
          <span class="name">{{ node.name }}</span>
          <!-- Tenure badge -->
          <span v-if="visuals.tenureBadge" class="tenure-badge" :class="tenureBadgeClass">
            {{ visuals.tenureBadge }}
          </span>
          <!-- Performance badge -->
          <span v-if="visuals.performanceBadge" class="performance-badge">
            {{ visuals.performanceBadge }}
          </span>
        </div>

        <div class="meta-row">
          <span v-if="node.nodeType === 'Site'" class="meta-item location">
            <span class="icon">📍</span> {{ stats.activeAgents }} agents
          </span>
          <span v-else-if="node.nodeType === 'Team'" class="meta-item">
            <span class="icon">👥</span> {{ node.childCount }} members
          </span>
          <span v-else-if="isPersonNode" class="meta-item">
            <span class="icon">📊</span> {{ tierLabel }}
          </span>
        </div>

        <!-- Stats Summary -->
        <div v-if="showStats && (node.nodeType === 'Site' || node.nodeType === 'Team')" class="stats-row">
          <div class="stat-item">
            <span class="stat-value">{{ stats.activeAgents }}</span>
            <span class="stat-label">Active</span>
          </div>
          <div v-if="stats.trainingAgents > 0" class="stat-item training">
            <span class="stat-value">{{ stats.trainingAgents }}</span>
            <span class="stat-label">Training</span>
          </div>
          <div class="stat-item">
            <span class="stat-value">{{ formatTenure(stats.averageTenureYears) }}</span>
            <span class="stat-label">Avg Tenure</span>
          </div>
        </div>

        <!-- Tier distribution mini chart -->
        <div v-if="showTierChart" class="tier-chart">
          <div 
            v-for="(count, tier) in stats.tierDistribution" 
            :key="tier"
            class="tier-bar"
            :style="{ 
              width: `${getTierPercent(count)}%`,
              backgroundColor: getTierColor(tier as string)
            }"
            :title="`${tier}: ${count}`"
          />
        </div>
      </div>

      <!-- Counts Badge / Drill-down -->
      <div class="action-area">
        <div v-if="node.totalDescendants > 0" class="count-badge" :class="healthClass">
          {{ node.totalDescendants }}
        </div>
        <button 
          v-if="hasChildren" 
          class="drill-btn" 
          @click.stop="$emit('drill')"
          title="View details"
        >
          →
        </button>
      </div>
    </div>

    <!-- Loading overlay -->
    <div v-if="isLoading" class="loading-overlay">
      <div class="mini-spinner"></div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import type { GraphNodeProjection, GraphNodeStats, GraphNodeVisuals } from '@/types/graph';
import { TIER_COLORS, STATUS_COLORS } from '@/types/graph';

const props = withDefaults(defineProps<{
  node: GraphNodeProjection;
  expanded?: boolean;
  selected?: boolean;
  isLoading?: boolean;
  showStats?: boolean;
  showTierChart?: boolean;
}>(), {
  expanded: false,
  selected: false,
  isLoading: false,
  showStats: true,
  showTierChart: false
});

defineEmits<{
  click: [];
  toggle: [];
  drill: [];
}>();

const stats = computed(() => props.node.stats);
const visuals = computed(() => props.node.visuals);
const hasChildren = computed(() => props.node.hasChildren);

const nodeTypeClass = computed(() => props.node.nodeType.toLowerCase());

const isPersonNode = computed(
  () => props.node.nodeType === 'Manager' || props.node.nodeType === 'Agent'
);

const statusClass = computed(() => `status-${visuals.value.statusIndicator}`);

const healthPercent = computed(() => Math.round(visuals.value.healthScore * 100));

const healthClass = computed(() => {
  if (visuals.value.healthScore >= 0.9) return 'health-good';
  if (visuals.value.healthScore >= 0.7) return 'health-warning';
  return 'health-critical';
});

const statusDot = computed(() => {
  const indicator = visuals.value.statusIndicator;
  if (indicator === 'active' || indicator === 'healthy') return 'dot-active';
  if (indicator === 'training' || indicator === 'warning') return 'dot-warning';
  if (indicator === 'inactive' || indicator === 'critical') return 'dot-inactive';
  return null;
});

const cardStyle = computed(() => ({
  '--primary-color': visuals.value.primaryColor,
  '--secondary-color': visuals.value.secondaryColor,
  borderLeftColor: visuals.value.primaryColor
}));

const avatarStyle = computed(() => ({
  background: visuals.value.primaryColor
}));

const initials = computed(() => {
  const name = props.node.name;
  const parts = name.split(' ');
  if (parts.length >= 2) {
    return `${parts[0][0]}${parts[parts.length - 1][0]}`.toUpperCase();
  }
  return name.substring(0, 2).toUpperCase();
});

const tierLabel = computed(() => {
  const tier = Object.keys(stats.value.tierDistribution)[0];
  if (!tier || tier === 'None') return 'Standard';
  return tier.replace('Tier', 'Tier ');
});

const tenureBadgeClass = computed(() => {
  const badge = visuals.value.tenureBadge;
  if (badge === '5yr+') return 'tenure-veteran';
  if (badge === '2yr+') return 'tenure-experienced';
  if (badge === '1yr+') return 'tenure-established';
  if (badge === 'New') return 'tenure-new';
  return '';
});

function formatTenure(years: number): string {
  if (years < 1) {
    const months = Math.round(years * 12);
    return `${months}mo`;
  }
  return `${years.toFixed(1)}yr`;
}

function getTierPercent(count: number): number {
  const total = Object.values(stats.value.tierDistribution).reduce((a, b) => a + b, 0);
  return total > 0 ? (count / total) * 100 : 0;
}

function getTierColor(tier: string): string {
  return TIER_COLORS[tier] ?? '#6b7280';
}
</script>

<style scoped>
.graph-node-card {
  position: relative;
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 14px 16px;
  background: white;
  border: 1px solid #e5e7eb;
  border-left: 4px solid var(--primary-color, #6b7280);
  border-radius: 10px;
  cursor: pointer;
  transition: all 0.2s ease;
  overflow: hidden;
}

.graph-node-card:hover {
  border-color: var(--primary-color, #3b82f6);
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.08);
  transform: translateY(-1px);
}

.graph-node-card.selected {
  border-color: var(--primary-color, #3b82f6);
  background: var(--secondary-color, #eff6ff);
  box-shadow: 0 0 0 2px rgba(59, 130, 246, 0.2);
}

.graph-node-card.loading {
  pointer-events: none;
  opacity: 0.7;
}

/* Health bar at top */
.health-bar {
  position: absolute;
  top: 0;
  left: 0;
  height: 3px;
  transition: width 0.3s ease;
}

/* Node type styles */
.graph-node-card.site {
  background: linear-gradient(135deg, #f0f9ff 0%, #e0f2fe 100%);
}

.graph-node-card.team {
  background: linear-gradient(135deg, #f0fdf4 0%, #dcfce7 100%);
}

.graph-node-card.manager {
  background: linear-gradient(135deg, #faf5ff 0%, #f3e8ff 100%);
}

/* Toggle Button */
.toggle-btn {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 26px;
  height: 26px;
  border: 1px solid #d1d5db;
  border-radius: 6px;
  background: white;
  cursor: pointer;
  flex-shrink: 0;
  transition: all 0.2s ease;
}

.toggle-btn:hover {
  background: #f3f4f6;
  border-color: var(--primary-color);
}

.toggle-icon {
  font-size: 16px;
  font-weight: bold;
  color: #6b7280;
  transition: transform 0.2s ease;
}

.toggle-icon.rotated {
  transform: rotate(90deg);
}

.toggle-spacer {
  width: 26px;
  flex-shrink: 0;
}

/* Node Content */
.node-content {
  display: flex;
  align-items: center;
  gap: 14px;
  flex: 1;
  min-width: 0;
}

/* Avatar */
.avatar-container {
  position: relative;
  flex-shrink: 0;
}

.avatar {
  width: 44px;
  height: 44px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 15px;
  font-weight: 600;
  color: white;
}

.avatar.site,
.avatar.team {
  border-radius: 10px;
}

.status-dot {
  position: absolute;
  bottom: 0;
  right: 0;
  width: 12px;
  height: 12px;
  border-radius: 50%;
  border: 2px solid white;
}

.status-dot.dot-active {
  background: #22c55e;
}

.status-dot.dot-warning {
  background: #f59e0b;
}

.status-dot.dot-inactive {
  background: #6b7280;
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

.tenure-badge {
  font-size: 0.65rem;
  padding: 2px 6px;
  border-radius: 4px;
  font-weight: 600;
  text-transform: uppercase;
}

.tenure-badge.tenure-veteran {
  background: #fef3c7;
  color: #92400e;
}

.tenure-badge.tenure-experienced {
  background: #d1fae5;
  color: #065f46;
}

.tenure-badge.tenure-established {
  background: #e0f2fe;
  color: #0369a1;
}

.tenure-badge.tenure-new {
  background: #fce7f3;
  color: #9d174d;
}

.performance-badge {
  font-size: 0.65rem;
  padding: 2px 6px;
  background: #fef3c7;
  color: #92400e;
  border-radius: 4px;
  font-weight: 600;
  text-transform: uppercase;
}

.meta-row {
  display: flex;
  align-items: center;
  gap: 12px;
}

.meta-item {
  display: flex;
  align-items: center;
  gap: 4px;
  font-size: 0.8rem;
  color: #6b7280;
}

.meta-item .icon {
  font-size: 0.75rem;
}

/* Stats Row */
.stats-row {
  display: flex;
  gap: 16px;
  margin-top: 6px;
}

.stat-item {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 2px;
}

.stat-item.training {
  color: #f59e0b;
}

.stat-value {
  font-size: 0.9rem;
  font-weight: 700;
  color: #1f2937;
}

.stat-label {
  font-size: 0.65rem;
  color: #9ca3af;
  text-transform: uppercase;
}

/* Tier Chart */
.tier-chart {
  display: flex;
  height: 4px;
  border-radius: 2px;
  overflow: hidden;
  margin-top: 6px;
  background: #e5e7eb;
}

.tier-bar {
  height: 100%;
  transition: width 0.3s ease;
}

/* Action Area */
.action-area {
  display: flex;
  align-items: center;
  gap: 8px;
  flex-shrink: 0;
}

.count-badge {
  display: flex;
  align-items: center;
  justify-content: center;
  min-width: 30px;
  height: 30px;
  padding: 0 10px;
  background: #e5e7eb;
  border-radius: 15px;
  font-size: 0.8rem;
  font-weight: 700;
  color: #4b5563;
}

.count-badge.health-good {
  background: #dcfce7;
  color: #166534;
}

.count-badge.health-warning {
  background: #fef3c7;
  color: #92400e;
}

.count-badge.health-critical {
  background: #fee2e2;
  color: #991b1b;
}

.drill-btn {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 30px;
  height: 30px;
  border: 1px solid #d1d5db;
  border-radius: 6px;
  background: white;
  cursor: pointer;
  font-size: 14px;
  color: #6b7280;
  transition: all 0.15s ease;
}

.drill-btn:hover {
  background: var(--primary-color);
  border-color: var(--primary-color);
  color: white;
}

/* Loading Overlay */
.loading-overlay {
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  display: flex;
  align-items: center;
  justify-content: center;
  background: rgba(255, 255, 255, 0.7);
}

.mini-spinner {
  width: 20px;
  height: 20px;
  border: 2px solid #e5e7eb;
  border-top-color: var(--primary-color, #3b82f6);
  border-radius: 50%;
  animation: spin 0.8s linear infinite;
}

@keyframes spin {
  to {
    transform: rotate(360deg);
  }
}

/* Responsive */
@media (max-width: 640px) {
  .stats-row {
    display: none;
  }
  
  .tier-chart {
    display: none;
  }
}
</style>
