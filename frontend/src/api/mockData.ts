/**
 * Mock data for demo mode when API is unavailable
 */

import type { OrgReportData } from '@/types/reports';
import type { GraphNodeProjection, GraphNodeDetail } from '@/types/graph';

export const mockReportData: OrgReportData = {
  summary: {
    totalAgents: 42,
    activeAgents: 38,
    inactiveAgents: 4,
    trainingAgents: 8,
    totalManagers: 6,
    totalTeams: 8,
    totalSites: 2,
    averageTenure: 2.3,
    averageTeamSize: 5.25
  },
  siteComparison: [
    {
      siteId: "1",
      siteName: "Austin",
      totalAgents: 24,
      activeAgents: 22,
      trainingAgents: 5,
      teamCount: 4,
      averageTenure: 2.5,
      tier1Count: 8,
      tier2Count: 10,
      tier3Count: 6
    },
    {
      siteId: "2",
      siteName: "Charlotte",
      totalAgents: 18,
      activeAgents: 16,
      trainingAgents: 3,
      teamCount: 4,
      averageTenure: 2.1,
      tier1Count: 5,
      tier2Count: 8,
      tier3Count: 5
    }
  ],
  tierDistribution: {
    tier1: 13,
    tier2: 18,
    tier3: 11,
    none: 0,
    bySite: [
      { siteName: "Austin", tier1: 8, tier2: 10, tier3: 6 },
      { siteName: "Charlotte", tier1: 5, tier2: 8, tier3: 5 }
    ]
  },
  tenureDistribution: {
    ranges: [
      { label: "< 3 months", count: 4, percentage: 9.5 },
      { label: "3-6 months", count: 6, percentage: 14.3 },
      { label: "6-12 months", count: 8, percentage: 19.0 },
      { label: "1-2 years", count: 12, percentage: 28.6 },
      { label: "2-5 years", count: 9, percentage: 21.4 },
      { label: "5+ years", count: 3, percentage: 7.1 }
    ],
    averageByRole: [
      { role: "Managers", averageTenure: 4.2 },
      { role: "Agents", averageTenure: 1.8 }
    ]
  },
  teamComposition: [
    { teamId: "1", teamName: "Alpha Team", siteName: "Austin", managerName: "Sarah Johnson", memberCount: 6, activeCount: 6, trainingCount: 1, averageTenure: 2.8, tier1Percent: 33, tier2Percent: 50, tier3Percent: 17 },
    { teamId: "2", teamName: "Bravo Team", siteName: "Austin", managerName: "Mike Chen", memberCount: 6, activeCount: 5, trainingCount: 2, averageTenure: 1.9, tier1Percent: 33, tier2Percent: 33, tier3Percent: 34 },
    { teamId: "3", teamName: "Charlie Team", siteName: "Austin", managerName: "Lisa Park", memberCount: 6, activeCount: 6, trainingCount: 1, averageTenure: 2.4, tier1Percent: 50, tier2Percent: 33, tier3Percent: 17 },
    { teamId: "4", teamName: "Delta Team", siteName: "Austin", managerName: "Tom Wilson", memberCount: 6, activeCount: 5, trainingCount: 1, averageTenure: 2.7, tier1Percent: 17, tier2Percent: 50, tier3Percent: 33 },
    { teamId: "5", teamName: "Echo Team", siteName: "Charlotte", managerName: "Amy Roberts", memberCount: 5, activeCount: 4, trainingCount: 1, averageTenure: 2.0, tier1Percent: 40, tier2Percent: 40, tier3Percent: 20 },
    { teamId: "6", teamName: "Foxtrot Team", siteName: "Charlotte", managerName: "David Lee", memberCount: 4, activeCount: 4, trainingCount: 1, averageTenure: 1.8, tier1Percent: 25, tier2Percent: 50, tier3Percent: 25 },
    { teamId: "7", teamName: "Golf Team", siteName: "Charlotte", managerName: "Rachel Kim", memberCount: 5, activeCount: 4, trainingCount: 0, averageTenure: 2.5, tier1Percent: 20, tier2Percent: 60, tier3Percent: 20 },
    { teamId: "8", teamName: "Hotel Team", siteName: "Charlotte", managerName: "Chris Brown", memberCount: 4, activeCount: 4, trainingCount: 1, averageTenure: 2.2, tier1Percent: 25, tier2Percent: 25, tier3Percent: 50 }
  ],
  hierarchyDepth: {
    totalLevels: 4,
    distribution: [
      { level: 1, label: "Sites", count: 2 },
      { level: 2, label: "Teams", count: 8 },
      { level: 3, label: "Managers", count: 6 },
      { level: 4, label: "Agents", count: 36 }
    ],
    spanOfControl: [
      { managerId: "1", managerName: "Sarah Johnson", directReports: 6, totalReports: 6 },
      { managerId: "2", managerName: "Mike Chen", directReports: 6, totalReports: 6 },
      { managerId: "3", managerName: "Lisa Park", directReports: 6, totalReports: 6 },
      { managerId: "4", managerName: "Tom Wilson", directReports: 6, totalReports: 6 },
      { managerId: "5", managerName: "Amy Roberts", directReports: 5, totalReports: 5 },
      { managerId: "6", managerName: "David Lee", directReports: 4, totalReports: 4 }
    ]
  },
  growthTrend: [
    { period: "2024-01", hireCount: 3, cumulativeTotal: 30 },
    { period: "2024-02", hireCount: 2, cumulativeTotal: 32 },
    { period: "2024-03", hireCount: 4, cumulativeTotal: 36 },
    { period: "2024-04", hireCount: 2, cumulativeTotal: 38 },
    { period: "2024-05", hireCount: 1, cumulativeTotal: 39 },
    { period: "2024-06", hireCount: 3, cumulativeTotal: 42 }
  ]
};

