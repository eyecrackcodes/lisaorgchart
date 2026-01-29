<template>
  <div class="drill-view">
    <!-- Breadcrumbs -->
    <nav class="breadcrumbs">
      <button class="breadcrumb-btn" @click="navigateUp">
        <span class="icon">←</span>
        Back to Overview
      </button>
      <template v-if="detail">
        <span class="separator">/</span>
        <span 
          v-for="(crumb, index) in detail.breadcrumbs" 
          :key="crumb.id"
          class="crumb"
        >
          <button 
            v-if="index < detail.breadcrumbs.length - 1"
            class="crumb-link"
            @click="navigateTo(crumb.id)"
          >
            {{ crumb.name }}
          </button>
          <span v-else class="crumb-current">{{ crumb.name }}</span>
          <span v-if="index < detail.breadcrumbs.length - 1" class="separator">/</span>
        </span>
        <span class="separator">/</span>
        <span class="crumb-current">{{ detail.name }}</span>
      </template>
    </nav>

    <!-- Loading State -->
    <div v-if="loading" class="loading-container">
      <div class="spinner"></div>
      <span>Loading details...</span>
    </div>

    <!-- Content -->
    <div v-else-if="detail" class="drill-content">
      <!-- Header Section -->
      <div class="drill-header" :style="headerStyle">
        <div class="header-avatar" :style="avatarStyle">
          <span class="initials">{{ initials }}</span>
        </div>
        <div class="header-info">
          <h1 class="title">{{ detail.name }}</h1>
          <p class="subtitle">{{ detail.nodeType }}</p>
          
          <!-- Tags -->
          <div v-if="detail.tags.length > 0" class="header-tags">
            <span 
              v-for="tag in detail.tags" 
              :key="tag.id"
              class="tag-pill"
              :style="{ backgroundColor: `#${tag.hexColor}20`, color: `#${tag.hexColor}` }"
            >
              {{ tag.name }}
            </span>
          </div>
        </div>
        
        <!-- Quick Stats -->
        <div class="quick-stats">
          <div class="quick-stat">
            <span class="stat-value">{{ detail.stats.activeAgents }}</span>
            <span class="stat-label">Active</span>
          </div>
          <div class="quick-stat">
            <span class="stat-value">{{ formatTenure(detail.stats.averageTenureYears) }}</span>
            <span class="stat-label">Avg Tenure</span>
          </div>
          <div class="quick-stat health" :class="healthClass">
            <span class="stat-value">{{ healthPercent }}%</span>
            <span class="stat-label">Health</span>
          </div>
        </div>
      </div>

      <!-- Main Grid -->
      <div class="drill-grid">
        <!-- Left: Details & Metadata -->
        <div class="grid-section metadata-section">
          <h2 class="section-title">Details</h2>
          <div class="metadata-grid">
            <div v-if="detail.metadata.siteName" class="meta-item">
              <span class="meta-label">Site</span>
              <span class="meta-value">{{ detail.metadata.siteName }}</span>
            </div>
            <div v-if="detail.metadata.teamName" class="meta-item">
              <span class="meta-label">Team</span>
              <span class="meta-value">{{ detail.metadata.teamName }}</span>
            </div>
            <div v-if="detail.metadata.title" class="meta-item">
              <span class="meta-label">Title</span>
              <span class="meta-value">{{ detail.metadata.title }}</span>
            </div>
            <div v-if="detail.metadata.email" class="meta-item">
              <span class="meta-label">Email</span>
              <a :href="`mailto:${detail.metadata.email}`" class="meta-value email">
                {{ detail.metadata.email }}
              </a>
            </div>
            <div v-if="detail.metadata.phone" class="meta-item">
              <span class="meta-label">Phone</span>
              <a :href="`tel:${detail.metadata.phone}`" class="meta-value">
                {{ detail.metadata.phone }}
              </a>
            </div>
            <div v-if="detail.metadata.commissionTier && detail.metadata.commissionTier !== 'None'" class="meta-item">
              <span class="meta-label">Commission Tier</span>
              <span class="meta-value tier" :class="tierClass">
                {{ formatTier(detail.metadata.commissionTier) }}
              </span>
            </div>
            <div v-if="detail.metadata.agentStatus" class="meta-item">
              <span class="meta-label">Status</span>
              <span class="meta-value status" :class="detail.metadata.agentStatus.toLowerCase()">
                {{ detail.metadata.agentStatus }}
              </span>
            </div>
            <div v-if="detail.metadata.startDate" class="meta-item">
              <span class="meta-label">Start Date</span>
              <span class="meta-value">{{ formatDate(detail.metadata.startDate) }}</span>
            </div>
          </div>
        </div>

        <!-- Center: Tier Distribution -->
        <div class="grid-section stats-section">
          <h2 class="section-title">Team Composition</h2>
          
          <!-- Tier Distribution Chart -->
          <div class="tier-distribution">
            <div 
              v-for="(count, tier) in detail.stats.tierDistribution" 
              :key="tier as string"
              class="tier-row"
            >
              <span class="tier-label">{{ tier }}</span>
              <div class="tier-bar-container">
                <div 
                  class="tier-bar"
                  :style="{ 
                    width: `${getTierPercent(count as number)}%`,
                    backgroundColor: getTierColor(tier as string)
                  }"
                ></div>
              </div>
              <span class="tier-count">{{ count }}</span>
            </div>
          </div>

          <!-- Key Metrics -->
          <div class="key-metrics">
            <div class="metric">
              <div class="metric-icon">👥</div>
              <div class="metric-info">
                <span class="metric-value">{{ detail.totalDescendants }}</span>
                <span class="metric-label">Total Members</span>
              </div>
            </div>
            <div class="metric">
              <div class="metric-icon">📊</div>
              <div class="metric-info">
                <span class="metric-value">{{ detail.stats.managerCount }}</span>
                <span class="metric-label">Managers</span>
              </div>
            </div>
            <div class="metric">
              <div class="metric-icon">🎓</div>
              <div class="metric-info">
                <span class="metric-value">{{ detail.stats.trainingAgents }}</span>
                <span class="metric-label">In Training</span>
              </div>
            </div>
          </div>
        </div>

        <!-- Right: Direct Reports / Peers -->
        <div class="grid-section reports-section">
          <h2 class="section-title">
            {{ isPersonNode ? 'Peers' : 'Direct Reports' }}
            <span class="count">({{ reportsList.length }})</span>
          </h2>
          
          <div class="reports-list">
            <div 
              v-for="report in reportsList.slice(0, 8)" 
              :key="report.id"
              class="report-item"
              @click="navigateTo(report.id)"
            >
              <div class="report-avatar" :style="{ backgroundColor: report.visuals.primaryColor }">
                {{ getInitials(report.name) }}
              </div>
              <div class="report-info">
                <span class="report-name">{{ report.name }}</span>
                <span class="report-type">{{ report.nodeType }}</span>
              </div>
              <div 
                class="report-status"
                :class="report.visuals.statusIndicator"
              ></div>
            </div>
            
            <div v-if="reportsList.length > 8" class="more-reports">
              +{{ reportsList.length - 8 }} more
            </div>
          </div>

          <!-- Manager (for person nodes) -->
          <template v-if="isPersonNode && detail.manager">
            <h3 class="subsection-title">Reports To</h3>
            <div 
              class="report-item manager"
              @click="navigateTo(detail.manager!.id)"
            >
              <div class="report-avatar" :style="{ backgroundColor: detail.manager.visuals.primaryColor }">
                {{ getInitials(detail.manager.name) }}
              </div>
              <div class="report-info">
                <span class="report-name">{{ detail.manager.name }}</span>
                <span class="report-type">Manager</span>
              </div>
            </div>
          </template>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue';
