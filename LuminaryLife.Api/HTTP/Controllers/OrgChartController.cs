using LuminaryLife.Common.Systems.OrgChart.Hubs;
using LuminaryLife.Common.Systems.OrgChart.Models;
using LuminaryLife.Common.Systems.OrgChart.Queries;
using LuminaryLife.Common.Systems.OrgChart.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LuminaryLife.Api.HTTP.Controllers;

/// <summary>
/// API controller for org chart operations
/// Implements CQRS pattern with optimized query endpoints
/// </summary>
[ApiController]
[AllowAnonymous] // Allow anonymous for demo purposes - change to [Authorize] in production
[Route("/org-chart")]
public class OrgChartController : ControllerBase
{
    private readonly IOrgChartService _orgChartService;
    private readonly IOrgChartTagEngine _tagEngine;
    private readonly IGraphTraversalService _graphService;
    private readonly IOrgChartNotifier _notifier;
    private readonly IReportService _reportService;

    public OrgChartController(
        IOrgChartService orgChartService,
        IOrgChartTagEngine tagEngine,
        IGraphTraversalService graphService,
        IOrgChartNotifier notifier,
        IReportService reportService)
    {
        _orgChartService = orgChartService;
        _tagEngine = tagEngine;
        _graphService = graphService;
        _notifier = notifier;
        _reportService = reportService;
    }

