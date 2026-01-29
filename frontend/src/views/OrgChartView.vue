<template>
  <div class="org-chart-view">
    <!-- Nested route for drill-down views -->
    <RouterView v-if="$route.name !== 'org-chart'" />
    
    <template v-else>
      <!-- Header -->
      <header class="page-header">
        <div class="header-content">
          <div class="title-row">
            <h1 class="page-title">Organization Chart</h1>
            <!-- Connection Status -->
            <div class="connection-status" :class="{ connected: isConnected }">
              <span class="status-dot"></span>
              {{ isConnected ? 'Live' : 'Offline' }}
            </div>
          </div>
          <p class="page-subtitle">
            {{ totalAgentCount }} team members across {{ siteCount }} sites
          </p>
        </div>
        <div class="header-actions">
          <button class="action-btn" @click="expandAll">Expand All</button>
          <button class="action-btn" @click="collapseAll">Collapse All</button>
          <button class="action-btn primary" @click="refreshData">
            <span v-if="loading" class="btn-spinner"></span>
            Refresh
          </button>
        </div>
      </header>

      <!-- Filters -->
      <OrgChartFilters
        v-if="filterOptions"
        :options="filterOptions"
        v-model:site-id="params.siteId"
        v-model:team-id="params.teamId"
        v-model:manager-id="params.managerId"
        v-model:tag-ids="params.tagIds"
        v-model:include-inactive="params.includeInactive"
        v-model:search="params.search"
        @reset="resetFilters"
      />

      <!-- Loading State -->
      <div v-if="loading && rootNodes.length === 0" class="loading-state">
        <div class="spinner"></div>
        <span>Loading organization chart...</span>
      </div>

      <!-- Error State -->
      <div v-else-if="error" class="error-state">
        <span class="error-icon">⚠️</span>
        <p>{{ error.message }}</p>
        <button @click="loadHierarchy" class="retry-btn">Retry</button>
      </div>

      <!-- Empty State -->
      <div v-else-if="rootNodes.length === 0" class="empty-state">
        <span class="empty-icon">📊</span>
        <p>No organization data found</p>
      </div>

      <!-- Main Content Area -->
      <div v-else class="main-content">
        <!-- Org Chart Tree -->
        <div class="tree-section">
          <div class="tree-container">
            <!-- Site Cards -->
            <div 
              v-for="site in rootNodes" 
              :key="site.id" 
              class="site-group"
            >
              <GraphNodeCard
                :node="site"
                :expanded="expandedIds.has(site.id)"
                :selected="selectedId === site.id"
                :show-tier-chart="true"
                @click="handleNodeClick(site.id)"
                @toggle="toggleNode(site.id)"
                @drill="handleDrillDown(site.id)"
              />
              
              <!-- Team Children -->
              <div v-if="expandedIds.has(site.id)" class="children-container">
                <div 
                  v-for="team in getChildren(site.id)" 
                  :key="team.id"
                  class="team-group"
                >
                  <GraphNodeCard
                    :node="team"
                    :expanded="expandedIds.has(team.id)"
                    :selected="selectedId === team.id"
                    :is-loading="loadingChildren.has(team.id)"
                    @click="handleNodeClick(team.id)"
                    @toggle="toggleNode(team.id)"
                    @drill="handleDrillDown(team.id)"
                  />
                  
                  <!-- Agent Children -->
                  <div v-if="expandedIds.has(team.id)" class="children-container agents">
                    <GraphNodeCard
                      v-for="agent in getChildren(team.id)"
                      :key="agent.id"
                      :node="agent"
                      :expanded="false"
                      :selected="selectedId === agent.id"
                      :show-stats="false"
                      @click="handleNodeClick(agent.id)"
                      @drill="handleDrillDown(agent.id)"
                    />
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- Stats Sidebar (Right) -->
        <aside class="stats-sidebar">
          <!-- Global Stats Card -->
          <div v-if="globalStats" class="stats-card">
            <h3 class="stats-title">Organization Overview</h3>
            <div class="stats-grid">
              <div class="stat-item">
                <span class="stat-value">{{ globalStats.activeAgents }}</span>
                <span class="stat-label">Active Agents</span>
              </div>
              <div class="stat-item training">
                <span class="stat-value">{{ globalStats.trainingAgents }}</span>
                <span class="stat-label">In Training</span>
              </div>
              <div class="stat-item">
                <span class="stat-value">{{ globalStats.managerCount }}</span>
                <span class="stat-label">Managers</span>
              </div>
              <div class="stat-item">
                <span class="stat-value">{{ formatTenure(globalStats.averageTenureYears) }}</span>
                <span class="stat-label">Avg Tenure</span>
              </div>
            </div>
            
            <!-- Tier Distribution -->
            <div class="tier-breakdown">
              <h4 class="tier-title">Tier Distribution</h4>
              <div class="tier-bars">
                <div 
                  v-for="(count, tier) in globalStats.tierDistribution" 
                  :key="tier as string"
                  class="tier-row"
                >
                  <span class="tier-label">{{ tier }}</span>
                  <div class="tier-track">
                    <div 
                      class="tier-fill"
                      :style="{ 
                        width: `${getTierPercent(count as number)}%`,
                        backgroundColor: getTierColor(tier as string)
                      }"
                    ></div>
                  </div>
                  <span class="tier-count">{{ count }}</span>
                </div>
              </div>
            </div>
          </div>

          <!-- Quick Actions -->
          <div class="quick-actions">
            <h3 class="actions-title">Quick Actions</h3>
            <button class="quick-action-btn" @click="exportData">
              <span class="icon">📊</span>
              Export Report
            </button>
            <button class="quick-action-btn" @click="syncTags">
              <span class="icon">🔄</span>
              Sync Tags
            </button>
          </div>
        </aside>
      </div>

      <!-- Enhanced Detail Panel -->
      <GraphDetailPanel 
        :detail="selectedDetail" 
        @close="selectNode(null)"
        @select="handleNodeClick"
        @drill-down="handleDrillDown"
      />

      <!-- Backdrop for mobile -->
      <div v-if="selectedDetail" class="backdrop" @click="selectNode(null)" />
    </template>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, ref, watch } from "vue";