export const mockHierarchy: GraphNodeProjection[] = [
  {
    nodeId: "site-1",
    nodeType: "site",
    name: "Austin",
    subtitle: "TX",
    avatarUrl: null,
    hasChildren: true,
    isExpanded: false,
    totalDescendants: 24,
    stats: {
      activeAgents: 22,
      totalAgents: 24,
      trainingAgents: 5,
      averageTenure: 2.5,
      tier1Count: 8,
      tier2Count: 10,
      tier3Count: 6
    },
    visuals: {
      primaryColor: "#3b82f6",
      secondaryColor: "#93c5fd",
      statusIndicator: "active",
      healthScore: 92,
      tenureBadge: null,
      performanceBadge: null
    }
  },
  {
    nodeId: "site-2",
    nodeType: "site",
    name: "Charlotte",
    subtitle: "NC",
    avatarUrl: null,
    hasChildren: true,
    isExpanded: false,
    totalDescendants: 18,
    stats: {
      activeAgents: 16,
      totalAgents: 18,
      trainingAgents: 3,
      averageTenure: 2.1,
      tier1Count: 5,
      tier2Count: 8,
      tier3Count: 5
    },
    visuals: {
      primaryColor: "#8b5cf6",
      secondaryColor: "#c4b5fd",
      statusIndicator: "active",
      healthScore: 89,
      tenureBadge: null,
      performanceBadge: null
    }
  }
];

export const mockFilterOptions = {
  sites: [
    { id: 1, name: "Austin" },
    { id: 2, name: "Charlotte" }
  ],
  teams: [
    { id: 1, name: "Alpha Team" },
    { id: 2, name: "Bravo Team" },
    { id: 3, name: "Charlie Team" },
    { id: 4, name: "Delta Team" },
    { id: 5, name: "Echo Team" },
    { id: 6, name: "Foxtrot Team" },
    { id: 7, name: "Golf Team" },
    { id: 8, name: "Hotel Team" }
  ],
  managers: [
    { id: "mgr-1", name: "Sarah Johnson" },
    { id: "mgr-2", name: "Mike Chen" },
    { id: "mgr-3", name: "Lisa Park" },
    { id: "mgr-4", name: "Tom Wilson" },
    { id: "mgr-5", name: "Amy Roberts" },
    { id: "mgr-6", name: "David Lee" }
  ],
  tags: [
    { id: 1, name: "Top Performer", color: "#22c55e" },
    { id: 2, name: "New Hire", color: "#f59e0b" },
    { id: 3, name: "Training", color: "#3b82f6" }
  ]
};

export function isDemoMode(): boolean {
  const apiUrl = import.meta.env.VITE_API_BASE_URL;
  return !apiUrl || apiUrl === '' || apiUrl === 'undefined';
}
