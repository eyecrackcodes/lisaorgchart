using LuminaryLife.Common.Data;
using LuminaryLife.Common.Entities;
using LuminaryLife.Common.Enums;
using LuminaryLife.Common.Services;
using LuminaryLife.Common.Systems.OrgChart.Models;
using LuminaryLife.Common.Systems.Tags.Models;
using LuminaryLife.Common.Systems.Tags.Services;
using Microsoft.EntityFrameworkCore;

namespace LuminaryLife.Common.Systems.OrgChart.Services;

/// <summary>
/// Service implementation for org chart operations
/// </summary>
public class OrgChartService : IOrgChartService
{
    private readonly CoreApiEfDbContext _context;
    private readonly ITagService _tagService;
    private readonly ICacheService _cacheService;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    public OrgChartService(
        CoreApiEfDbContext context,
        ITagService tagService,
        ICacheService cacheService)
    {
        _context = context;
        _tagService = tagService;
        _cacheService = cacheService;
    }

    public async Task<OrgChartNodeDto> GetOrgChartTreeAsync(OrgChartQueryParams query)
    {
        var cacheKey = $"org-chart:tree:{query.GetCacheKey()}";
        var cached = await _cacheService.GetAsync<OrgChartNodeDto>(cacheKey);
        if (cached != null) return cached;

        var sites = await _context.AgencySites
            .Where(s => s.DeletedAt == null)
            .OrderBy(s => s.Name)
            .ToListAsync();

        if (query.SiteId.HasValue)
            sites = sites.Where(s => s.Id == query.SiteId.Value).ToList();

        var root = new OrgChartNodeDto
        {
            Id = "root",
            NodeType = "Root",
            Name = "Organization",
            Depth = 0,
            Children = new List<OrgChartNodeDto>()
        };

        foreach (var site in sites)
        {
            var siteNode = await BuildSiteNodeAsync(site, query);
            if (siteNode != null)
            {
                root.Children.Add(siteNode);
                root.TotalReportCount += siteNode.TotalReportCount;
            }
        }

        root.DirectReportCount = root.Children.Count;

        await _cacheService.SetAsync(cacheKey, root, CacheDuration);
        return root;
    }

    public async Task<List<OrgChartNodeDto>> GetOrgChartFlatAsync(OrgChartQueryParams query)
    {
        var tree = await GetOrgChartTreeAsync(query);
        var flat = new List<OrgChartNodeDto>();
        FlattenTree(tree, flat);
        return flat;
    }

    public async Task<OrgChartNodeDto?> GetOrgChartSubtreeAsync(
        int? siteId, 
        int? teamId, 
        string? managerId, 
        OrgChartQueryParams query)
    {
        query.SiteId = siteId;
        query.TeamId = teamId;
        query.ManagerId = managerId;
        
        var tree = await GetOrgChartTreeAsync(query);
        
        // If filtering by team or manager, return first matching subtree
        if (teamId.HasValue || !string.IsNullOrEmpty(managerId))
        {
            return FindFirstMatchingNode(tree, node =>
                (teamId.HasValue && node.Id == $"team-{teamId}") ||
                (!string.IsNullOrEmpty(managerId) && node.ManagerId == managerId));
        }
        
        // If filtering by site, return the site node
        if (siteId.HasValue && tree.Children.Count == 1)
        {
            return tree.Children[0];
        }
        
        return tree;
    }

    public async Task<OrgChartNodeDto?> GetPersonNodeAsync(string userId)
    {
        var user = await _context.Users
            .Include(u => u.AgencyTeam)
            .Include(u => u.AgencySite)
            .FirstOrDefaultAsync(u => u.Id == userId && u.DeletedAt == null);

        if (user == null) return null;

        var tags = await _tagService.GetTagsByEntityIdAsync(EntityTypeEnum.User, userId);
        var isManager = await _context.AgencyTeams
            .AnyAsync(t => t.ManagerUserId == userId && t.DeletedAt == null);

        return BuildUserNode(user, user.AgencyTeam, isManager ? "Manager" : "Agent", 
            new Dictionary<string, List<TagResponse>> { { userId, tags } });
    }

