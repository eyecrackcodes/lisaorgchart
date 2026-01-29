using LuminaryLife.Common.Data;
using LuminaryLife.Common.Enums;
using Microsoft.EntityFrameworkCore;

namespace LuminaryLife.Common.Systems.OrgChart.Services;

/// <summary>
/// Implementation of organizational reporting service
/// </summary>
public class ReportService : IReportService
{
    private readonly CoreApiEfDbContext _context;

    public ReportService(CoreApiEfDbContext context)
    {
        _context = context;
    }

    public async Task<OrgReportData> GetFullReportAsync()
    {
        return new OrgReportData
        {
            Summary = await GetSummaryAsync(),
            SiteComparison = await GetSiteComparisonAsync(),
            TierDistribution = await GetTierDistributionAsync(),
            TenureDistribution = await GetTenureDistributionAsync(),
            TeamComposition = await GetTeamCompositionAsync(),
            HierarchyDepth = await GetHierarchyAnalysisAsync(),
            GrowthTrend = await GetGrowthTrendAsync()
        };
    }

    public async Task<OrgSummary> GetSummaryAsync()
    {
        var users = await _context.Users
            .Where(u => u.DeletedAt == null)
            .ToListAsync();

        var teams = await _context.AgencyTeams
            .Where(t => t.DeletedAt == null)
            .ToListAsync();

        var sites = await _context.AgencySites
            .Where(s => s.DeletedAt == null)
            .ToListAsync();

        var tenures = users
            .Where(u => u.AgencyStartDate.HasValue)
            .Select(u => (DateTime.UtcNow - u.AgencyStartDate!.Value).TotalDays / 365.0)
            .ToList();

        return new OrgSummary
        {
            TotalAgents = users.Count,
            ActiveAgents = users.Count(u => u.AgentStatus == AgentStatusEnum.Active),
            InactiveAgents = users.Count(u => u.AgentStatus == AgentStatusEnum.Inactive),
            TrainingAgents = users.Count(u => u.AgentType == AgentTypeEnum.Training),
            TotalManagers = users.Count(u => u.Title == TitleEnum.Manager),
            TotalTeams = teams.Count,
            TotalSites = sites.Count,
            AverageTenure = tenures.Any() ? tenures.Average() : 0,
            AverageTeamSize = teams.Any() ? (double)users.Count / teams.Count : 0
        };
    }

    public async Task<List<SiteComparison>> GetSiteComparisonAsync()
    {
        var sites = await _context.AgencySites
            .Where(s => s.DeletedAt == null)
            .ToListAsync();

        var users = await _context.Users
            .Where(u => u.DeletedAt == null)
            .ToListAsync();

        var teams = await _context.AgencyTeams
            .Where(t => t.DeletedAt == null)
            .ToListAsync();

        var result = new List<SiteComparison>();

        foreach (var site in sites)
        {
            var siteUsers = users.Where(u => u.AgencySiteId == site.Id).ToList();
            var siteTeams = teams.Where(t => t.AgencySiteId == site.Id).ToList();

            var tenures = siteUsers
                .Where(u => u.AgencyStartDate.HasValue)
                .Select(u => (DateTime.UtcNow - u.AgencyStartDate!.Value).TotalDays / 365.0)
                .ToList();

            result.Add(new SiteComparison
            {
                SiteId = site.Id.ToString(),
                SiteName = site.Name,
                TotalAgents = siteUsers.Count,
                ActiveAgents = siteUsers.Count(u => u.AgentStatus == AgentStatusEnum.Active),
                TrainingAgents = siteUsers.Count(u => u.AgentType == AgentTypeEnum.Training),
                TeamCount = siteTeams.Count,
                AverageTenure = tenures.Any() ? tenures.Average() : 0,
                Tier1Count = siteUsers.Count(u => u.CommissionTier == CommissionTierEnum.Tier1),
                Tier2Count = siteUsers.Count(u => u.CommissionTier == CommissionTierEnum.Tier2),
                Tier3Count = siteUsers.Count(u => u.CommissionTier == CommissionTierEnum.Tier3)
            });
        }

        return result.OrderBy(s => s.SiteName).ToList();
    }