import { useRouter, useRoute } from 'vue-router';
import { fetchNodeDetail } from '@/api/graph';
import type { GraphNodeDetail } from '@/types/graph';
import { TIER_COLORS } from '@/types/graph';

const props = defineProps<{
  siteId?: string;
  teamId?: string;
  personId?: string;
}>();

const router = useRouter();
const route = useRoute();

const detail = ref<GraphNodeDetail | null>(null);
const loading = ref(false);

const nodeId = computed(() => {
  if (props.personId) return props.personId;
  if (props.teamId) return `team-${props.teamId.replace('team-', '')}`;
  if (props.siteId) return `site-${props.siteId.replace('site-', '')}`;
  return null;
});

const isPersonNode = computed(() => 
  detail.value?.nodeType === 'Manager' || detail.value?.nodeType === 'Agent'
);

const reportsList = computed(() => 
  isPersonNode.value ? detail.value?.peers ?? [] : detail.value?.directReports ?? []
);

const initials = computed(() => {
  if (!detail.value) return '';
  return getInitials(detail.value.name);
});

const healthPercent = computed(() => 
  Math.round((detail.value?.visuals.healthScore ?? 0) * 100)
);

const healthClass = computed(() => {
  const score = detail.value?.visuals.healthScore ?? 0;
  if (score >= 0.9) return 'good';
  if (score >= 0.7) return 'warning';
  return 'critical';
});

