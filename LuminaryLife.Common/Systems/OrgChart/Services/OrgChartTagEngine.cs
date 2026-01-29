using LuminaryLife.Common.Data;
using LuminaryLife.Common.Entities;
using LuminaryLife.Common.Enums;
using LuminaryLife.Common.Systems.Tags.Models;
using LuminaryLife.Common.Systems.Tags.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LuminaryLife.Common.Systems.OrgChart.Services;

/// <summary>
/// Engine for automatically computing and syncing tags based on entity properties
/// </summary>
public class OrgChartTagEngine : IOrgChartTagEngine
{
    private readonly CoreApiEfDbContext _context;
    private readonly ITagRepository _tagRepository;
    private Dictionary<string, int>? _tagNameToId;

    public OrgChartTagEngine(CoreApiEfDbContext context, ITagRepository tagRepository)
    {
        _context = context;
        _tagRepository = tagRepository;
    }

    public async Task<List<int>> ComputeTagsForUserAsync(User user)
    {
        var tagIds = new List<int>();
        await EnsureTagCacheAsync();

        // Agent Type tags
        if (user.AgentType == AgentTypeEnum.Training)
            TryAddTag(tagIds, "Training");
        if (user.AgentType == AgentTypeEnum.Performance)
            TryAddTag(tagIds, "Performance");

        // Commission Tier tags
        if (user.CommissionTier == CommissionTierEnum.Tier1)
            TryAddTag(tagIds, "Tier 1");
        if (user.CommissionTier == CommissionTierEnum.Tier2)
            TryAddTag(tagIds, "Tier 2");
        if (user.CommissionTier == CommissionTierEnum.Tier3)
            TryAddTag(tagIds, "Tier 3");

        // Agent Status tags
        if (user.AgentStatus == AgentStatusEnum.Active)
            TryAddTag(tagIds, "Active");
        if (user.AgentStatus == AgentStatusEnum.Inactive)
            TryAddTag(tagIds, "Inactive");

        // Tenure-based tags
        if (user.AgencyStartDate.HasValue)
        {
            var daysSinceStart = (DateTime.UtcNow - user.AgencyStartDate.Value).TotalDays;
            if (daysSinceStart <= 90)
                TryAddTag(tagIds, "New Hire");
            else
                TryAddTag(tagIds, "Tenured");
        }

        // Manager tag - check if user is a manager of any team
        var isManager = await _context.AgencyTeams
            .AnyAsync(t => t.ManagerUserId == user.Id && t.DeletedAt == null);
        if (isManager)
            TryAddTag(tagIds, "Manager");

        return tagIds;
    }

    public async Task<List<int>> ComputeTagsForSiteAsync(AgencySite site)
    {
        var tagIds = new List<int>();
        await EnsureTagCacheAsync();

        // HQ tag for headquarters sites
        if (site.Name?.Contains("HQ", StringComparison.OrdinalIgnoreCase) == true ||
            site.Name?.Contains("Headquarters", StringComparison.OrdinalIgnoreCase) == true)
        {
            TryAddTag(tagIds, "HQ");
        }

        return tagIds;
    }

    public async Task<List<int>> ComputeTagsForTeamAsync(AgencyTeam team)
    {
        var tagIds = new List<int>();
        await EnsureTagCacheAsync();

        // Large Team tag for teams with 10+ members
        if (team.MemberCount >= 10)
            TryAddTag(tagIds, "Large Team");

        return tagIds;
    }

    public async Task SyncTagsForUserAsync(string userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null || user.DeletedAt != null) return;

        var tagIds = await ComputeTagsForUserAsync(user);
        await _tagRepository.UpdateEntityTagsAsync(EntityTypeEnum.User, userId, tagIds, "system");
    }

    public async Task SyncTagsForSiteAsync(int siteId)
    {
        var site = await _context.AgencySites.FindAsync(siteId);
        if (site == null || site.DeletedAt != null) return;

        var tagIds = await ComputeTagsForSiteAsync(site);
        await _tagRepository.UpdateEntityTagsAsync(EntityTypeEnum.AgencySite, siteId, tagIds, "system");
    }

    public async Task SyncTagsForTeamAsync(int teamId)
    {
        var team = await _context.AgencyTeams.FindAsync(teamId);
        if (team == null || team.DeletedAt != null) return;

        var tagIds = await ComputeTagsForTeamAsync(team);
        await _tagRepository.UpdateEntityTagsAsync(EntityTypeEnum.AgencyTeam, teamId, tagIds, "system");
    }

    public async Task SyncAllTagsAsync()
    {
        // Sync all users
        var users = await _context.Users
            .Where(u => u.DeletedAt == null)
            .ToListAsync();

        foreach (var user in users)
        {
            var tagIds = await ComputeTagsForUserAsync(user);
            await _tagRepository.UpdateEntityTagsAsync(EntityTypeEnum.User, user.Id, tagIds, "system");
        }

        // Sync all sites
        var sites = await _context.AgencySites
            .Where(s => s.DeletedAt == null)
            .ToListAsync();

        foreach (var site in sites)
        {
            var tagIds = await ComputeTagsForSiteAsync(site);
            await _tagRepository.UpdateEntityTagsAsync(EntityTypeEnum.AgencySite, site.Id, tagIds, "system");
        }

        // Sync all teams
        var teams = await _context.AgencyTeams
            .Where(t => t.DeletedAt == null)
            .ToListAsync();

        foreach (var team in teams)
        {
            var tagIds = await ComputeTagsForTeamAsync(team);
            await _tagRepository.UpdateEntityTagsAsync(EntityTypeEnum.AgencyTeam, team.Id, tagIds, "system");
        }
    }

    private async Task EnsureTagCacheAsync()
    {
        if (_tagNameToId != null) return;
        
        var tags = await _context.Tags.AsNoTracking().ToListAsync();
        _tagNameToId = tags.ToDictionary(
            t => t.Name, 
            t => t.Id, 
            StringComparer.OrdinalIgnoreCase);
    }

    private void TryAddTag(List<int> tagIds, string tagName)
    {
        if (_tagNameToId != null && _tagNameToId.TryGetValue(tagName, out var id))
        {
            tagIds.Add(id);
        }
    }
}
