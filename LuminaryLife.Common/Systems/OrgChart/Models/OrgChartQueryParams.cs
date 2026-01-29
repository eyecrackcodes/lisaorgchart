namespace LuminaryLife.Common.Systems.OrgChart.Models;

/// <summary>
/// Query parameters for filtering and configuring org chart requests
/// </summary>
public class OrgChartQueryParams
{
    /// <summary>
    /// Filter by specific site ID
    /// </summary>
    public int? SiteId { get; set; }

    /// <summary>
    /// Filter by specific team ID
    /// </summary>
    public int? TeamId { get; set; }

    /// <summary>
    /// Filter by specific manager user ID
    /// </summary>
    public string? ManagerId { get; set; }

    /// <summary>
    /// Filter by tag IDs (nodes must have at least one of these tags)
    /// </summary>
    public List<int>? TagIds { get; set; }

    /// <summary>
    /// Include inactive agents in the results
    /// </summary>
    public bool IncludeInactive { get; set; }

    /// <summary>
    /// Maximum depth to traverse (null = unlimited)
    /// </summary>
    public int? MaxDepth { get; set; }

    /// <summary>
    /// Search term for filtering by name
    /// </summary>
    public string? SearchTerm { get; set; }

    /// <summary>
    /// Generates a unique cache key based on query parameters
    /// </summary>
    public string GetCacheKey()
    {
        var tagIdString = TagIds != null && TagIds.Count > 0 
            ? string.Join(",", TagIds.OrderBy(t => t)) 
            : "";
        
        return $"{SiteId ?? 0}_{TeamId ?? 0}_{ManagerId ?? ""}_{tagIdString}_{IncludeInactive}_{MaxDepth ?? 99}_{SearchTerm ?? ""}";
    }
}
