using LuminaryLife.Common.Data;
using LuminaryLife.Common.Entities;
using LuminaryLife.Common.Enums;
using LuminaryLife.Common.Services;
using LuminaryLife.Common.Systems.Tags.Models;
using LuminaryLife.Common.Systems.Tags.Services;
using Microsoft.EntityFrameworkCore;

namespace LuminaryLife.Common.Systems.OrgChart.Queries;

/// <summary>
/// Optimized graph traversal service with caching and efficient queries
/// Implements CQRS read-side with sub-second latency targets
/// </summary>
public class GraphTraversalService : IGraphTraversalService
{
    private readonly CoreApiEfDbContext _context;
    private readonly ITagService _tagService;
    private readonly ICacheService _cache;
    private static readonly TimeSpan ShortCache = TimeSpan.FromMinutes(1);
    private static readonly TimeSpan LongCache = TimeSpan.FromMinutes(10);

    public GraphTraversalService(
        CoreApiEfDbContext context,
        ITagService tagService,
        ICacheService cache)
    {
        _context = context;
        _tagService = tagService;
        _cache = cache;
    }

    public async Task<List<GraphNodeProjection>> GetHierarchyOverviewAsync(GraphQuery query)
    {
        var cacheKey = $"hierarchy:{query.GetCacheKey()}";
        var cached = await _cache.GetAsync<List<GraphNodeProjection>>(cacheKey);
        if (cached != null) return cached;

        var result = new List<GraphNodeProjection>();

        // Load sites with pre-aggregated stats
        var sites = await _context.AgencySites
            .Where(s => s.DeletedAt == null)
            .Where(s => !query.SiteId.HasValue || s.Id == query.SiteId.Value)
            .OrderBy(s => s.Name)
            .ToListAsync();

        foreach (var site in sites)
        {
            var siteNode = await BuildSiteProjectionAsync(site, query);
            result.Add(siteNode);

            // If not shallow load, include teams
            if (!query.ShallowLoad)
            {
                var teamNodes = await GetTeamProjectionsAsync(site.Id, query);
                result.AddRange(teamNodes);
            }
        }

        await _cache.SetAsync(cacheKey, result, ShortCache);
        return result;
    }

    public async Task<List<GraphNodeProjection>> GetNodeChildrenAsync(string nodeId, GraphQuery query)
    {
        var cacheKey = $"children:{nodeId}:{query.GetCacheKey()}";
        var cached = await _cache.GetAsync<List<GraphNodeProjection>>(cacheKey);
        if (cached != null) return cached;

        var result = new List<GraphNodeProjection>();

        if (nodeId.StartsWith("site-"))
        {
            var siteId = int.Parse(nodeId.Replace("site-", ""));
            result = await GetTeamProjectionsAsync(siteId, query);
        }
        else if (nodeId.StartsWith("team-"))
        {
            var teamId = int.Parse(nodeId.Replace("team-", ""));
            result = await GetUserProjectionsAsync(teamId, query);
        }

        await _cache.SetAsync(cacheKey, result, ShortCache);
        return result;
    }

    public async Task<GraphNodeDetail?> GetNodeDetailAsync(string nodeId)
    {
        var cacheKey = $"detail:{nodeId}";
        var cached = await _cache.GetAsync<GraphNodeDetail>(cacheKey);
        if (cached != null) return cached;

        GraphNodeDetail? detail = null;

        if (nodeId.StartsWith("site-"))
        {
            detail = await BuildSiteDetailAsync(nodeId);
        }
        else if (nodeId.StartsWith("team-"))
        {
            detail = await BuildTeamDetailAsync(nodeId);
        }
        else
        {
            detail = await BuildUserDetailAsync(nodeId);
        }

        if (detail != null)
        {
            await _cache.SetAsync(cacheKey, detail, ShortCache);
        }

        return detail;
    }

