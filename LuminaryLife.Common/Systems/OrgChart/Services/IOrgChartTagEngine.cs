using LuminaryLife.Common.Entities;

namespace LuminaryLife.Common.Systems.OrgChart.Services;

/// <summary>
/// Interface for automated tag computation engine
/// </summary>
public interface IOrgChartTagEngine
{
    /// <summary>
    /// Computes applicable tag IDs for a user based on their properties
    /// </summary>
    Task<List<int>> ComputeTagsForUserAsync(User user);
    
    /// <summary>
    /// Computes applicable tag IDs for a site based on its properties
    /// </summary>
    Task<List<int>> ComputeTagsForSiteAsync(AgencySite site);
    
    /// <summary>
    /// Computes applicable tag IDs for a team based on its properties
    /// </summary>
    Task<List<int>> ComputeTagsForTeamAsync(AgencyTeam team);
    
    /// <summary>
    /// Syncs computed tags for a specific user
    /// </summary>
    Task SyncTagsForUserAsync(string userId);
    
    /// <summary>
    /// Syncs computed tags for a specific site
    /// </summary>
    Task SyncTagsForSiteAsync(int siteId);
    
    /// <summary>
    /// Syncs computed tags for a specific team
    /// </summary>
    Task SyncTagsForTeamAsync(int teamId);
    
    /// <summary>
    /// Syncs tags for all entities in the system
    /// </summary>
    Task SyncAllTagsAsync();
}
