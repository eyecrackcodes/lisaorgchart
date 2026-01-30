/**
 * API client for org chart operations
 */

import type {
  OrgChartNode,
  OrgChartFilterOptions,
  OrgChartQueryParams,
} from "@/types/orgChart";
import { mockFilterOptions, isDemoMode } from "./mockData";

const baseUrl = import.meta.env.VITE_API_BASE_URL || "http://localhost:5000";

/**
 * Fetches the org chart tree with optional filtering
 */
export async function fetchOrgChart(
  params: OrgChartQueryParams = {},
): Promise<OrgChartNode> {
  const search = new URLSearchParams();

  if (params.siteId != null) search.set("siteId", String(params.siteId));
  if (params.teamId != null) search.set("teamId", String(params.teamId));
  if (params.managerId) search.set("managerId", params.managerId);
  if (params.tagIds?.length) search.set("tagIds", params.tagIds.join(","));
  if (params.includeInactive) search.set("includeInactive", "true");
  if (params.depth != null) search.set("depth", String(params.depth));
  if (params.search) search.set("search", params.search);

  const queryString = search.toString();
  const url = `${baseUrl}/org-chart${queryString ? `?${queryString}` : ""}`;

  const res = await fetch(url, {
    headers: getHeaders(),
  });

  if (!res.ok) {
    throw new Error(`Failed to fetch org chart: ${res.statusText}`);
  }

  return res.json();
}

/**
 * Fetches a flat list of org chart nodes
 */
export async function fetchOrgChartFlat(
  params: OrgChartQueryParams = {},
): Promise<OrgChartNode[]> {
  const search = new URLSearchParams();

  if (params.siteId != null) search.set("siteId", String(params.siteId));
  if (params.teamId != null) search.set("teamId", String(params.teamId));
  if (params.managerId) search.set("managerId", params.managerId);
  if (params.includeInactive) search.set("includeInactive", "true");
  if (params.search) search.set("search", params.search);

  const queryString = search.toString();
  const url = `${baseUrl}/org-chart/flat${queryString ? `?${queryString}` : ""}`;

  const res = await fetch(url, {
    headers: getHeaders(),
  });

  if (!res.ok) {
    throw new Error(`Failed to fetch org chart: ${res.statusText}`);
  }

  return res.json();
}

/**
 * Fetches available filter options
 */
export async function fetchFilterOptions(): Promise<OrgChartFilterOptions> {
  // Return mock data if no API configured (demo mode)
  if (isDemoMode()) {
    console.log('🔧 Demo mode: Using mock filter options');
    return Promise.resolve(mockFilterOptions as OrgChartFilterOptions);
  }

  try {
    const res = await fetch(`${baseUrl}/org-chart/filter-options`, {
      headers: getHeaders(),
    });

    if (!res.ok) {
      throw new Error(`Failed to fetch filter options: ${res.statusText}`);
    }

    return res.json();
  } catch (error) {
    console.warn('API unavailable, falling back to mock data:', error);
    return mockFilterOptions as OrgChartFilterOptions;
  }
}

/**
 * Fetches a single person node
 */
export async function fetchPerson(userId: string): Promise<OrgChartNode> {
  const res = await fetch(`${baseUrl}/org-chart/person/${userId}`, {
    headers: getHeaders(),
  });

  if (!res.ok) {
    throw new Error(`Failed to fetch person: ${res.statusText}`);
  }

  return res.json();
}

/**
 * Triggers a full tag sync
 */
export async function syncAllTags(): Promise<void> {
  const res = await fetch(`${baseUrl}/org-chart/sync-tags`, {
    method: "POST",
    headers: getHeaders(),
  });

  if (!res.ok) {
    throw new Error(`Failed to sync tags: ${res.statusText}`);
  }
}

function getHeaders(): HeadersInit {
  const headers: HeadersInit = {
    "Content-Type": "application/json",
  };

  // Add auth token if available
  const token = localStorage.getItem("accessToken");
  if (token) {
    headers["Authorization"] = `Bearer ${token}`;
  }

  return headers;
}