    public async Task<OrgChartFilterOptions> GetFilterOptionsAsync()
    {
        var sites = await _context.AgencySites
            .Where(s => s.DeletedAt == null)
            .OrderBy(s => s.Name)
            .Select(s => new FilterOption { Id = s.Id.ToString(), Name = s.Name })
            .ToListAsync();

        var teams = await _context.AgencyTeams
            .Where(t => t.DeletedAt == null)
            .OrderBy(t => t.Name)
            .Select(t => new FilterOption { Id = t.Id.ToString(), Name = t.Name })
            .ToListAsync();

        var managers = await _context.AgencyTeams
            .Where(t => t.DeletedAt == null && t.ManagerUserId != null)
            .Include(t => t.Manager)
            .Where(t => t.Manager != null)
            .Select(t => new FilterOption 
            { 
                Id = t.ManagerUserId!, 
                Name = t.Manager!.Name ?? t.Manager.Email ?? t.ManagerUserId! 
            })
            .Distinct()
            .OrderBy(m => m.Name)
            .ToListAsync();

        var tags = await _tagService.GetAllTagsAsync();

        return new OrgChartFilterOptions
        {
            Sites = sites,
            Teams = teams,
            Managers = managers,
            Tags = tags
        };
    }

    private async Task<OrgChartNodeDto?> BuildSiteNodeAsync(AgencySite site, OrgChartQueryParams query)
    {
        var teams = await _context.AgencyTeams
            .Include(t => t.Manager)
            .Where(t => t.AgencySiteId == site.Id && t.DeletedAt == null)
            .OrderBy(t => t.Name)
            .ToListAsync();

        if (query.TeamId.HasValue)
            teams = teams.Where(t => t.Id == query.TeamId.Value).ToList();
        if (!string.IsNullOrEmpty(query.ManagerId))
            teams = teams.Where(t => t.ManagerUserId == query.ManagerId).ToList();

        var siteNode = new OrgChartNodeDto
        {
            Id = $"site-{site.Id}",
            NodeType = "Site",
            Name = site.Name,
            ParentId = "root",
            Depth = 1,
            Metadata = new OrgChartNodeMetadata
            {
                SiteId = site.Id.ToString(),
                SiteName = site.Name,
                City = site.City,
                State = site.State,
                Phone = site.Phone
            },
            Children = new List<OrgChartNodeDto>()
        };

        // Get site tags
        siteNode.Tags = await _tagService.GetTagsByEntityIdAsync(
            EntityTypeEnum.AgencySite, 
            site.Id);

        foreach (var team in teams)
        {
            var teamNode = await BuildTeamNodeAsync(team, site, query);
            if (teamNode != null)
            {
                siteNode.Children.Add(teamNode);
                siteNode.TotalReportCount += teamNode.TotalReportCount + 1;
            }
        }

        siteNode.DirectReportCount = siteNode.Children.Count;

        return siteNode;
    }

