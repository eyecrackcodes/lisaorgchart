/**
 * TypeScript types for reporting and analytics
 */

export interface OrgReportData {
  summary: OrgSummary;
  siteComparison: SiteComparison[];
  tierDistribution: TierDistributionData;
  tenureDistribution: TenureDistributionData;
  teamComposition: TeamCompositionData[];
  hierarchyDepth: HierarchyDepthData;
  growthTrend: GrowthTrendData[];
}

export interface OrgSummary {
  totalAgents: number;
  activeAgents: number;
  inactiveAgents: number;
  trainingAgents: number;
  totalManagers: number;
  totalTeams: number;
  totalSites: number;
  averageTenure: number;
  averageTeamSize: number;
}

export interface SiteComparison {
  siteId: string;
  siteName: string;
  totalAgents: number;
  activeAgents: number;
  trainingAgents: number;
  teamCount: number;
  averageTenure: number;
  tier1Count: number;
  tier2Count: number;
  tier3Count: number;
}

export interface TierDistributionData {
  tier1: number;
  tier2: number;
  tier3: number;
  none: number;
  bySite: {
    siteName: string;
    tier1: number;
    tier2: number;
    tier3: number;
  }[];
}

export interface TenureDistributionData {
  ranges: {
    label: string;
    count: number;
    percentage: number;
  }[];
  averageByRole: {
    role: string;
    averageTenure: number;
  }[];
}

export interface TeamCompositionData {
  teamId: string;
  teamName: string;
  siteName: string;
  managerName: string;
  memberCount: number;
  activeCount: number;
  trainingCount: number;
  averageTenure: number;
  tier1Percent: number;
  tier2Percent: number;
  tier3Percent: number;
}

export interface HierarchyDepthData {
  totalLevels: number;
  distribution: {
    level: number;
    label: string;
    count: number;
  }[];
  spanOfControl: {
    managerId: string;
    managerName: string;
    directReports: number;
    totalReports: number;
  }[];
}

export interface GrowthTrendData {
  period: string;
  hireCount: number;
  cumulativeTotal: number;
}

// Chart color palettes
export const CHART_COLORS = {
  primary: '#3b82f6',
  secondary: '#8b5cf6',
  success: '#22c55e',
  warning: '#f59e0b',
  danger: '#ef4444',
  info: '#0ea5e9',
  gray: '#6b7280',
  
  tier1: '#22c55e',
  tier2: '#3b82f6',
  tier3: '#8b5cf6',
  
  sites: ['#0ea5e9', '#8b5cf6', '#f59e0b', '#22c55e', '#ef4444'],
  
  gradient: [
    'rgba(59, 130, 246, 0.8)',
    'rgba(139, 92, 246, 0.8)',
    'rgba(34, 197, 94, 0.8)',
    'rgba(245, 158, 11, 0.8)',
    'rgba(239, 68, 68, 0.8)',
  ]
};
