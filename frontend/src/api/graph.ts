/**
 * API client for CQRS Graph Traversal endpoints
 * Optimized for fast reads with sub-second latency
 */

import type {
  GraphNodeProjection,
  GraphNodeDetail,
  GraphQuery,
  GraphNodeStats
} from '@/types/graph';
import { mockHierarchy, mockReportData, isDemoMode } from './mockData';

const baseUrl = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000';

/**
 * Fetches the hierarchy overview - optimized for initial load
 */
export async function fetchHierarchy(params: GraphQuery = {}): Promise<GraphNodeProjection[]> {
  // Return mock data if no API configured (demo mode)
  if (isDemoMode()) {
    console.log('🏢 Demo mode: Using mock hierarchy data');
    return Promise.resolve(mockHierarchy);
  }

  try {
    const search = new URLSearchParams();
    
    if (params.siteId != null) search.set('siteId', String(params.siteId));
    if (params.teamId != null) search.set('teamId', String(params.teamId));
    if (params.managerId) search.set('managerId', params.managerId);
    if (params.includeInactive) search.set('includeInactive', 'true');
    if (params.shallowLoad) search.set('shallowLoad', 'true');
    if (params.searchTerm) search.set('search', params.searchTerm);

    const queryString = search.toString();
    const url = `${baseUrl}/org-chart/graph/hierarchy${queryString ? `?${queryString}` : ''}`;

    const res = await fetch(url, { headers: getHeaders() });
    
    if (!res.ok) {
      throw new Error(`Failed to fetch hierarchy: ${res.statusText}`);
    }

    return res.json();
  } catch (error) {
    console.warn('API unavailable, falling back to mock data:', error);
    return mockHierarchy;
  }
}

/**
 * Fetches children of a node - for progressive disclosure
 */
export async function fetchNodeChildren(
  nodeId: string, 
  params: GraphQuery = {}
): Promise<GraphNodeProjection[]> {
  // Return empty array in demo mode (simplified)
  if (isDemoMode()) {
    return Promise.resolve([]);
  }

  try {
    const search = new URLSearchParams();
    
    if (params.includeInactive) search.set('includeInactive', 'true');
    if (params.searchTerm) search.set('search', params.searchTerm);

    const queryString = search.toString();
    const url = `${baseUrl}/org-chart/graph/node/${nodeId}/children${queryString ? `?${queryString}` : ''}`;

    const res = await fetch(url, { headers: getHeaders() });
    
    if (!res.ok) {
      throw new Error(`Failed to fetch node children: ${res.statusText}`);
    }

    return res.json();
  } catch (error) {
    console.warn('API unavailable:', error);
    return [];
  }
}

/**
 * Fetches full details for a node - for detail panel
 */
export async function fetchNodeDetail(nodeId: string): Promise<GraphNodeDetail | null> {
  // Return null in demo mode (simplified)
  if (isDemoMode()) {
    return Promise.resolve(null);
  }

  try {
    const url = `${baseUrl}/org-chart/graph/node/${nodeId}`;

    const res = await fetch(url, { headers: getHeaders() });
    
    if (res.status === 404) {
      return null;
    }
    
    if (!res.ok) {
      throw new Error(`Failed to fetch node detail: ${res.statusText}`);
    }

    return res.json();
  } catch (error) {
    console.warn('API unavailable:', error);
    return null;
  }
}

/**
 * Search nodes by name or email
 */
export async function searchGraphNodes(
  query: string, 
  limit: number = 20
): Promise<GraphNodeProjection[]> {
  // Return filtered mock hierarchy in demo mode
  if (isDemoMode()) {
    const filtered = mockHierarchy.filter(n => 
      n.name.toLowerCase().includes(query.toLowerCase())
    );
    return Promise.resolve(filtered.slice(0, limit));
  }

  try {
    const search = new URLSearchParams();
    search.set('q', query);
    search.set('limit', String(limit));

    const url = `${baseUrl}/org-chart/graph/search?${search.toString()}`;

    const res = await fetch(url, { headers: getHeaders() });
    
    if (!res.ok) {
      throw new Error(`Failed to search nodes: ${res.statusText}`);
    }

    return res.json();
  } catch (error) {
    console.warn('API unavailable:', error);
    return [];
  }
}

/**
 * Fetches aggregated stats for a node or the entire org
 */
export async function fetchStats(nodeId?: string): Promise<GraphNodeStats> {
  // Return mock stats in demo mode
  if (isDemoMode()) {
    return Promise.resolve({
      activeAgents: mockReportData.summary.activeAgents,
      totalAgents: mockReportData.summary.totalAgents,
      trainingAgents: mockReportData.summary.trainingAgents,
      averageTenure: mockReportData.summary.averageTenure,
      tier1Count: mockReportData.tierDistribution.tier1,
      tier2Count: mockReportData.tierDistribution.tier2,
      tier3Count: mockReportData.tierDistribution.tier3
    });
  }

  try {
    const url = nodeId 
      ? `${baseUrl}/org-chart/graph/stats?nodeId=${nodeId}`
      : `${baseUrl}/org-chart/graph/stats`;

    const res = await fetch(url, { headers: getHeaders() });
    
    if (!res.ok) {
      throw new Error(`Failed to fetch stats: ${res.statusText}`);
    }

    return res.json();
  } catch (error) {
    console.warn('API unavailable:', error);
    return {
      activeAgents: 0,
      totalAgents: 0,
      trainingAgents: 0,
      averageTenure: 0,
      tier1Count: 0,
      tier2Count: 0,
      tier3Count: 0
    };
  }
}

function getHeaders(): HeadersInit {
  const headers: HeadersInit = {
    'Content-Type': 'application/json'
  };

  const token = localStorage.getItem('accessToken');
  if (token) {
    headers['Authorization'] = `Bearer ${token}`;
  }

  return headers;
}