import { useRouter } from "vue-router";
import { useOrgChart } from "@/composables/useOrgChart";
import { fetchHierarchy, fetchNodeChildren, fetchNodeDetail, fetchStats } from "@/api/graph";
import { syncAllTags } from "@/api/orgChart";
import { globalSignalR } from "@/composables/useSignalR";
import OrgChartFilters from "@/components/org-chart/OrgChartFilters.vue";
import GraphNodeCard from "@/components/org-chart/GraphNodeCard.vue";
import GraphDetailPanel from "@/components/org-chart/GraphDetailPanel.vue";
import type { GraphNodeProjection, GraphNodeDetail, GraphNodeStats } from "@/types/graph";
import { TIER_COLORS } from "@/types/graph";

const router = useRouter();

// Use original composable for filter options
const {
  filterOptions,
  params,
  loadFilterOptions,
  resetFilters,
} = useOrgChart();

// Graph state
const rootNodes = ref<GraphNodeProjection[]>([]);
const nodeMap = ref<Map<string, GraphNodeProjection>>(new Map());
const selectedDetail = ref<GraphNodeDetail | null>(null);
const globalStats = ref<GraphNodeStats | null>(null);
const loading = ref(false);
const error = ref<Error | null>(null);
const expandedIds = ref<Set<string>>(new Set());
const selectedId = ref<string | null>(null);
const loadingChildren = ref<Set<string>>(new Set());

// SignalR
const { isConnected, connect, onNodeUpdated, onStatsUpdated } = globalSignalR;

// Computed
const siteCount = computed(() => rootNodes.value.length);
const totalAgentCount = computed(() => globalStats.value?.activeAgents ?? 0);

// Methods
async function loadHierarchy() {
  loading.value = true;
  error.value = null;
  
  try {
    const nodes = await fetchHierarchy({
      siteId: params.value.siteId,
      teamId: params.value.teamId,
      managerId: params.value.managerId,
      includeInactive: params.value.includeInactive,
      searchTerm: params.value.search
    });
    
    nodeMap.value.clear();
    nodes.forEach(n => nodeMap.value.set(n.id, n));
    rootNodes.value = nodes.filter(n => n.nodeType === 'Site');
    
    // Auto-expand sites
    rootNodes.value.forEach(site => expandedIds.value.add(site.id));
    
    // Load global stats
    globalStats.value = await fetchStats();
  } catch (e) {
    error.value = e as Error;
  } finally {
    loading.value = false;
  }
}

async function loadChildren(nodeId: string) {
  if (loadingChildren.value.has(nodeId)) return;
  
  loadingChildren.value.add(nodeId);
  
  try {
    const children = await fetchNodeChildren(nodeId, {
      includeInactive: params.value.includeInactive,
      searchTerm: params.value.search
    });
    
    children.forEach(c => nodeMap.value.set(c.id, c));
  } finally {
    loadingChildren.value.delete(nodeId);
  }
}

function getChildren(parentId: string): GraphNodeProjection[] {
  const children: GraphNodeProjection[] = [];
  nodeMap.value.forEach(node => {
    if (node.parentId === parentId) children.push(node);
  });
  return children.sort((a, b) => {
    // Managers first
    if (a.nodeType === 'Manager' && b.nodeType !== 'Manager') return -1;
    if (b.nodeType === 'Manager' && a.nodeType !== 'Manager') return 1;
    return a.name.localeCompare(b.name);
  });
}

