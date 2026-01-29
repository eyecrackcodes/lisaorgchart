namespace LuminaryLife.Common.Systems.OrgChart.Queries;

/// <summary>
/// CQRS Query Service - Optimized for fast graph reads with sub-second latency
/// </summary>
public interface IGraphTraversalService
{
    /// <summary>
    /// Gets the high-level hierarchy view with aggregated stats
    /// Optimized for initial page load - shallow traversal
    /// </summary>
    Task<List<GraphNodeProjection>> GetHierarchyOverviewAsync(GraphQuery query);
    
    /// <summary>
    /// Gets children of a specific node - for progressive disclosure
    /// Only loads immediate children, not full subtree
    /// </summary>
    Task<List<GraphNodeProjection>> GetNodeChildrenAsync(string nodeId, GraphQuery query);
    
    /// <summary>
    /// Gets full details for a specific node - for detail panel
    /// Includes related nodes (manager, peers, direct reports)
    /// </summary>
    Task<GraphNodeDetail?> GetNodeDetailAsync(string nodeId);
    
    /// <summary>
    /// Gets subtree rooted at a specific node - for drill-down views
    /// </summary>
    Task<GraphNodeProjection?> GetSubtreeAsync(string nodeId, int maxDepth = 2);
    
    /// <summary>
    /// Search nodes by name/email with relevance scoring
    /// </summary>
    Task<List<GraphNodeProjection>> SearchNodesAsync(string searchTerm, int limit = 20);
    
    /// <summary>
    /// Gets aggregated statistics for the entire org or a subtree
    /// </summary>
    Task<GraphNodeStats> GetAggregateStatsAsync(string? rootNodeId = null);
    
    /// <summary>
    /// Invalidates cache for a specific node or all nodes
    /// Called after mutations
    /// </summary>
    Task InvalidateCacheAsync(string? nodeId = null);
}
