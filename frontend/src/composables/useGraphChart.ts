/**
 * Enhanced composable for org chart with CQRS and real-time updates
 * Uses SignalR for live synchronization and optimized graph traversal
 */

import { ref, computed, watch, onMounted, onUnmounted } from 'vue';
import { useRouter, useRoute } from 'vue-router';
import { fetchHierarchy, fetchNodeChildren, fetchNodeDetail, searchGraphNodes, fetchStats } from '@/api/graph';
import { fetchFilterOptions } from '@/api/orgChart';
import { globalSignalR } from './useSignalR';
import type { 
  GraphNodeProjection, 
  GraphNodeDetail, 
  GraphQuery,
  GraphNodeStats 
} from '@/types/graph';
import type { OrgChartFilterOptions } from '@/types/orgChart';

export function useGraphChart() {
  const router = useRouter();
  const route = useRoute();

  // Core state
  const nodes = ref<Map<string, GraphNodeProjection>>(new Map());
  const rootNodes = ref<GraphNodeProjection[]>([]);
  const selectedDetail = ref<GraphNodeDetail | null>(null);
  const filterOptions = ref<OrgChartFilterOptions | null>(null);
  const globalStats = ref<GraphNodeStats | null>(null);
  
  // Loading states
  const loading = ref(false);
  const loadingDetail = ref(false);
  const loadingChildren = ref<Set<string>>(new Set());
  const error = ref<Error | null>(null);

  // Query state
  const query = ref<GraphQuery>({});

  // UI state
  const expandedIds = ref<Set<string>>(new Set());
  const selectedId = ref<string | null>(null);
  const focusPath = ref<string[]>([]); // Breadcrumb path for drill-down

  // SignalR connection
  const { 
    isConnected,
    connect: connectSignalR,
    subscribeToAll,
    onNodeUpdated,
    onNodeDeleted,
    onStatsUpdated
  } = globalSignalR;

  // Computed
  const selectedNode = computed(() => 
    selectedId.value ? nodes.value.get(selectedId.value) : null
  );

  const totalAgentCount = computed(() => 
    globalStats.value?.activeAgents ?? 0
  );

  const siteCount = computed(() => 
    rootNodes.value.filter(n => n.nodeType === 'Site').length
  );

  // Methods
  async function loadHierarchy() {
    loading.value = true;
    error.value = null;

    try {
      const hierarchy = await fetchHierarchy(query.value);
      
      // Update node map
      hierarchy.forEach(node => {
        nodes.value.set(node.id, node);
      });

      // Set root nodes (sites)
      rootNodes.value = hierarchy.filter(n => n.nodeType === 'Site');

      // Auto-expand sites
      rootNodes.value.forEach(site => {
        expandedIds.value.add(site.id);
      });

      // Load global stats
      globalStats.value = await fetchStats();
    } catch (e) {
      error.value = e as Error;
      console.error('Failed to load hierarchy:', e);
    } finally {
      loading.value = false;
    }
  }

  async function loadChildren(nodeId: string) {
    if (loadingChildren.value.has(nodeId)) return;
    
    loadingChildren.value.add(nodeId);
    
    try {
      const children = await fetchNodeChildren(nodeId, query.value);
      
      // Add children to node map
      children.forEach(child => {
        nodes.value.set(child.id, child);
      });

      // Update parent's child references
      const parent = nodes.value.get(nodeId);
      if (parent) {
        parent.childCount = children.length;
      }

      return children;
    } catch (e) {
      console.error('Failed to load children:', e);
      return [];
    } finally {
      loadingChildren.value.delete(nodeId);
    }
  }

  async function loadDetail(nodeId: string) {
    loadingDetail.value = true;
    
    try {
      selectedDetail.value = await fetchNodeDetail(nodeId);
      
      if (selectedDetail.value) {
        // Update focus path from breadcrumbs
        focusPath.value = selectedDetail.value.breadcrumbs.map(b => b.id);
      }
    } catch (e) {
      console.error('Failed to load detail:', e);
      selectedDetail.value = null;
    } finally {
      loadingDetail.value = false;
    }
  }

  async function loadFilterOptions() {
    try {
      filterOptions.value = await fetchFilterOptions();
    } catch (e) {
      console.error('Failed to load filter options:', e);
    }
  }

  function toggleNode(nodeId: string) {
    const isExpanded = expandedIds.value.has(nodeId);
    
    if (isExpanded) {
      expandedIds.value.delete(nodeId);
    } else {
      expandedIds.value.add(nodeId);
      
      // Load children if not already loaded
      const node = nodes.value.get(nodeId);
      if (node?.hasChildren && !hasLoadedChildren(nodeId)) {
        loadChildren(nodeId);
      }
    }
    
    // Trigger reactivity
    expandedIds.value = new Set(expandedIds.value);
  }

  function hasLoadedChildren(nodeId: string): boolean {
    // Check if any node has this as parent
    for (const [id, node] of nodes.value) {
      if (node.parentId === nodeId) return true;
    }
    return false;
  }

  function getChildren(nodeId: string): GraphNodeProjection[] {
    const children: GraphNodeProjection[] = [];
    for (const [id, node] of nodes.value) {
      if (node.parentId === nodeId) {
        children.push(node);
      }
    }
    return children.sort((a, b) => a.name.localeCompare(b.name));
  }

  async function selectNode(nodeId: string | null) {
    selectedId.value = nodeId;
    
    if (nodeId) {
      await loadDetail(nodeId);
      
      // Update URL for deep linking
      updateRouteQuery({ node: nodeId });
    } else {
      selectedDetail.value = null;
      updateRouteQuery({ node: undefined });
    }
  }

  async function drillDown(nodeId: string) {
    // Navigate to nested route for drill-down view
    const node = nodes.value.get(nodeId);
    if (!node) return;

    focusPath.value.push(nodeId);
    
    // Update route
    if (node.nodeType === 'Site') {
      router.push({ name: 'org-chart-site', params: { siteId: node.id } });
    } else if (node.nodeType === 'Team') {
      router.push({ name: 'org-chart-team', params: { teamId: node.id } });
    } else {
      router.push({ name: 'org-chart-person', params: { personId: node.id } });
    }
  }

  function drillUp() {
    if (focusPath.value.length > 0) {
      focusPath.value.pop();
      const parentId = focusPath.value[focusPath.value.length - 1];
      
      if (parentId) {
        selectNode(parentId);
      } else {
        router.push({ name: 'org-chart' });
      }
    }
  }

  function expandAll() {
    nodes.value.forEach((node, id) => {
      if (node.hasChildren) {
        expandedIds.value.add(id);
      }
    });
    expandedIds.value = new Set(expandedIds.value);
  }

  function collapseAll() {
    // Keep only sites expanded
    expandedIds.value = new Set(
      rootNodes.value.map(n => n.id)
    );
  }

  function resetFilters() {
    query.value = {};
  }

  function updateRouteQuery(params: Record<string, string | undefined>) {
    const newQuery = { ...route.query };
    Object.entries(params).forEach(([key, value]) => {
      if (value === undefined) {
        delete newQuery[key];
      } else {
        newQuery[key] = value;
      }
    });
    router.replace({ query: newQuery });
  }

  // SignalR event handlers
  function setupSignalRHandlers() {
    onNodeUpdated.value = (updatedNode: GraphNodeProjection) => {
      nodes.value.set(updatedNode.id, updatedNode);
      
      // If this is the selected node, reload detail
      if (selectedId.value === updatedNode.id) {
        loadDetail(updatedNode.id);
      }
    };

    onNodeDeleted.value = (nodeId: string) => {
      nodes.value.delete(nodeId);
      expandedIds.value.delete(nodeId);
      
      if (selectedId.value === nodeId) {
        selectNode(null);
      }
    };

    onStatsUpdated.value = ({ nodeId, stats }) => {
      const node = nodes.value.get(nodeId);
      if (node) {
        node.stats = stats;
      }
      
      if (!nodeId) {
        globalStats.value = stats;
      }
    };
  }

  // Watch for query changes
  watch(query, async () => {
    await loadHierarchy();
  }, { deep: true });

  // Initialize from route
  function initFromRoute() {
    const nodeParam = route.query.node as string | undefined;
    if (nodeParam) {
      selectedId.value = nodeParam;
      loadDetail(nodeParam);
    }
  }

  // Lifecycle
  onMounted(async () => {
    setupSignalRHandlers();
    
    // Connect SignalR
    await connectSignalR();
    if (isConnected.value) {
      await subscribeToAll();
    }
    
    await loadFilterOptions();
    await loadHierarchy();
    initFromRoute();
  });

  return {
    // State
    nodes,
    rootNodes,
    selectedDetail,
    filterOptions,
    globalStats,
    loading,
    loadingDetail,
    loadingChildren,
    error,
    query,
    expandedIds,
    selectedId,
    focusPath,
    isConnected,

    // Computed
    selectedNode,
    totalAgentCount,
    siteCount,

    // Methods
    loadHierarchy,
    loadChildren,
    loadDetail,
    loadFilterOptions,
    toggleNode,
    getChildren,
    selectNode,
    drillDown,
    drillUp,
    expandAll,
    collapseAll,
    resetFilters,
    hasLoadedChildren
  };
}

// Export singleton for global state
const globalGraphChart = useGraphChart;
export { globalGraphChart };