function toggleNode(nodeId: string) {
  const isExpanded = expandedIds.value.has(nodeId);
  
  if (isExpanded) {
    expandedIds.value.delete(nodeId);
  } else {
    expandedIds.value.add(nodeId);
    
    const node = nodeMap.value.get(nodeId);
    if (node?.hasChildren && getChildren(nodeId).length === 0) {
      loadChildren(nodeId);
    }
  }
  
  expandedIds.value = new Set(expandedIds.value);
}

async function handleNodeClick(nodeId: string) {
  selectedId.value = nodeId;
  
  try {
    selectedDetail.value = await fetchNodeDetail(nodeId);
  } catch (e) {
    console.error('Failed to load detail:', e);
  }
}

function handleDrillDown(nodeId: string) {
  const node = nodeMap.value.get(nodeId);
  if (!node) return;

  if (node.nodeType === 'Site') {
    router.push({ name: 'org-chart-site', params: { siteId: nodeId } });
  } else if (node.nodeType === 'Team') {
    router.push({ name: 'org-chart-team', params: { teamId: nodeId } });
  } else {
    router.push({ name: 'org-chart-person', params: { personId: nodeId } });
  }
}

function selectNode(nodeId: string | null) {
  selectedId.value = nodeId;
  if (!nodeId) {
    selectedDetail.value = null;
  }
}

function expandAll() {
  nodeMap.value.forEach((node, id) => {
    if (node.hasChildren) expandedIds.value.add(id);
  });
  expandedIds.value = new Set(expandedIds.value);
}

function collapseAll() {
  expandedIds.value = new Set(rootNodes.value.map(n => n.id));
}

async function refreshData() {
  await loadHierarchy();
}

function exportData() {
  console.log('Export data...');
  // TODO: Implement export
}

async function syncTags() {
  try {
    await syncAllTags();
    await loadHierarchy();
  } catch (e) {
    console.error('Failed to sync tags:', e);
  }
}

function formatTenure(years: number): string {
  if (years < 1) return `${Math.round(years * 12)}mo`;
  return `${years.toFixed(1)}yr`;
}

function getTierPercent(count: number): number {
  if (!globalStats.value) return 0;
  const total = Object.values(globalStats.value.tierDistribution).reduce((a, b) => a + b, 0);
  return total > 0 ? (count / total) * 100 : 0;
}

function getTierColor(tier: string): string {
  return TIER_COLORS[tier] ?? '#6b7280';
}

// Watch params for filter changes
watch(params, loadHierarchy, { deep: true });

// SignalR handlers
onNodeUpdated.value = (node) => {
  nodeMap.value.set(node.id, node);
  if (rootNodes.value.some(n => n.id === node.id)) {
    rootNodes.value = [...rootNodes.value];
  }
};

onStatsUpdated.value = ({ stats }) => {
  globalStats.value = stats;
};

onMounted(async () => {
  await connect();
  await loadFilterOptions();
  await loadHierarchy();
});
</script>

<style scoped>
.org-chart-view {
  min-height: 100vh;
  background: #f9fafb;
  padding: 24px;
}

/* Header */
.page-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 24px;
  flex-wrap: wrap;
  gap: 16px;
}

