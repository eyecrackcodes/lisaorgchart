using LuminaryLife.Common.Systems.OrgChart.Models;

namespace LuminaryLife.Common.Systems.OrgChart.Services;

/// <summary>
/// Service interface for org chart operations
/// </summary>
public interface IOrgChartService
{
    /// <summary>
    /// Gets the full org chart tree with optional filtering
    /// </summary>
    Task<OrgChartNodeDto> GetOrgChartTreeAsync(OrgChartQueryParams query);
    
    /// <summary>
    /// Gets a flat list of all org chart nodes
    /// </summary>
    Task<List<OrgChartNodeDto>> GetOrgChartFlatAsync(OrgChartQueryParams query);
    
    /// <summary>
    /// Gets a subtree starting from a specific site, team, or manager
    /// </summary>
    Task<OrgChartNodeDto?> GetOrgChartSubtreeAsync(int? siteId, int? teamId, string? managerId, OrgChartQueryParams query);
    
    /// <summary>
    /// Gets a single person node by user ID
    /// </summary>
    Task<OrgChartNodeDto?> GetPersonNodeAsync(string userId);
    
    /// <summary>
    /// Gets available filter options for the UI
    /// </summary>
    Task<OrgChartFilterOptions> GetFilterOptionsAsync();
}
