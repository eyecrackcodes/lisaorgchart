/**
 * TypeScript types for org chart data structures
 */

export interface OrgChartNode {
  id: string;
  nodeType: "Root" | "Site" | "Team" | "Manager" | "Agent";
  name: string;
  parentId?: string;
  depth: number;
  managerId?: string;
  directReportCount: number;
  totalReportCount: number;
  tags: Tag[];
  metadata: OrgChartNodeMetadata;
  children: OrgChartNode[];
}

export interface OrgChartNodeMetadata {
  siteId?: string;
  siteName?: string;
  teamId?: string;
  teamName?: string;
  title?: string;
  agentStatus?: "Active" | "Inactive";
  agentType?: "Training" | "Performance";
  commissionTier?: "None" | "Tier1" | "Tier2" | "Tier3";
  agencyStartDate?: string;
  tenureYears?: number;
  email?: string;
  imageUrl?: string;
  city?: string;
  state?: string;
  phone?: string;
}

export interface Tag {
  id: number;
  name: string;
  hexColorCode?: string;
}

export interface OrgChartFilterOptions {
  sites: FilterOption[];
  teams: FilterOption[];
  managers: FilterOption[];
  tags: Tag[];
}

export interface FilterOption {
  id: string;
  name: string;
}

export interface OrgChartQueryParams {
  siteId?: number;
  teamId?: number;
  managerId?: string;
  tagIds?: number[];
  includeInactive?: boolean;
  depth?: number;
  search?: string;
}

export type NodeType = "Root" | "Site" | "Team" | "Manager" | "Agent";

export interface OrgChartState {
  tree: OrgChartNode | null;
  filterOptions: OrgChartFilterOptions | null;
  params: OrgChartQueryParams;
  expandedIds: Set<string>;
  selectedId: string | null;
  loading: boolean;
  error: Error | null;
}