    public async Task<GraphNodeProjection?> GetSubtreeAsync(string nodeId, int maxDepth = 2)
    {
        var detail = await GetNodeDetailAsync(nodeId);
        if (detail == null) return null;

        var node = new GraphNodeProjection
        {
            Id = detail.Id,
            NodeType = detail.NodeType,
            Name = detail.Name,
            ParentId = detail.ParentId,
            Depth = detail.Depth,
            ChildCount = detail.DirectReports.Count,
            TotalDescendants = detail.TotalDescendants,
            HasChildren = detail.DirectReports.Count > 0,
            Stats = detail.Stats,
            Visuals = detail.Visuals
        };

        return node;
    }

    public async Task<List<GraphNodeProjection>> SearchNodesAsync(string searchTerm, int limit = 20)
    {
        var term = searchTerm.ToLower();

        var users = await _context.Users
            .Where(u => u.DeletedAt == null)
            .Where(u => 
                (u.Name != null && u.Name.ToLower().Contains(term)) ||
                (u.Email != null && u.Email.ToLower().Contains(term)))
            .OrderBy(u => u.Name)
            .Take(limit)
            .ToListAsync();

        var results = new List<GraphNodeProjection>();
        foreach (var user in users)
        {
            results.Add(BuildUserProjection(user));
        }

        return results;
    }

    public async Task<GraphNodeStats> GetAggregateStatsAsync(string? rootNodeId = null)
    {
        var cacheKey = $"stats:{rootNodeId ?? "all"}";
        var cached = await _cache.GetAsync<GraphNodeStats>(cacheKey);
        if (cached != null) return cached;

        var usersQuery = _context.Users.Where(u => u.DeletedAt == null);

        if (rootNodeId != null)
        {
            if (rootNodeId.StartsWith("site-"))
            {
                var siteId = int.Parse(rootNodeId.Replace("site-", ""));
                usersQuery = usersQuery.Where(u => u.AgencySiteId == siteId);
            }
            else if (rootNodeId.StartsWith("team-"))
            {
                var teamId = int.Parse(rootNodeId.Replace("team-", ""));
                usersQuery = usersQuery.Where(u => u.AgencyTeamId == teamId);
            }
        }

        var users = await usersQuery.ToListAsync();
        var stats = CalculateStats(users);

        await _cache.SetAsync(cacheKey, stats, ShortCache);
        return stats;
    }

    public async Task InvalidateCacheAsync(string? nodeId = null)
    {
        // In a production system, this would clear specific cache entries
        // For now, the in-memory cache will handle expiration naturally
        await Task.CompletedTask;
    }

    // Private helper methods

    private async Task<GraphNodeProjection> BuildSiteProjectionAsync(AgencySite site, GraphQuery query)
    {
        var usersQuery = _context.Users
            .Where(u => u.AgencySiteId == site.Id && u.DeletedAt == null);

        if (!query.IncludeInactive)
            usersQuery = usersQuery.Where(u => u.AgentStatus == AgentStatusEnum.Active);

        var users = await usersQuery.ToListAsync();
        var stats = CalculateStats(users);

        var teamCount = await _context.AgencyTeams
            .CountAsync(t => t.AgencySiteId == site.Id && t.DeletedAt == null);

        return new GraphNodeProjection
        {
            Id = $"site-{site.Id}",
            NodeType = "Site",
            Name = site.Name,
            ParentId = "root",
            Depth = 1,
            ChildCount = teamCount,
            TotalDescendants = users.Count + teamCount,
            HasChildren = teamCount > 0,
            Stats = stats,
            Visuals = CalculateVisuals(stats, "Site")
        };
    }