const tierClass = computed(() => 
  (detail.value?.metadata.commissionTier ?? 'None').toLowerCase()
);

const headerStyle = computed(() => ({
  '--accent-color': detail.value?.visuals.primaryColor ?? '#6b7280'
}));

const avatarStyle = computed(() => ({
  backgroundColor: detail.value?.visuals.primaryColor ?? '#6b7280'
}));

async function loadDetail() {
  if (!nodeId.value) return;
  
  loading.value = true;
  try {
    detail.value = await fetchNodeDetail(nodeId.value);
  } catch (e) {
    console.error('Failed to load detail:', e);
  } finally {
    loading.value = false;
  }
}

function navigateUp() {
  router.push({ name: 'org-chart' });
}

function navigateTo(id: string) {
  if (id.startsWith('site-')) {
    router.push({ name: 'org-chart-site', params: { siteId: id } });
  } else if (id.startsWith('team-')) {
    router.push({ name: 'org-chart-team', params: { teamId: id } });
  } else if (id === 'root') {
    router.push({ name: 'org-chart' });
  } else {
    router.push({ name: 'org-chart-person', params: { personId: id } });
  }
}

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
  if (!detail.value) return 0;
  const total = Object.values(detail.value.stats.tierDistribution).reduce((a, b) => a + b, 0);
  return total > 0 ? (count / total) * 100 : 0;
}

function getTierColor(tier: string): string {
  return TIER_COLORS[tier] ?? '#6b7280';
}

watch(nodeId, loadDetail);

onMounted(() => {
  loadDetail();
});
</script>

<style scoped>
.drill-view {
  padding: 24px;
  background: #f9fafb;
  min-height: 100vh;
}

/* Breadcrumbs */
.breadcrumbs {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-bottom: 24px;
  flex-wrap: wrap;
}

.breadcrumb-btn {
  display: flex;
  align-items: center;
  gap: 6px;
  padding: 8px 12px;
  background: white;
  border: 1px solid #d1d5db;
  border-radius: 6px;
  cursor: pointer;
  font-size: 0.875rem;
  color: #4b5563;
  transition: all 0.15s ease;
}

.breadcrumb-btn:hover {
  background: #f3f4f6;
  border-color: #9ca3af;
}

.separator {
  color: #9ca3af;
}

.crumb-link {
  background: none;
  border: none;
  color: #3b82f6;
  cursor: pointer;
  font-size: 0.875rem;
}

.crumb-link:hover {
  text-decoration: underline;
}

.crumb-current {
  color: #6b7280;
  font-size: 0.875rem;
}

/* Loading */
.loading-container {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 64px;
  gap: 16px;
  color: #6b7280;
}