.header-content {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.title-row {
  display: flex;
  align-items: center;
  gap: 12px;
}

.page-title {
  font-size: 1.75rem;
  font-weight: 700;
  color: #1f2937;
  margin: 0;
}

.connection-status {
  display: flex;
  align-items: center;
  gap: 6px;
  padding: 4px 10px;
  background: #fee2e2;
  border-radius: 12px;
  font-size: 0.7rem;
  font-weight: 600;
  color: #991b1b;
  text-transform: uppercase;
}

.connection-status.connected {
  background: #dcfce7;
  color: #166534;
}

.status-dot {
  width: 6px;
  height: 6px;
  border-radius: 50%;
  background: currentColor;
}

.page-subtitle {
  font-size: 0.9rem;
  color: #6b7280;
  margin: 0;
}

.header-actions {
  display: flex;
  gap: 8px;
}

.action-btn {
  display: flex;
  align-items: center;
  gap: 6px;
  padding: 8px 16px;
  background: white;
  border: 1px solid #d1d5db;
  border-radius: 8px;
  font-size: 0.875rem;
  color: #4b5563;
  cursor: pointer;
  transition: all 0.15s ease;
}

.action-btn:hover {
  background: #f3f4f6;
  border-color: #9ca3af;
}

.action-btn.primary {
  background: #3b82f6;
  border-color: #3b82f6;
  color: white;
}

.action-btn.primary:hover {
  background: #2563eb;
}

.btn-spinner {
  width: 14px;
  height: 14px;
  border: 2px solid rgba(255, 255, 255, 0.3);
  border-top-color: white;
  border-radius: 50%;
  animation: spin 0.8s linear infinite;
}

/* Main Content Layout */
.main-content {
  display: grid;
  grid-template-columns: 1fr 340px;
  gap: 24px;
  align-items: start;
}

.tree-section {
  min-width: 0;
}

/* Tree Container */
.tree-container {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.site-group {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.children-container {
  margin-left: 32px;
  padding-left: 20px;
  border-left: 2px solid #e5e7eb;
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.children-container.agents {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(320px, 1fr));
  gap: 8px;
  border-left: 2px dashed #e5e7eb;
}

.team-group {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

/* Stats Sidebar */
.stats-sidebar {
  display: flex;
  flex-direction: column;
  gap: 20px;
  position: sticky;
  top: 24px;
}

.stats-card {
  background: white;
  border-radius: 16px;
  padding: 24px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
}

.stats-title {
  font-size: 0.85rem;
  font-weight: 600;
  color: #6b7280;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  margin: 0 0 20px 0;
}

.stats-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 16px;
  margin-bottom: 24px;
}

.stat-item {
  display: flex;
  flex-direction: column;
  align-items: center;
  padding: 16px;
  background: #f9fafb;
  border-radius: 12px;
  gap: 4px;
}

.stat-item.training {
  background: #fef3c7;
}

.stat-value {
  font-size: 1.5rem;
  font-weight: 700;
  color: #1f2937;
}

.stat-label {
  font-size: 0.7rem;
  color: #6b7280;
  text-transform: uppercase;
}

/* Tier Breakdown */
.tier-breakdown {
  padding-top: 20px;
  border-top: 1px solid #e5e7eb;
}

.tier-title {
  font-size: 0.75rem;
  font-weight: 600;
  color: #9ca3af;
  text-transform: uppercase;
  margin: 0 0 16px 0;
}

.tier-bars {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.tier-row {
  display: flex;
  align-items: center;
  gap: 12px;
}

.tier-label {
  width: 50px;
  font-size: 0.75rem;
  color: #6b7280;
}

.tier-track {
  flex: 1;
  height: 10px;
  background: #e5e7eb;
  border-radius: 5px;
  overflow: hidden;
}

.tier-fill {
  height: 100%;
  border-radius: 5px;
  transition: width 0.5s ease;
}

.tier-count {
  width: 28px;
  text-align: right;
  font-size: 0.8rem;
  font-weight: 600;
  color: #4b5563;
}

/* Quick Actions */
.quick-actions {
  background: white;
  border-radius: 16px;
  padding: 24px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
}

.actions-title {
  font-size: 0.85rem;
  font-weight: 600;
  color: #6b7280;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  margin: 0 0 16px 0;
}

.quick-action-btn {
  display: flex;
  align-items: center;
  gap: 10px;
  width: 100%;
  padding: 14px 16px;
  margin-bottom: 10px;
  background: #f9fafb;
  border: 1px solid #e5e7eb;
  border-radius: 10px;
  font-size: 0.875rem;
  color: #4b5563;
  cursor: pointer;
  transition: all 0.15s ease;
}

.quick-action-btn:last-child {
  margin-bottom: 0;
}

.quick-action-btn:hover {
  background: #eff6ff;
  border-color: #3b82f6;
  color: #3b82f6;
}

.quick-action-btn .icon {
  font-size: 1.1rem;
}

/* States */
.loading-state,
.error-state,
.empty-state {
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
  to {
    transform: rotate(360deg);
  }
}

.error-icon,
.empty-icon {
  font-size: 48px;
}

.retry-btn {
  padding: 8px 24px;
  background: #3b82f6;
  color: white;
  border: none;
  border-radius: 6px;
  cursor: pointer;
}

.retry-btn:hover {
  background: #2563eb;
}

/* Backdrop */
.backdrop {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.3);
  z-index: 999;
}

@media (min-width: 500px) {
  .backdrop {
    display: none;
  }
}

/* Responsive */
@media (max-width: 1200px) {
  .main-content {
    grid-template-columns: 1fr;
  }
  
  .stats-sidebar {
    position: static;
    flex-direction: row;
    flex-wrap: wrap;
  }
  
  .stats-card,
  .quick-actions {
    flex: 1;
    min-width: 280px;
  }
}

@media (max-width: 768px) {
  .org-chart-view {
    padding: 16px;
  }

  .page-header {
    flex-direction: column;
    align-items: flex-start;
  }
  
  .children-container {
    margin-left: 16px;
    padding-left: 12px;
  }
  
  .children-container.agents {
    grid-template-columns: 1fr;
  }
}
</style>