    private async Task<List<GraphNodeProjection>> GetTeamProjectionsAsync(int siteId, GraphQuery query)
    {
        var teams = await _context.AgencyTeams
            .Include(t => t.Manager)
            .Where(t => t.AgencySiteId == siteId && t.DeletedAt == null)
            .Where(t => !query.TeamId.HasValue || t.Id == query.TeamId.Value)
            .Where(t => string.IsNullOrEmpty(query.ManagerId) || t.ManagerUserId == query.ManagerId)
            .OrderBy(t => t.Name)
            .ToListAsync();

        var result = new List<GraphNodeProjection>();

        foreach (var team in teams)
        {
            var usersQuery = _context.Users
                .Where(u => u.AgencyTeamId == team.Id && u.DeletedAt == null);

            if (!query.IncludeInactive)
                usersQuery = usersQuery.Where(u => u.AgentStatus == AgentStatusEnum.Active);

            var users = await usersQuery.ToListAsync();
            var stats = CalculateStats(users);

            result.Add(new GraphNodeProjection
            {
                Id = $"team-{team.Id}",
                NodeType = "Team",
                Name = team.Name,
                ParentId = $"site-{siteId}",
                Depth = 2,
                ChildCount = users.Count,
                TotalDescendants = users.Count,
                HasChildren = users.Count > 0,
                Stats = stats,
                Visuals = CalculateVisuals(stats, "Team")
            });
        }

        return result;
    }

    private async Task<List<GraphNodeProjection>> GetUserProjectionsAsync(int teamId, GraphQuery query)
    {
        var usersQuery = _context.Users
            .Where(u => u.AgencyTeamId == teamId && u.DeletedAt == null);

        if (!query.IncludeInactive)
            usersQuery = usersQuery.Where(u => u.AgentStatus == AgentStatusEnum.Active);

        if (!string.IsNullOrEmpty(query.SearchTerm))
        {
            var term = query.SearchTerm.ToLower();
            usersQuery = usersQuery.Where(u =>
                (u.Name != null && u.Name.ToLower().Contains(term)) ||
                (u.Email != null && u.Email.ToLower().Contains(term)));
        }

        var users = await usersQuery.OrderBy(u => u.Name).ToListAsync();

        // Check which users are managers
        var managerIds = await _context.AgencyTeams
            .Where(t => t.DeletedAt == null && t.ManagerUserId != null)
            .Select(t => t.ManagerUserId!)
            .ToListAsync();

        return users.Select(u =>
        {
            var isManager = managerIds.Contains(u.Id);
            return BuildUserProjection(u, isManager);
        }).ToList();
    }

    private GraphNodeProjection BuildUserProjection(User user, bool isManager = false)
    {
        var tenureYears = user.AgencyStartDate.HasValue
            ? (DateTime.UtcNow - user.AgencyStartDate.Value).TotalDays / 365.0
            : 0;

        var nodeType = isManager ? "Manager" : "Agent";
        var stats = new GraphNodeStats
        {
            ActiveAgents = user.AgentStatus == AgentStatusEnum.Active ? 1 : 0,
            InactiveAgents = user.AgentStatus == AgentStatusEnum.Inactive ? 1 : 0,
            TrainingAgents = user.AgentType == AgentTypeEnum.Training ? 1 : 0,
            AverageTenureYears = tenureYears,
            TierDistribution = new Dictionary<string, int>
            {
                { user.CommissionTier.ToString(), 1 }
            }
        };

        var visuals = new GraphNodeVisuals
        {
            PrimaryColor = GetTierColor(user.CommissionTier),
            SecondaryColor = user.AgentStatus == AgentStatusEnum.Active ? "#dcfce7" : "#f3f4f6",
            StatusIndicator = GetStatusIndicator(user),
            HealthScore = user.AgentStatus == AgentStatusEnum.Active ? 1.0 : 0.3,
            TenureBadge = GetTenureBadge(tenureYears),
            PerformanceBadge = user.AgentType == AgentTypeEnum.Training ? "Training" : null
        };

        return new GraphNodeProjection
        {
            Id = user.Id,
            NodeType = nodeType,
            Name = user.Name ?? user.Email ?? user.Id,
            ParentId = user.AgencyTeamId.HasValue ? $"team-{user.AgencyTeamId}" : null,
            Depth = 3,
            ChildCount = 0,
            TotalDescendants = 0,
            HasChildren = false,
            Stats = stats,
            Visuals = visuals
        };
    }

