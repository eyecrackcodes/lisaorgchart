/**
 * API client for reporting endpoints
 */

import type { OrgReportData, OrgSummary, SiteComparison, TeamCompositionData } from '@/types/reports';
import { mockReportData, isDemoMode } from './mockData';

const baseUrl = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000';

/**
 * Fetches complete report data
 */
export async function fetchReportData(): Promise<OrgReportData> {
  // Return mock data if no API configured (demo mode)
  if (isDemoMode()) {
    console.log('📊 Demo mode: Using mock report data');
    return Promise.resolve(mockReportData);
  }

  try {
    const res = await fetch(`${baseUrl}/org-chart/reports`, {
      headers: getHeaders()
    });
    
    if (!res.ok) {
      throw new Error(`Failed to fetch report data: ${res.statusText}`);
    }

    return res.json();
  } catch (error) {
    console.warn('API unavailable, falling back to mock data:', error);
    return mockReportData;
  }
}

/**
 * Fetches organization summary
 */
export async function fetchOrgSummary(): Promise<OrgSummary> {
  const res = await fetch(`${baseUrl}/org-chart/reports/summary`, {
    headers: getHeaders()
  });
  
  if (!res.ok) {
    throw new Error(`Failed to fetch org summary: ${res.statusText}`);
  }

  return res.json();
}

/**
 * Fetches site comparison data
 */
export async function fetchSiteComparison(): Promise<SiteComparison[]> {
  const res = await fetch(`${baseUrl}/org-chart/reports/sites`, {
    headers: getHeaders()
  });
  
  if (!res.ok) {
    throw new Error(`Failed to fetch site comparison: ${res.statusText}`);
  }

  return res.json();
}

/**
 * Fetches team composition data
 */
export async function fetchTeamComposition(): Promise<TeamCompositionData[]> {
  const res = await fetch(`${baseUrl}/org-chart/reports/teams`, {
    headers: getHeaders()
  });
  
  if (!res.ok) {
    throw new Error(`Failed to fetch team composition: ${res.statusText}`);
  }

  return res.json();
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