.spinner {
  width: 40px;
  height: 40px;
  border: 3px solid #e5e7eb;
  border-top-color: #3b82f6;
  border-radius: 50%;
  animation: spin 0.8s linear infinite;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

/* Header */
.drill-header {
  display: flex;
  align-items: center;
  gap: 24px;
  padding: 32px;
  background: white;
  border-radius: 16px;
  border-left: 6px solid var(--accent-color);
  box-shadow: 0 4px 24px rgba(0, 0, 0, 0.06);
  margin-bottom: 24px;
}

.header-avatar {
  width: 80px;
  height: 80px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 28px;
  font-weight: 600;
  color: white;
  flex-shrink: 0;
}

.header-info {
  flex: 1;
}

.title {
  font-size: 1.75rem;
  font-weight: 700;
  color: #1f2937;
  margin: 0 0 4px 0;
}

.subtitle {
  font-size: 0.9rem;
  color: #6b7280;
  margin: 0 0 12px 0;
}

.header-tags {
  display: flex;
  flex-wrap: wrap;
  gap: 6px;
}

.tag-pill {
  padding: 4px 10px;
  border-radius: 12px;
  font-size: 0.75rem;
  font-weight: 600;
}

.quick-stats {
  display: flex;
  gap: 24px;
}

.quick-stat {
  display: flex;
  flex-direction: column;
  align-items: center;
  padding: 16px 24px;
  background: #f9fafb;
  border-radius: 12px;
}

.quick-stat .stat-value {
  font-size: 1.5rem;
  font-weight: 700;
  color: #1f2937;
}

.quick-stat .stat-label {
  font-size: 0.75rem;
  color: #6b7280;
  text-transform: uppercase;
}

.quick-stat.health.good { background: #dcfce7; }
.quick-stat.health.good .stat-value { color: #166534; }
.quick-stat.health.warning { background: #fef3c7; }
.quick-stat.health.warning .stat-value { color: #92400e; }
.quick-stat.health.critical { background: #fee2e2; }
.quick-stat.health.critical .stat-value { color: #991b1b; }

/* Grid */
.drill-grid {
  display: grid;
  grid-template-columns: 1fr 1fr 1fr;
  gap: 24px;
}

.grid-section {
  background: white;
  border-radius: 16px;
  padding: 24px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.04);
}

.section-title {
  font-size: 0.85rem;
  font-weight: 600;
  color: #6b7280;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  margin: 0 0 20px 0;
  display: flex;
  align-items: center;
  gap: 8px;
}

.section-title .count {
  color: #9ca3af;
  font-weight: 500;
}

/* Metadata */
.metadata-grid {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.meta-item {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.meta-label {
  font-size: 0.7rem;
  color: #9ca3af;
  text-transform: uppercase;
  letter-spacing: 0.03em;
}

.meta-value {
  font-size: 0.9rem;
  color: #1f2937;
  font-weight: 500;
}

.meta-value.email {
  color: #3b82f6;
  text-decoration: none;
}

.meta-value.email:hover {
  text-decoration: underline;
}

.meta-value.tier {
  padding: 4px 8px;
  border-radius: 4px;
  width: fit-content;
}

.meta-value.tier.tier1 { background: #dcfce7; color: #166534; }
.meta-value.tier.tier2 { background: #dbeafe; color: #1e40af; }
.meta-value.tier.tier3 { background: #f3e8ff; color: #6b21a8; }

.meta-value.status {
  padding: 4px 8px;
  border-radius: 4px;
  width: fit-content;
  font-size: 0.75rem;
  text-transform: uppercase;
}

.meta-value.status.active { background: #dcfce7; color: #166534; }
.meta-value.status.inactive { background: #f3f4f6; color: #6b7280; }

/* Tier Distribution */
.tier-distribution {
  display: flex;
  flex-direction: column;
  gap: 12px;
  margin-bottom: 24px;
}

.tier-row {
  display: flex;
  align-items: center;
  gap: 12px;
}

.tier-label {
  width: 60px;
  font-size: 0.8rem;
  color: #6b7280;
}

.tier-bar-container {
  flex: 1;
  height: 12px;
  background: #e5e7eb;
  border-radius: 6px;
  overflow: hidden;
}

.tier-bar {
  height: 100%;
  border-radius: 6px;
  transition: width 0.3s ease;
}

.tier-count {
  width: 30px;
  text-align: right;
  font-size: 0.85rem;
  font-weight: 600;
  color: #4b5563;
}

/* Key Metrics */
.key-metrics {
  display: flex;
  gap: 16px;
}

.metric {
  flex: 1;
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 16px;
  background: #f9fafb;
  border-radius: 12px;
}

.metric-icon {
  font-size: 24px;
}

.metric-info {
  display: flex;
  flex-direction: column;
}

.metric-value {
  font-size: 1.25rem;
  font-weight: 700;
  color: #1f2937;
}

.metric-label {
  font-size: 0.7rem;
  color: #6b7280;
  text-transform: uppercase;
}

/* Reports List */
.reports-list {
  display: flex;
  flex-direction: column;
  gap: 8px;
  max-height: 400px;
  overflow-y: auto;
}

.report-item {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 12px;
  border-radius: 8px;
  cursor: pointer;
  transition: all 0.15s ease;
}

.report-item:hover {
  background: #f3f4f6;
}

.report-item.manager {
  background: #f3e8ff;
}

.report-avatar {
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

.report-info {
  flex: 1;
  min-width: 0;
}

.report-name {
  display: block;
  font-size: 0.875rem;
  font-weight: 500;
  color: #1f2937;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.report-type {
  font-size: 0.75rem;
  color: #6b7280;
}

.report-status {
  width: 8px;
  height: 8px;
  border-radius: 50%;
  flex-shrink: 0;
}

.report-status.active,
.report-status.healthy { background: #22c55e; }
.report-status.warning,
.report-status.training { background: #f59e0b; }
.report-status.inactive,
.report-status.critical { background: #6b7280; }

.more-reports {
  text-align: center;
  padding: 12px;
  color: #6b7280;
  font-size: 0.875rem;
}

.subsection-title {
  font-size: 0.75rem;
  color: #9ca3af;
  text-transform: uppercase;
  margin: 24px 0 12px 0;
}

/* Responsive */
@media (max-width: 1024px) {
  .drill-grid {
    grid-template-columns: 1fr;
  }
  
  .drill-header {
    flex-direction: column;
    text-align: center;
  }
  
  .quick-stats {
    width: 100%;
    justify-content: center;
  }
}
</style>