    private async Task<GraphNodeDetail?> BuildSiteDetailAsync(string nodeId)
    {
        var siteId = int.Parse(nodeId.Replace("site-", ""));
        var site = await _context.AgencySites.FindAsync(siteId);
        if (site == null) return null;

        var users = await _context.Users
            .Where(u => u.AgencySiteId == siteId && u.DeletedAt == null)
            .ToListAsync();

        var teams = await _context.AgencyTeams
            .Where(t => t.AgencySiteId == siteId && t.DeletedAt == null)
            .ToListAsync();

        var tags = await _tagService.GetTagsByEntityIdAsync(EntityTypeEnum.AgencySite, siteId);
        var stats = CalculateStats(users);

        return new GraphNodeDetail
        {
            Id = nodeId,
            NodeType = "Site",
            Name = site.Name,
            ParentId = "root",
            Depth = 1,
            ChildCount = teams.Count,
            TotalDescendants = users.Count + teams.Count,
            HasChildren = teams.Count > 0,
            Stats = stats,
            Visuals = CalculateVisuals(stats, "Site"),
            Metadata = new GraphNodeMetadata
            {
                SiteId = siteId.ToString(),
                SiteName = site.Name,
                City = site.City,
                State = site.State,
                Phone = site.Phone
            },
            Tags = tags.Select(t => new GraphTagInfo
            {
                Id = t.Id,
                Name = t.Name,
                HexColor = t.HexColorCode ?? "808080",
                Category = GetTagCategory(t.Name)
            }).ToList(),
            Breadcrumbs = new[] { new GraphNodeBreadcrumb { Id = "root", Name = "Organization", NodeType = "Root" } }
        };
    }

    private async Task<GraphNodeDetail?> BuildTeamDetailAsync(string nodeId)
    {
        var teamId = int.Parse(nodeId.Replace("team-", ""));
        var team = await _context.AgencyTeams
            .Include(t => t.AgencySite)
            .Include(t => t.Manager)
            .FirstOrDefaultAsync(t => t.Id == teamId);
            
        if (team == null) return null;

        var users = await _context.Users
            .Where(u => u.AgencyTeamId == teamId && u.DeletedAt == null)
            .OrderBy(u => u.Name)
            .ToListAsync();

        var tags = await _tagService.GetTagsByEntityIdAsync(EntityTypeEnum.AgencyTeam, teamId);
        var stats = CalculateStats(users);

        var managerIds = await _context.AgencyTeams
            .Where(t => t.DeletedAt == null && t.ManagerUserId != null)
            .Select(t => t.ManagerUserId!)
            .ToListAsync();

        return new GraphNodeDetail
        {
            Id = nodeId,
            NodeType = "Team",
            Name = team.Name,
            ParentId = team.AgencySiteId.HasValue ? $"site-{team.AgencySiteId}" : null,
            Depth = 2,
            ChildCount = users.Count,
            TotalDescendants = users.Count,
            HasChildren = users.Count > 0,
            Stats = stats,
            Visuals = CalculateVisuals(stats, "Team"),
            Metadata = new GraphNodeMetadata
            {
                SiteId = team.AgencySiteId?.ToString(),
                SiteName = team.AgencySite?.Name,
                TeamId = teamId.ToString(),
                TeamName = team.Name
            },
            Tags = tags.Select(t => new GraphTagInfo
            {
                Id = t.Id,
                Name = t.Name,
                HexColor = t.HexColorCode ?? "808080",
                Category = GetTagCategory(t.Name)
            }).ToList(),
            DirectReports = users.Select(u => BuildUserProjection(u, managerIds.Contains(u.Id))).ToList(),
            Breadcrumbs = new[]
            {
                new GraphNodeBreadcrumb { Id = "root", Name = "Organization", NodeType = "Root" },
                new GraphNodeBreadcrumb { Id = $"site-{team.AgencySiteId}", Name = team.AgencySite?.Name ?? "Site", NodeType = "Site" }
            }
        };
    }