    /// <summary>
    /// Gets the full org chart tree with optional filtering
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(OrgChartNodeDto), StatusCodes.Status200OK)]
    public async Task<OrgChartNodeDto> GetOrgChart(
        [FromQuery] int? siteId,
        [FromQuery] int? teamId,
        [FromQuery] string? managerId,
        [FromQuery] string? tagIds,
        [FromQuery] bool includeInactive = false,
        [FromQuery] int? depth = null,
        [FromQuery] string? search = null)
    {
        var query = new OrgChartQueryParams
        {
            SiteId = siteId,
            TeamId = teamId,
            ManagerId = managerId,
            TagIds = ParseTagIds(tagIds),
            IncludeInactive = includeInactive,
            MaxDepth = depth,
            SearchTerm = search
        };
        
        return await _orgChartService.GetOrgChartTreeAsync(query);
    }

    /// <summary>
    /// Gets a flat list of all org chart nodes
    /// </summary>
    [HttpGet("flat")]
    [ProducesResponseType(typeof(List<OrgChartNodeDto>), StatusCodes.Status200OK)]
    public async Task<List<OrgChartNodeDto>> GetOrgChartFlat(
        [FromQuery] int? siteId,
        [FromQuery] int? teamId,
        [FromQuery] string? managerId,
        [FromQuery] bool includeInactive = false,
        [FromQuery] string? search = null)
    {
        var query = new OrgChartQueryParams
        {
            SiteId = siteId,
            TeamId = teamId,
            ManagerId = managerId,
            IncludeInactive = includeInactive,
            SearchTerm = search
        };
        
        return await _orgChartService.GetOrgChartFlatAsync(query);
    }

    /// <summary>
    /// Gets a subtree starting from a specific site, team, or manager
    /// </summary>
    [HttpGet("subtree")]
    [ProducesResponseType(typeof(OrgChartNodeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrgChartNodeDto>> GetSubtree(
        [FromQuery] int? siteId,
        [FromQuery] int? teamId,
        [FromQuery] string? managerId,
        [FromQuery] bool includeInactive = false)
    {
        var query = new OrgChartQueryParams { IncludeInactive = includeInactive };
        var node = await _orgChartService.GetOrgChartSubtreeAsync(siteId, teamId, managerId, query);
        
        return node == null ? NotFound() : Ok(node);
    }

    /// <summary>
    /// Gets available filter options for the UI
    /// </summary>
    [HttpGet("filter-options")]
    [ProducesResponseType(typeof(OrgChartFilterOptions), StatusCodes.Status200OK)]
    public async Task<OrgChartFilterOptions> GetFilterOptions()
    {
        return await _orgChartService.GetFilterOptionsAsync();
    }

    /// <summary>
    /// Gets a single person node by user ID
    /// </summary>
    [HttpGet("person/{userId}")]
    [ProducesResponseType(typeof(OrgChartNodeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrgChartNodeDto>> GetPerson([FromRoute] string userId)
    {
        var node = await _orgChartService.GetPersonNodeAsync(userId);
        return node == null ? NotFound() : Ok(node);
    }

    /// <summary>
    /// Triggers a full tag sync for all entities (admin only)
    /// </summary>
    [HttpPost("sync-tags")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> SyncAllTags()
    {
        await _tagEngine.SyncAllTagsAsync();
        return Ok(new { message = "Tag sync completed successfully" });
    }

    /// <summary>
    /// Syncs tags for a specific user
    /// </summary>
    [HttpPost("person/{userId}/sync-tags")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SyncUserTags([FromRoute] string userId)
    {
        await _tagEngine.SyncTagsForUserAsync(userId);
        return Ok(new { message = $"Tags synced for user {userId}" });
    }

    // ===============================
    // CQRS Query Endpoints (Optimized)
    // ===============================

    /// <summary>
    /// Gets the hierarchy overview with aggregated stats - optimized for fast initial load
    /// </summary>
    [HttpGet("graph/hierarchy")]
    [ProducesResponseType(typeof(List<GraphNodeProjection>), StatusCodes.Status200OK)]
    public async Task<List<GraphNodeProjection>> GetHierarchyOverview(
        [FromQuery] int? siteId,
        [FromQuery] int? teamId,
        [FromQuery] string? managerId,
        [FromQuery] bool includeInactive = false,
        [FromQuery] bool shallowLoad = false,
        [FromQuery] string? search = null)
    {
        var query = new GraphQuery
        {
            SiteId = siteId,
            TeamId = teamId,
            ManagerId = managerId,
            IncludeInactive = includeInactive,
            ShallowLoad = shallowLoad,
            SearchTerm = search
        };
        
        return await _graphService.GetHierarchyOverviewAsync(query);
    }

    /// <summary>
    /// Gets children of a specific node - for progressive disclosure
    /// </summary>
    [HttpGet("graph/node/{nodeId}/children")]
    [ProducesResponseType(typeof(List<GraphNodeProjection>), StatusCodes.Status200OK)]
    public async Task<List<GraphNodeProjection>> GetNodeChildren(
        [FromRoute] string nodeId,
        [FromQuery] bool includeInactive = false,
        [FromQuery] string? search = null)
    {
        var query = new GraphQuery
        {
            IncludeInactive = includeInactive,
            SearchTerm = search
        };
        
        return await _graphService.GetNodeChildrenAsync(nodeId, query);
    }

    /// <summary>
    /// Gets full details for a specific node - for detail panel
    /// </summary>
    [HttpGet("graph/node/{nodeId}")]
    [ProducesResponseType(typeof(GraphNodeDetail), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GraphNodeDetail>> GetNodeDetail([FromRoute] string nodeId)
    {
        var detail = await _graphService.GetNodeDetailAsync(nodeId);
        return detail == null ? NotFound() : Ok(detail);
    }

    /// <summary>
    /// Search nodes by name or email
    /// </summary>
    [HttpGet("graph/search")]
    [ProducesResponseType(typeof(List<GraphNodeProjection>), StatusCodes.Status200OK)]
    public async Task<List<GraphNodeProjection>> SearchNodes(
        [FromQuery] string q,
        [FromQuery] int limit = 20)
    {
        return await _graphService.SearchNodesAsync(q, limit);
    }

    /// <summary>
    /// Gets aggregated statistics for the org or a specific node
    /// </summary>
    [HttpGet("graph/stats")]
    [ProducesResponseType(typeof(GraphNodeStats), StatusCodes.Status200OK)]
    public async Task<GraphNodeStats> GetStats([FromQuery] string? nodeId = null)
    {
        return await _graphService.GetAggregateStatsAsync(nodeId);
    }

    // ===============================
    // Reporting Endpoints
    // ===============================

    /// <summary>
    /// Gets complete report data for the organization
    /// </summary>
    [HttpGet("reports")]
    [ProducesResponseType(typeof(OrgReportData), StatusCodes.Status200OK)]
    public async Task<OrgReportData> GetFullReport()
    {
        return await _reportService.GetFullReportAsync();
    }

    /// <summary>
    /// Gets organization summary metrics
    /// </summary>
    [HttpGet("reports/summary")]
    [ProducesResponseType(typeof(OrgSummary), StatusCodes.Status200OK)]
    public async Task<OrgSummary> GetReportSummary()
    {
        return await _reportService.GetSummaryAsync();
    }

    /// <summary>
    /// Gets site-by-site comparison data
    /// </summary>
    [HttpGet("reports/sites")]
    [ProducesResponseType(typeof(List<SiteComparison>), StatusCodes.Status200OK)]
    public async Task<List<SiteComparison>> GetSiteComparison()
    {
        return await _reportService.GetSiteComparisonAsync();
    }

    /// <summary>
    /// Gets team composition breakdown
    /// </summary>
    [HttpGet("reports/teams")]
    [ProducesResponseType(typeof(List<TeamCompositionData>), StatusCodes.Status200OK)]
    public async Task<List<TeamCompositionData>> GetTeamComposition()
    {
        return await _reportService.GetTeamCompositionAsync();
    }

    /// <summary>
    /// Gets tier distribution across the organization
    /// </summary>
    [HttpGet("reports/tiers")]
    [ProducesResponseType(typeof(TierDistributionData), StatusCodes.Status200OK)]
    public async Task<TierDistributionData> GetTierDistribution()
    {
        return await _reportService.GetTierDistributionAsync();
    }

    /// <summary>
    /// Gets tenure distribution analysis
    /// </summary>
    [HttpGet("reports/tenure")]
    [ProducesResponseType(typeof(TenureDistributionData), StatusCodes.Status200OK)]
    public async Task<TenureDistributionData> GetTenureDistribution()
    {
        return await _reportService.GetTenureDistributionAsync();
    }

    /// <summary>
    /// Gets hierarchy depth and span of control analysis
    /// </summary>
    [HttpGet("reports/hierarchy")]
    [ProducesResponseType(typeof(HierarchyDepthData), StatusCodes.Status200OK)]
    public async Task<HierarchyDepthData> GetHierarchyAnalysis()
    {
        return await _reportService.GetHierarchyAnalysisAsync();
    }

    private static List<int>? ParseTagIds(string? tagIds)
    {
        if (string.IsNullOrEmpty(tagIds)) return null;
        
        return tagIds
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(s => int.TryParse(s.Trim(), out var id) ? id : (int?)null)
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .ToList();
    }
}
