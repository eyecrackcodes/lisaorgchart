/**
 * TypeScript types for CQRS Graph Traversal API
 * Optimized projections for fast rendering and real-time updates
 */

export interface GraphNodeStats {
  activeAgents: number;
  inactiveAgents: number;
  trainingAgents: number;
  managerCount: number;
  averageTenureYears: number;
  tierDistribution: Record<string, number>;
}

export interface GraphNodeVisuals {
  primaryColor: string;
  secondaryColor: string;
  statusIndicator: 'healthy' | 'warning' | 'critical' | 'neutral' | 'active' | 'inactive' | 'training';
  healthScore: number; // 0-1
  tenureBadge?: string;
  performanceBadge?: string;
}

export interface GraphNodeProjection {
  id: string;
  nodeType: 'Root' | 'Site' | 'Team' | 'Manager' | 'Agent';
  name: string;
  parentId?: string;
  depth: number;
  childCount: number;
  totalDescendants: number;
  hasChildren: boolean;
  stats: GraphNodeStats;
  visuals: GraphNodeVisuals;
}

export interface GraphNodeMetadata {
  siteId?: string;
  siteName?: string;
  teamId?: string;
  teamName?: string;
  title?: string;
  agentStatus?: 'Active' | 'Inactive';
  agentType?: 'Training' | 'Performance';
  commissionTier?: 'None' | 'Tier1' | 'Tier2' | 'Tier3';
  startDate?: string;
  tenureYears?: number;
  email?: string;
  phone?: string;
  imageUrl?: string;
  city?: string;
  state?: string;
}

export interface GraphTagInfo {
  id: number;
  name: string;
  hexColor: string;
  category: 'status' | 'type' | 'tier' | 'tenure' | 'role' | 'general';
}

export interface GraphNodeBreadcrumb {
  id: string;
  name: string;
  nodeType: string;
}

export interface GraphNodeDetail extends GraphNodeProjection {
  metadata: GraphNodeMetadata;
  tags: GraphTagInfo[];
  directReports: GraphNodeProjection[];
  manager?: GraphNodeProjection;
  peers: GraphNodeProjection[];
  breadcrumbs: GraphNodeBreadcrumb[];
}

export interface GraphQuery {
  siteId?: number;
  teamId?: number;
  managerId?: string;
  tagIds?: number[];
  includeInactive?: boolean;
  maxDepth?: number;
  shallowLoad?: boolean;
  focusNodeId?: string;
  searchTerm?: string;
}

// Visual encoding configuration
export const TIER_COLORS: Record<string, string> = {
  'Tier3': '#8b5cf6', // Purple - top tier
  'Tier2': '#3b82f6', // Blue - mid tier  
  'Tier1': '#22c55e', // Green - entry tier
  'None': '#6b7280'   // Gray
};

export const STATUS_COLORS: Record<string, string> = {
  'healthy': '#22c55e',
  'active': '#22c55e',
  'warning': '#f59e0b',
  'training': '#f59e0b',
  'critical': '#ef4444',
  'inactive': '#6b7280',
  'neutral': '#94a3b8'
};

export const NODE_TYPE_COLORS: Record<string, string> = {
  'Site': '#0ea5e9',
  'Team': '#22c55e',
  'Manager': '#8b5cf6',
  'Agent': '#6b7280',
  'Root': '#1f2937'
};