    private async Task<GraphNodeDetail?> BuildUserDetailAsync(string userId)
    {
        var user = await _context.Users
            .Include(u => u.AgencyTeam)
            .ThenInclude(t => t!.AgencySite)
            .FirstOrDefaultAsync(u => u.Id == userId && u.DeletedAt == null);

        if (user == null) return null;

        var tags = await _tagService.GetTagsByEntityIdAsync(EntityTypeEnum.User, userId);
        var isManager = await _context.AgencyTeams.AnyAsync(t => t.ManagerUserId == userId && t.DeletedAt == null);

        var tenureYears = user.AgencyStartDate.HasValue
            ? (DateTime.UtcNow - user.AgencyStartDate.Value).TotalDays / 365.0
            : 0;

        // Get peers (same team, excluding self)
        var peers = new List<GraphNodeProjection>();
        GraphNodeProjection? manager = null;

        if (user.AgencyTeamId.HasValue)
        {
            var teamMembers = await _context.Users
                .Where(u => u.AgencyTeamId == user.AgencyTeamId && u.Id != userId && u.DeletedAt == null)
                .Take(10)
                .ToListAsync();

            var managerIds = await _context.AgencyTeams
                .Where(t => t.DeletedAt == null && t.ManagerUserId != null)
                .Select(t => t.ManagerUserId!)
                .ToListAsync();

            peers = teamMembers.Select(u => BuildUserProjection(u, managerIds.Contains(u.Id))).ToList();

            // Get manager
            var team = await _context.AgencyTeams
                .Include(t => t.Manager)
                .FirstOrDefaultAsync(t => t.Id == user.AgencyTeamId);

            if (team?.Manager != null && team.Manager.Id != userId)
            {
                manager = BuildUserProjection(team.Manager, true);
            }
        }

        var stats = new GraphNodeStats
        {
            ActiveAgents = user.AgentStatus == AgentStatusEnum.Active ? 1 : 0,
            InactiveAgents = user.AgentStatus == AgentStatusEnum.Inactive ? 1 : 0,
            TrainingAgents = user.AgentType == AgentTypeEnum.Training ? 1 : 0,
            AverageTenureYears = tenureYears,
            TierDistribution = new Dictionary<string, int> { { user.CommissionTier.ToString(), 1 } }
        };

        var visuals = new GraphNodeVisuals
        {
            PrimaryColor = GetTierColor(user.CommissionTier),
            SecondaryColor = user.AgentStatus == AgentStatusEnum.Active ? "#dcfce7" : "#f3f4f6",
            StatusIndicator = GetStatusIndicator(user),
            HealthScore = user.AgentStatus == AgentStatusEnum.Active ? 1.0 : 0.3,
            TenureBadge = GetTenureBadge(tenureYears),
            PerformanceBadge = user.AgentType == AgentTypeEnum.Training ? "Training" : null
        };

        var breadcrumbs = new List<GraphNodeBreadcrumb>
        {
            new() { Id = "root", Name = "Organization", NodeType = "Root" }
        };

        if (user.AgencyTeam?.AgencySite != null)
        {
            breadcrumbs.Add(new GraphNodeBreadcrumb
            {
                Id = $"site-{user.AgencyTeam.AgencySiteId}",
                Name = user.AgencyTeam.AgencySite.Name,
                NodeType = "Site"
            });
        }

        if (user.AgencyTeam != null)
        {
            breadcrumbs.Add(new GraphNodeBreadcrumb
            {
                Id = $"team-{user.AgencyTeam.Id}",
                Name = user.AgencyTeam.Name,
                NodeType = "Team"
            });
        }

        return new GraphNodeDetail
        {
            Id = userId,
            NodeType = isManager ? "Manager" : "Agent",
            Name = user.Name ?? user.Email ?? userId,
            ParentId = user.AgencyTeamId.HasValue ? $"team-{user.AgencyTeamId}" : null,
            Depth = 3,
            ChildCount = 0,
            TotalDescendants = 0,
            HasChildren = false,
            Stats = stats,
            Visuals = visuals,
            Metadata = new GraphNodeMetadata
            {
                SiteId = user.AgencyTeam?.AgencySiteId?.ToString(),
                SiteName = user.AgencyTeam?.AgencySite?.Name,
                TeamId = user.AgencyTeamId?.ToString(),
                TeamName = user.AgencyTeam?.Name,
                Title = user.Title.ToString(),
                AgentStatus = user.AgentStatus.ToString(),
                AgentType = user.AgentType.ToString(),
                CommissionTier = user.CommissionTier.ToString(),
                StartDate = user.AgencyStartDate,
                TenureYears = tenureYears,
                Email = user.Email,
                Phone = user.Phone,
                ImageUrl = user.ImageAvatarUrl
            },
            Tags = tags.Select(t => new GraphTagInfo
            {
                Id = t.Id,
                Name = t.Name,
                HexColor = t.HexColorCode ?? "808080",
                Category = GetTagCategory(t.Name)
            }).ToList(),
            Manager = manager,
            Peers = peers,
            Breadcrumbs = breadcrumbs.ToArray()
        };
    }

