namespace LuminaryLife.Common.Systems.OrgChart.Models;

/// <summary>
/// Extended metadata for org chart nodes including site, team, and agent details
/// </summary>
public class OrgChartNodeMetadata
{
    /// <summary>
    /// The agency site ID this node belongs to
    /// </summary>
    public string? SiteId { get; set; }

    /// <summary>
    /// The agency site name
    /// </summary>
    public string? SiteName { get; set; }

    /// <summary>
    /// The team ID this node belongs to
    /// </summary>
    public string? TeamId { get; set; }

    /// <summary>
    /// The team name
    /// </summary>
    public string? TeamName { get; set; }

    /// <summary>
    /// Job title (Agent, Manager, Admin, Developer, Other)
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Agent status (Active, Inactive)
    /// </summary>
    public string? AgentStatus { get; set; }

    /// <summary>
    /// Agent type (Training, Performance)
    /// </summary>
    public string? AgentType { get; set; }

    /// <summary>
    /// Commission tier (None, Tier1, Tier2, Tier3)
    /// </summary>
    public string? CommissionTier { get; set; }

    /// <summary>
    /// Date the agent started at the agency
    /// </summary>
    public DateTime? AgencyStartDate { get; set; }

    /// <summary>
    /// Calculated tenure in years
    /// </summary>
    public double? TenureYears { get; set; }

    /// <summary>
    /// Agent's email address
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// URL to agent's avatar image
    /// </summary>
    public string? ImageUrl { get; set; }

    /// <summary>
    /// City location
    /// </summary>
    public string? City { get; set; }

    /// <summary>
    /// State location
    /// </summary>
    public string? State { get; set; }

    /// <summary>
    /// Phone number
    /// </summary>
    public string? Phone { get; set; }
}
