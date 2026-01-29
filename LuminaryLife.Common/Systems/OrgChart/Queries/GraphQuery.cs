namespace LuminaryLife.Common.Systems.OrgChart.Queries;

/// <summary>
/// CQRS Query for high-level graph traversal - optimized for fast reads
/// </summary>
public class GraphQuery
{
    public int? SiteId { get; set; }
    public int? TeamId { get; set; }
    public string? ManagerId { get; set; }
    public List<int>? TagIds { get; set; }
    public bool IncludeInactive { get; set; }
    public int? MaxDepth { get; set; }
    public string? SearchTerm { get; set; }
    public bool ShallowLoad { get; set; } // Only load immediate children for progressive disclosure
    public string? FocusNodeId { get; set; } // For drill-down navigation
    
    public string GetCacheKey() =>
        $"graph:{SiteId}:{TeamId}:{ManagerId}:{(TagIds != null ? string.Join(",", TagIds) : "")}:" +
        $"{IncludeInactive}:{MaxDepth}:{ShallowLoad}:{FocusNodeId}:{SearchTerm?.ToLower()}";
}

/// <summary>
/// Lightweight projection for hierarchy overview
/// </summary>
public class GraphNodeProjection
{
    public string Id { get; set; } = string.Empty;
    public string NodeType { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? ParentId { get; set; }
    public int Depth { get; set; }
    public int ChildCount { get; set; }
    public int TotalDescendants { get; set; }
    public bool HasChildren { get; set; }
    public GraphNodeStats Stats { get; set; } = new();
    public GraphNodeVisuals Visuals { get; set; } = new();
}

/// <summary>
/// Aggregated statistics for a node
/// </summary>
public class GraphNodeStats
{
    public int ActiveAgents { get; set; }
    public int InactiveAgents { get; set; }
    public int TrainingAgents { get; set; }
    public int ManagerCount { get; set; }
    public double AverageTenureYears { get; set; }
    public Dictionary<string, int> TierDistribution { get; set; } = new();
}

/// <summary>
/// Visual encoding data for frontend rendering
/// </summary>
public class GraphNodeVisuals
{
    public string PrimaryColor { get; set; } = "#6b7280";
    public string SecondaryColor { get; set; } = "#e5e7eb";
    public string StatusIndicator { get; set; } = "neutral"; // healthy, warning, critical
    public double HealthScore { get; set; } = 1.0; // 0-1 for visual intensity
    public string? TenureBadge { get; set; } // "New", "1yr+", "5yr+"
    public string? PerformanceBadge { get; set; }
}

/// <summary>
/// Complete node with full details - used for detail panel
/// </summary>
public class GraphNodeDetail : GraphNodeProjection
{
    public GraphNodeMetadata Metadata { get; set; } = new();
    public List<GraphTagInfo> Tags { get; set; } = new();
    public List<GraphNodeProjection> DirectReports { get; set; } = new();
    public GraphNodeProjection? Manager { get; set; }
    public List<GraphNodeProjection> Peers { get; set; } = new();
    public GraphNodeBreadcrumb[] Breadcrumbs { get; set; } = Array.Empty<GraphNodeBreadcrumb>();
}

/// <summary>
/// Metadata for person nodes
/// </summary>
public class GraphNodeMetadata
{
    public string? SiteId { get; set; }
    public string? SiteName { get; set; }
    public string? TeamId { get; set; }
    public string? TeamName { get; set; }
    public string? Title { get; set; }
    public string? AgentStatus { get; set; }
    public string? AgentType { get; set; }
    public string? CommissionTier { get; set; }
    public DateTime? StartDate { get; set; }
    public double? TenureYears { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? ImageUrl { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
}

/// <summary>
/// Tag info with visual properties
/// </summary>
public class GraphTagInfo
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string HexColor { get; set; } = "808080";
    public string Category { get; set; } = "general"; // status, performance, tier, tenure
}

/// <summary>
/// Breadcrumb for navigation
/// </summary>
public class GraphNodeBreadcrumb
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string NodeType { get; set; } = string.Empty;
}
