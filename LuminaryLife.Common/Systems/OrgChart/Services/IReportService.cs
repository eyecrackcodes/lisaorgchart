namespace LuminaryLife.Common.Systems.OrgChart.Services;

/// <summary>
/// Service interface for organizational reporting and analytics
/// </summary>
public interface IReportService
{
    /// <summary>
    /// Gets complete report data for the organization
    /// </summary>
    Task<OrgReportData> GetFullReportAsync();
    
    /// <summary>
    /// Gets organization summary metrics
    /// </summary>
    Task<OrgSummary> GetSummaryAsync();
    
    /// <summary>
    /// Gets site-by-site comparison data
    /// </summary>
    Task<List<SiteComparison>> GetSiteComparisonAsync();
    
    /// <summary>
    /// Gets team composition breakdown
    /// </summary>
    Task<List<TeamCompositionData>> GetTeamCompositionAsync();
    
    /// <summary>
    /// Gets tier distribution across the organization
    /// </summary>
    Task<TierDistributionData> GetTierDistributionAsync();
    
    /// <summary>
    /// Gets tenure distribution analysis
    /// </summary>
    Task<TenureDistributionData> GetTenureDistributionAsync();
    
    /// <summary>
    /// Gets hierarchy depth and span of control analysis
    /// </summary>
    Task<HierarchyDepthData> GetHierarchyAnalysisAsync();
    
    /// <summary>
    /// Gets growth trend data (hires over time)
    /// </summary>
    Task<List<GrowthTrendData>> GetGrowthTrendAsync();
}