    private async Task<OrgChartNodeDto?> BuildTeamNodeAsync(
        AgencyTeam team, 
        AgencySite site, 
        OrgChartQueryParams query)
    {
        var usersQuery = _context.Users
            .Where(u => u.AgencyTeamId == team.Id && u.DeletedAt == null);

        if (!query.IncludeInactive)
            usersQuery = usersQuery.Where(u => u.AgentStatus == AgentStatusEnum.Active);

        if (!string.IsNullOrEmpty(query.SearchTerm))
        {
            var searchTerm = query.SearchTerm.ToLower();
            usersQuery = usersQuery.Where(u => 
                (u.Name != null && u.Name.ToLower().Contains(searchTerm)) ||
                (u.Email != null && u.Email.ToLower().Contains(searchTerm)));
        }

        var users = await usersQuery.OrderBy(u => u.Name).ToListAsync();

        var teamNode = new OrgChartNodeDto
        {
            Id = $"team-{team.Id}",
            NodeType = "Team",
            Name = team.Name,
            ParentId = $"site-{site.Id}",
            Depth = 2,
            ManagerId = team.ManagerUserId,
            Metadata = new OrgChartNodeMetadata
            {
                TeamId = team.Id.ToString(),
                TeamName = team.Name,
                SiteId = site.Id.ToString(),
                SiteName = site.Name
            },
            Children = new List<OrgChartNodeDto>()
        };

        // Get team tags
        teamNode.Tags = await _tagService.GetTagsByEntityIdAsync(
            EntityTypeEnum.AgencyTeam, 
            team.Id);

        // Bulk fetch tags for all users in the team
        var userTagMap = await _tagService.GetTagsForEntitiesAsync(
            EntityTypeEnum.User, 
            users.Select(u => u.Id).ToList());

        // Separate manager from agents
        var manager = users.FirstOrDefault(u => u.Id == team.ManagerUserId);
        var agents = users.Where(u => u.Id != team.ManagerUserId).ToList();

        // Add manager first if exists
        if (manager != null)
        {
            var managerNode = BuildUserNode(manager, team, "Manager", userTagMap);
            if (PassesTagFilter(managerNode, query.TagIds))
            {
                teamNode.Children.Add(managerNode);
            }
        }

        // Add agents
        foreach (var agent in agents)
        {
            var agentNode = BuildUserNode(agent, team, "Agent", userTagMap);
            if (PassesTagFilter(agentNode, query.TagIds))
            {
                teamNode.Children.Add(agentNode);
            }
        }

        teamNode.DirectReportCount = teamNode.Children.Count;
        teamNode.TotalReportCount = teamNode.Children.Count;

        return teamNode;
    }

    private static OrgChartNodeDto BuildUserNode(
        User user, 
        AgencyTeam? team, 
        string nodeType, 
        Dictionary<string, List<TagResponse>> userTagMap)
    {
        var tenureYears = user.AgencyStartDate.HasValue
            ? (DateTime.UtcNow - user.AgencyStartDate.Value).TotalDays / 365.0
            : (double?)null;

        return new OrgChartNodeDto
        {
            Id = user.Id,
            NodeType = nodeType,
            Name = user.Name ?? user.Email ?? user.Id,
            ParentId = team != null ? $"team-{team.Id}" : null,
            Depth = 3,
            Tags = userTagMap.GetValueOrDefault(user.Id, new List<TagResponse>()),
            Metadata = new OrgChartNodeMetadata
            {
                SiteId = team?.AgencySiteId?.ToString(),
                TeamId = team?.Id.ToString(),
                TeamName = team?.Name,
                Title = user.Title.ToString(),
                AgentStatus = user.AgentStatus.ToString(),
                AgentType = user.AgentType.ToString(),
                CommissionTier = user.CommissionTier.ToString(),
                AgencyStartDate = user.AgencyStartDate,
                TenureYears = tenureYears,
                Email = user.Email,
                ImageUrl = user.ImageAvatarUrl,
                Phone = user.Phone
            },
            Children = new List<OrgChartNodeDto>()
        };
    }

    private static bool PassesTagFilter(OrgChartNodeDto node, List<int>? tagIds)
    {
        if (tagIds == null || tagIds.Count == 0) return true;
        return node.Tags.Any(t => tagIds.Contains(t.Id));
    }

    private static void FlattenTree(OrgChartNodeDto node, List<OrgChartNodeDto> flat)
    {
        var nodeCopy = new OrgChartNodeDto
        {
            Id = node.Id,
            NodeType = node.NodeType,
            Name = node.Name,
            ParentId = node.ParentId,
            Depth = node.Depth,
            ManagerId = node.ManagerId,
            DirectReportCount = node.DirectReportCount,
            TotalReportCount = node.TotalReportCount,
            Tags = node.Tags,
            Metadata = node.Metadata,
            Children = new List<OrgChartNodeDto>() // Don't include children in flat list
        };
        
        flat.Add(nodeCopy);
        
        foreach (var child in node.Children)
        {
            FlattenTree(child, flat);
        }
    }

    private static OrgChartNodeDto? FindFirstMatchingNode(
        OrgChartNodeDto node, 
        Func<OrgChartNodeDto, bool> predicate)
    {
        if (predicate(node)) return node;
        
        foreach (var child in node.Children)
        {
            var match = FindFirstMatchingNode(child, predicate);
            if (match != null) return match;
        }
        
        return null;
    }
}
