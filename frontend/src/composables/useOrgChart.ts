/**
 * Composable for org chart state management
 */

import { ref, computed, watch, type Ref } from "vue";
import { fetchOrgChart, fetchFilterOptions } from "@/api/orgChart";
import type {
  OrgChartNode,
  OrgChartFilterOptions,
  OrgChartQueryParams,
} from "@/types/orgChart";

export function useOrgChart() {
  // State
  const tree = ref<OrgChartNode | null>(null);
  const filterOptions = ref<OrgChartFilterOptions | null>(null);
  const loading = ref(false);
  const error = ref<Error | null>(null);

  // Filter parameters
  const params = ref<OrgChartQueryParams>({});

  // UI state
  const expandedIds = ref<Set<string>>(new Set(["root"]));
  const selectedId = ref<string | null>(null);

  // Computed
  const selectedNode = computed(() => {
    if (!selectedId.value || !tree.value) return null;
    return findNode(tree.value, selectedId.value);
  });

  const siteNodes = computed(() => {
    if (!tree.value) return [];
    return tree.value.children;
  });

  const totalAgentCount = computed(() => {
    if (!tree.value) return 0;
    return tree.value.totalReportCount;
  });

  // Methods
  async function loadTree() {
    loading.value = true;
    error.value = null;

    try {
      tree.value = await fetchOrgChart(params.value);
      // Auto-expand sites
      tree.value.children.forEach((site) => {
        expandedIds.value.add(site.id);
      });
    } catch (e) {
      error.value = e as Error;
      console.error("Failed to load org chart:", e);
    } finally {
      loading.value = false;
    }
  }

  async function loadFilterOptions() {
    try {
      filterOptions.value = await fetchFilterOptions();
    } catch (e) {
      error.value = e as Error;
      console.error("Failed to load filter options:", e);
    }
  }

  function toggleNode(id: string) {
    const next = new Set(expandedIds.value);
    if (next.has(id)) {
      next.delete(id);
    } else {
      next.add(id);
    }
    expandedIds.value = next;
  }

  function selectNode(id: string | null) {
    selectedId.value = id;
  }

  function expandAll() {
    if (!tree.value) return;
    const allIds = new Set<string>();
    collectAllIds(tree.value, allIds);
    expandedIds.value = allIds;
  }

  function collapseAll() {
    expandedIds.value = new Set(["root"]);
  }

  function resetFilters() {
    params.value = {};
  }

  // Watch for param changes and reload
  watch(params, loadTree, { deep: true });

  return {
    // State
    tree,
    filterOptions,
    params,
    loading,
    error,
    expandedIds,
    selectedId,

    // Computed
    selectedNode,
    siteNodes,
    totalAgentCount,

    // Methods
    loadTree,
    loadFilterOptions,
    toggleNode,
    selectNode,
    expandAll,
    collapseAll,
    resetFilters,
  };
}

// Helper functions
function findNode(node: OrgChartNode, id: string): OrgChartNode | null {
  if (node.id === id) return node;

  for (const child of node.children) {
    const found = findNode(child, id);
    if (found) return found;
  }

  return null;
}

function collectAllIds(node: OrgChartNode, ids: Set<string>) {
  ids.add(node.id);
  for (const child of node.children) {
    collectAllIds(child, ids);
  }
}

// Export singleton instance for global state
const globalOrgChart = useOrgChart();
export { globalOrgChart };
