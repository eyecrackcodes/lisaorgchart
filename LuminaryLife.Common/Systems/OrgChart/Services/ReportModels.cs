namespace LuminaryLife.Common.Systems.OrgChart.Services;

/// <summary>
/// Report data models for organizational analytics
/// </summary>

public class OrgReportData
{
    public OrgSummary Summary { get; set; } = new();
    public List<SiteComparison> SiteComparison { get; set; } = new();
    public TierDistributionData TierDistribution { get; set; } = new();
    public TenureDistributionData TenureDistribution { get; set; } = new();
    public List<TeamCompositionData> TeamComposition { get; set; } = new();
    public HierarchyDepthData HierarchyDepth { get; set; } = new();
    public List<GrowthTrendData> GrowthTrend { get; set; } = new();
}

public class OrgSummary
{
    public int TotalAgents { get; set; }
    public int ActiveAgents { get; set; }
    public int InactiveAgents { get; set; }
    public int TrainingAgents { get; set; }
    public int TotalManagers { get; set; }
    public int TotalTeams { get; set; }
    public int TotalSites { get; set; }
    public double AverageTenure { get; set; }
    public double AverageTeamSize { get; set; }
}

public class SiteComparison
{
    public string SiteId { get; set; } = string.Empty;
    public string SiteName { get; set; } = string.Empty;
    public int TotalAgents { get; set; }
    public int ActiveAgents { get; set; }
    public int TrainingAgents { get; set; }
    public int TeamCount { get; set; }
    public double AverageTenure { get; set; }
    public int Tier1Count { get; set; }
    public int Tier2Count { get; set; }
    public int Tier3Count { get; set; }
}

public class TierDistributionData
{
    public int Tier1 { get; set; }
    public int Tier2 { get; set; }
    public int Tier3 { get; set; }
    public int None { get; set; }
    public List<TierBySite> BySite { get; set; } = new();
}

public class TierBySite
{
    public string SiteName { get; set; } = string.Empty;
    public int Tier1 { get; set; }
    public int Tier2 { get; set; }
    public int Tier3 { get; set; }
}

public class TenureDistributionData
{
    public List<TenureRange> Ranges { get; set; } = new();
    public List<TenureByRole> AverageByRole { get; set; } = new();
}

public class TenureRange
{
    public string Label { get; set; } = string.Empty;
    public int Count { get; set; }
    public double Percentage { get; set; }
}

public class TenureByRole
{
    public string Role { get; set; } = string.Empty;
    public double AverageTenure { get; set; }
}

public class TeamCompositionData
{
    public string TeamId { get; set; } = string.Empty;
    public string TeamName { get; set; } = string.Empty;
    public string SiteName { get; set; } = string.Empty;
    public string ManagerName { get; set; } = string.Empty;
    public int MemberCount { get; set; }
    public int ActiveCount { get; set; }
    public int TrainingCount { get; set; }
    public double AverageTenure { get; set; }
    public double Tier1Percent { get; set; }
    public double Tier2Percent { get; set; }
    public double Tier3Percent { get; set; }
}

public class HierarchyDepthData
{
    public int TotalLevels { get; set; }
    public List<HierarchyLevel> Distribution { get; set; } = new();
    public List<SpanOfControl> SpanOfControl { get; set; } = new();
}

public class HierarchyLevel
{
    public int Level { get; set; }
    public string Label { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class SpanOfControl
{
    public string ManagerId { get; set; } = string.Empty;
    public string ManagerName { get; set; } = string.Empty;
    public int DirectReports { get; set; }
    public int TotalReports { get; set; }
}

public class GrowthTrendData
{
    public string Period { get; set; } = string.Empty;
    public int HireCount { get; set; }
    public int CumulativeTotal { get; set; }
}