    public async Task<List<TeamCompositionData>> GetTeamCompositionAsync()
    {
        var teams = await _context.AgencyTeams
            .Include(t => t.AgencySite)
            .Include(t => t.Manager)
            .Where(t => t.DeletedAt == null)
            .ToListAsync();

        var users = await _context.Users
            .Where(u => u.DeletedAt == null)
            .ToListAsync();

        var result = new List<TeamCompositionData>();

        foreach (var team in teams)
        {
            var teamUsers = users.Where(u => u.AgencyTeamId == team.Id).ToList();
            var total = teamUsers.Count;

            var tenures = teamUsers
                .Where(u => u.AgencyStartDate.HasValue)
                .Select(u => (DateTime.UtcNow - u.AgencyStartDate!.Value).TotalDays / 365.0)
                .ToList();

            result.Add(new TeamCompositionData
            {
                TeamId = team.Id.ToString(),
                TeamName = team.Name,
                SiteName = team.AgencySite?.Name ?? "Unknown",
                ManagerName = team.Manager?.Name ?? "No Manager",
                MemberCount = total,
                ActiveCount = teamUsers.Count(u => u.AgentStatus == AgentStatusEnum.Active),
                TrainingCount = teamUsers.Count(u => u.AgentType == AgentTypeEnum.Training),
                AverageTenure = tenures.Any() ? tenures.Average() : 0,
                Tier1Percent = total > 0 ? (double)teamUsers.Count(u => u.CommissionTier == CommissionTierEnum.Tier1) / total * 100 : 0,
                Tier2Percent = total > 0 ? (double)teamUsers.Count(u => u.CommissionTier == CommissionTierEnum.Tier2) / total * 100 : 0,
                Tier3Percent = total > 0 ? (double)teamUsers.Count(u => u.CommissionTier == CommissionTierEnum.Tier3) / total * 100 : 0
            });
        }

        return result.OrderBy(t => t.SiteName).ThenBy(t => t.TeamName).ToList();
    }

    public async Task<TierDistributionData> GetTierDistributionAsync()
    {
        var users = await _context.Users
            .Include(u => u.AgencySite)
            .Where(u => u.DeletedAt == null)
            .ToListAsync();

        var sites = await _context.AgencySites
            .Where(s => s.DeletedAt == null)
            .ToListAsync();

        var bySite = sites.Select(site =>
        {
            var siteUsers = users.Where(u => u.AgencySiteId == site.Id).ToList();
            return new TierBySite
            {
                SiteName = site.Name,
                Tier1 = siteUsers.Count(u => u.CommissionTier == CommissionTierEnum.Tier1),
                Tier2 = siteUsers.Count(u => u.CommissionTier == CommissionTierEnum.Tier2),
                Tier3 = siteUsers.Count(u => u.CommissionTier == CommissionTierEnum.Tier3)
            };
        }).ToList();

        return new TierDistributionData
        {
            Tier1 = users.Count(u => u.CommissionTier == CommissionTierEnum.Tier1),
            Tier2 = users.Count(u => u.CommissionTier == CommissionTierEnum.Tier2),
            Tier3 = users.Count(u => u.CommissionTier == CommissionTierEnum.Tier3),
            None = users.Count(u => u.CommissionTier == CommissionTierEnum.None),
            BySite = bySite
        };
    }

