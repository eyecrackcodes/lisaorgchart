using LuminaryLife.Common.Systems.Tags.Models;

namespace LuminaryLife.Common.Systems.OrgChart.Models;

/// <summary>
/// Data transfer object representing a node in the org chart tree
/// Can represent a Root, Site, Team, Manager, or Agent node
/// </summary>
public class OrgChartNodeDto
{
    /// <summary>
    /// Unique identifier for the node (e.g., "root", "site-1", "team-5", or user GUID)
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Type of node: Root, Site, Team, Manager, Agent
    /// </summary>
    public string NodeType { get; set; } = string.Empty;

    /// <summary>
    /// Display name for the node
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Parent node ID for tree traversal
    /// </summary>
    public string? ParentId { get; set; }

    /// <summary>
    /// Depth level in the tree (0 = root, 1 = site, 2 = team, 3 = person)
    /// </summary>
    public int Depth { get; set; }

    /// <summary>
    /// For team nodes, the manager's user ID
    /// </summary>
    public string? ManagerId { get; set; }

    /// <summary>
    /// Number of direct reports for this node
    /// </summary>
    public int DirectReportCount { get; set; }

    /// <summary>
    /// Total number of reports including nested (for rollup calculations)
    /// </summary>
    public int TotalReportCount { get; set; }

    /// <summary>
    /// Tags assigned to this entity
    /// </summary>
    public List<TagResponse> Tags { get; set; } = new();

    /// <summary>
    /// Extended metadata for the node
    /// </summary>
    public OrgChartNodeMetadata Metadata { get; set; } = new();

    /// <summary>
    /// Child nodes in the tree
    /// </summary>
    public List<OrgChartNodeDto> Children { get; set; } = new();
}
