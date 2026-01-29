using LuminaryLife.Common.Systems.Tags.Models;

namespace LuminaryLife.Common.Systems.OrgChart.Models;

/// <summary>
/// Available filter options for the org chart UI
/// </summary>
public class OrgChartFilterOptions
{
    /// <summary>
    /// Available agency sites to filter by
    /// </summary>
    public List<FilterOption> Sites { get; set; } = new();

    /// <summary>
    /// Available teams to filter by
    /// </summary>
    public List<FilterOption> Teams { get; set; } = new();

    /// <summary>
    /// Available managers to filter by
    /// </summary>
    public List<FilterOption> Managers { get; set; } = new();

    /// <summary>
    /// Available tags to filter by
    /// </summary>
    public List<TagResponse> Tags { get; set; } = new();
}