    public async Task<TenureDistributionData> GetTenureDistributionAsync()
    {
        var users = await _context.Users
            .Where(u => u.DeletedAt == null && u.AgencyStartDate != null)
            .ToListAsync();

        var tenures = users.Select(u => (DateTime.UtcNow - u.AgencyStartDate!.Value).TotalDays / 365.0).ToList();
        var total = tenures.Count;

        var ranges = new List<TenureRange>
        {
            new() { Label = "< 3 months", Count = tenures.Count(t => t < 0.25), Percentage = total > 0 ? tenures.Count(t => t < 0.25) * 100.0 / total : 0 },
            new() { Label = "3-6 months", Count = tenures.Count(t => t >= 0.25 && t < 0.5), Percentage = total > 0 ? tenures.Count(t => t >= 0.25 && t < 0.5) * 100.0 / total : 0 },
            new() { Label = "6-12 months", Count = tenures.Count(t => t >= 0.5 && t < 1), Percentage = total > 0 ? tenures.Count(t => t >= 0.5 && t < 1) * 100.0 / total : 0 },
            new() { Label = "1-2 years", Count = tenures.Count(t => t >= 1 && t < 2), Percentage = total > 0 ? tenures.Count(t => t >= 1 && t < 2) * 100.0 / total : 0 },
            new() { Label = "2-5 years", Count = tenures.Count(t => t >= 2 && t < 5), Percentage = total > 0 ? tenures.Count(t => t >= 2 && t < 5) * 100.0 / total : 0 },
            new() { Label = "5+ years", Count = tenures.Count(t => t >= 5), Percentage = total > 0 ? tenures.Count(t => t >= 5) * 100.0 / total : 0 }
        };

        var managers = users.Where(u => u.Title == TitleEnum.Manager).ToList();
        var agents = users.Where(u => u.Title == TitleEnum.Agent).ToList();

        var avgByRole = new List<TenureByRole>
        {
            new()
            {
                Role = "Managers",
                AverageTenure = managers.Any() 
                    ? managers.Average(u => (DateTime.UtcNow - u.AgencyStartDate!.Value).TotalDays / 365.0) 
                    : 0
            },
            new()
            {
                Role = "Agents",
                AverageTenure = agents.Any() 
                    ? agents.Average(u => (DateTime.UtcNow - u.AgencyStartDate!.Value).TotalDays / 365.0) 
                    : 0
            }
        };

        return new TenureDistributionData
        {
            Ranges = ranges,
            AverageByRole = avgByRole
        };
    }

    public async Task<HierarchyDepthData> GetHierarchyAnalysisAsync()
    {
        var sites = await _context.AgencySites.Where(s => s.DeletedAt == null).CountAsync();
        var teams = await _context.AgencyTeams.Where(t => t.DeletedAt == null).ToListAsync();
        var users = await _context.Users.Where(u => u.DeletedAt == null).ToListAsync();

        var distribution = new List<HierarchyLevel>
        {
            new() { Level = 1, Label = "Sites", Count = sites },
            new() { Level = 2, Label = "Teams", Count = teams.Count },
            new() { Level = 3, Label = "Managers", Count = users.Count(u => u.Title == TitleEnum.Manager) },
            new() { Level = 4, Label = "Agents", Count = users.Count(u => u.Title == TitleEnum.Agent) }
        };

        // Calculate span of control for managers
        var spanOfControl = new List<SpanOfControl>();
        var managerIds = teams.Where(t => t.ManagerUserId != null).Select(t => t.ManagerUserId!).Distinct();

        foreach (var managerId in managerIds)
        {
            var manager = users.FirstOrDefault(u => u.Id == managerId);
            if (manager == null) continue;

            var managedTeams = teams.Where(t => t.ManagerUserId == managerId).ToList();
            var directReports = users.Count(u => managedTeams.Any(t => t.Id == u.AgencyTeamId) && u.Id != managerId);

            spanOfControl.Add(new SpanOfControl
            {
                ManagerId = managerId,
                ManagerName = manager.Name ?? manager.Email ?? managerId,
                DirectReports = directReports,
                TotalReports = directReports // In this flat structure, direct = total
            });
        }

        return new HierarchyDepthData
        {
            TotalLevels = 4,
            Distribution = distribution,
            SpanOfControl = spanOfControl.OrderByDescending(s => s.DirectReports).Take(15).ToList()
        };
    }

    public async Task<List<GrowthTrendData>> GetGrowthTrendAsync()
    {
        var users = await _context.Users
            .Where(u => u.DeletedAt == null && u.AgencyStartDate != null)
            .OrderBy(u => u.AgencyStartDate)
            .ToListAsync();

        // Group by month
        var grouped = users
            .GroupBy(u => new { u.AgencyStartDate!.Value.Year, u.AgencyStartDate.Value.Month })
            .OrderBy(g => g.Key.Year)
            .ThenBy(g => g.Key.Month)
            .ToList();

        var result = new List<GrowthTrendData>();
        var cumulative = 0;

        foreach (var group in grouped)
        {
            cumulative += group.Count();
            result.Add(new GrowthTrendData
            {
                Period = $"{group.Key.Year}-{group.Key.Month:D2}",
                HireCount = group.Count(),
                CumulativeTotal = cumulative
            });
        }

        // Take last 12 periods or all if less
        return result.TakeLast(12).ToList();
    }
}