    private static GraphNodeStats CalculateStats(List<User> users)
    {
        var tierGroups = users.GroupBy(u => u.CommissionTier.ToString())
            .ToDictionary(g => g.Key, g => g.Count());

        var avgTenure = users
            .Where(u => u.AgencyStartDate.HasValue)
            .Select(u => (DateTime.UtcNow - u.AgencyStartDate!.Value).TotalDays / 365.0)
            .DefaultIfEmpty(0)
            .Average();

        return new GraphNodeStats
        {
            ActiveAgents = users.Count(u => u.AgentStatus == AgentStatusEnum.Active),
            InactiveAgents = users.Count(u => u.AgentStatus == AgentStatusEnum.Inactive),
            TrainingAgents = users.Count(u => u.AgentType == AgentTypeEnum.Training),
            ManagerCount = users.Count(u => u.Title == TitleEnum.Manager),
            AverageTenureYears = avgTenure,
            TierDistribution = tierGroups
        };
    }

    private static GraphNodeVisuals CalculateVisuals(GraphNodeStats stats, string nodeType)
    {
        var total = stats.ActiveAgents + stats.InactiveAgents;
        var healthScore = total > 0 ? (double)stats.ActiveAgents / total : 1.0;
        
        var statusIndicator = healthScore switch
        {
            >= 0.9 => "healthy",
            >= 0.7 => "warning",
            _ => "critical"
        };

        var primaryColor = nodeType switch
        {
            "Site" => "#0ea5e9",
            "Team" => "#22c55e",
            _ => "#6b7280"
        };

        return new GraphNodeVisuals
        {
            PrimaryColor = primaryColor,
            SecondaryColor = "#e5e7eb",
            StatusIndicator = statusIndicator,
            HealthScore = healthScore,
            TenureBadge = stats.AverageTenureYears >= 2 ? "Experienced" : "Growing"
        };
    }

    private static string GetTierColor(CommissionTierEnum tier) => tier switch
    {
        CommissionTierEnum.Tier3 => "#8b5cf6", // Purple - top tier
        CommissionTierEnum.Tier2 => "#3b82f6", // Blue - mid tier
        CommissionTierEnum.Tier1 => "#22c55e", // Green - entry tier
        _ => "#6b7280" // Gray - none
    };

    private static string GetStatusIndicator(User user)
    {
        if (user.AgentStatus == AgentStatusEnum.Inactive) return "inactive";
        if (user.AgentType == AgentTypeEnum.Training) return "training";
        return "active";
    }

    private static string? GetTenureBadge(double years) => years switch
    {
        >= 5 => "5yr+",
        >= 2 => "2yr+",
        >= 1 => "1yr+",
        < 0.25 => "New",
        _ => null
    };

    private static string GetTagCategory(string tagName) => tagName.ToLower() switch
    {
        "active" or "inactive" => "status",
        "training" or "performance" => "type",
        "tier 1" or "tier 2" or "tier 3" or "tier1" or "tier2" or "tier3" => "tier",
        "new hire" or "tenured" => "tenure",
        "manager" => "role",
        _ => "general"
    };
}
