# LuminaryLife Org Chart: ChartHop-Style Concept

## Executive Summary

This document outlines how to leverage LuminaryLife's existing hierarchical data (agency sites, teams, managers, agents) to build an insightful, interactive org chart. It covers data utilization, automated tagging pipelines, C# backend concepts aligned with the codebase, and Vue.js frontend concepts.

---

## Part 1: Current Data Model & Hierarchy

### 1.1 Existing Organizational Structure

```
AgencySite (Top Level)
├── Id, UId, Name, Address1, Address2, City, State, Zip, Phone
├── AgentCount, TeamCount
└── AgencyTeams[] (1:many)
    └── AgencyTeam
        ├── Id, UId, Name, AgencySiteId, ManagerUserId, MemberCount
        ├── Manager (User)
        └── Users[] where User.AgencyTeamId = team.Id

User (Agent/Manager)
├── Id, Name, Email, AgencySiteId, AgencyTeamId
├── Title (Agent|Manager|Admin|Developer|Other)
├── CommissionTier (None|Tier1|Tier2|Tier3)
├── AgentStatus (Active|Inactive)
├── AgentType (Training|Performance)
├── AgencyStartDate
├── EmployeeProfile
└── Roles (SuperAdmin|SiteManager|TeamManager|Agent|...)

RingGroup (Call Routing)
├── ManagerUserId
└── RingGroupUser[] (many:many with User)
```

### 1.2 Key Tables

| Table | Key Columns |
|-------|-------------|
| `agency_sites` | Id, UId, Name, City, State, AgentCount, TeamCount |
| `agency_teams` | Id, UId, Name, AgencySiteId, ManagerUserId, MemberCount |
| `users` | Id, AgencySiteId, AgencyTeamId, Title, AgentStatus, AgentType, CommissionTier |
| `entity_tags` | EntityId (varchar), EntityType, TagId |
| `tags` | Id, Name, HexColorCode |
| `agency_ring_groups` | ManagerUserId, RingGroupUser[] |

---

## Part 2: C# Backend Concepts

### 2.1 Extend EntityTypeEnum

**File:** `LuminaryLife.Common/Systems/Tags/Models/EntityTypeEnum.cs`

```csharp
namespace LuminaryLife.Common.Systems.Tags.Models;

public enum EntityTypeEnum
{
    Note = 1,
    User = 2,
    AgencySite = 3,
    AgencyTeam = 4,
    RingGroup = 5
}
```

`entity_tags.EntityId` is `varchar(255)`. For User use `user.Id` (GUID string). For AgencySite/AgencyTeam use `Id.ToString()`.

### 2.2 TagRepository / TagService Overloads for String EntityId

`ITagRepository` and `ITagService` currently use `int entityId`. For User entities (GUID string), add overloads:

```csharp
Task<List<TagResponse>> GetTagsByEntityIdAsync(EntityTypeEnum entityType, string entityId);
Task<bool> UpdateEntityTagsAsync(EntityTypeEnum entityType, string entityId, List<int> tagIds, string? createdBy = null);
```

In `TagRepository`, use `entityId` as-is for `EntityTag.EntityId` (varchar). For `GetTagsForEntityAsync` with User, return `Dictionary<string, List<TagResponse>>` keyed by userId.

### 2.3 Org Chart Models

**Namespace:** `LuminaryLife.Common.Systems.OrgChart.Models`

```csharp
namespace LuminaryLife.Common.Systems.OrgChart.Models;

public class OrgChartNodeDto
{
    public string Id { get; set; } = string.Empty;
    public string NodeType { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? ParentId { get; set; }
    public int Depth { get; set; }
    public string? ManagerId { get; set; }
    public int DirectReportCount { get; set; }
    public int TotalReportCount { get; set; }
    public List<TagResponse> Tags { get; set; } = new();
    public OrgChartNodeMetadata Metadata { get; set; } = new();
    public List<OrgChartNodeDto> Children { get; set; } = new();
}

public class OrgChartNodeMetadata
{
    public string? SiteId { get; set; }
    public string? SiteName { get; set; }
    public string? TeamId { get; set; }
    public string? TeamName { get; set; }
    public string? Title { get; set; }
    public string? AgentStatus { get; set; }
    public string? AgentType { get; set; }
    public string? CommissionTier { get; set; }
    public DateTime? AgencyStartDate { get; set; }
    public double? TenureYears { get; set; }
    public string? Email { get; set; }
    public string? ImageUrl { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
}

public class OrgChartFilterOptions
{
    public List<FilterOption> Sites { get; set; } = new();
    public List<FilterOption> Teams { get; set; } = new();
    public List<FilterOption> Managers { get; set; } = new();
    public List<TagResponse> Tags { get; set; } = new();
}
```

### 2.4 Org Chart Service

**File:** `LuminaryLife.Common/Systems/OrgChart/Services/OrgChartService.cs`

Follow existing pattern: interface at top, single implementation. Dependencies: `CoreApiEfDbContext`, `IAgencySiteService`, `IAgencyTeamsService`, `IUserService`, `ITagService`, `ICacheService`, `IIdentityService`.

```csharp
namespace LuminaryLife.Common.Systems.OrgChart.Services;

public interface IOrgChartService
{
    Task<OrgChartNodeDto> GetOrgChartTreeAsync(OrgChartQueryParams query);
    Task<List<OrgChartNodeDto>> GetOrgChartFlatAsync(OrgChartQueryParams query);
    Task<OrgChartNodeDto?> GetOrgChartSubtreeAsync(int? siteId, int? teamId, string? managerId, OrgChartQueryParams query);
    Task<OrgChartNodeDto?> GetPersonNodeAsync(string userId);
    Task<OrgChartFilterOptions> GetFilterOptionsAsync();
}

public class OrgChartService : IOrgChartService
{
    private readonly CoreApiEfDbContext _context;
    private readonly ITagService _tagService;
    private readonly ICacheService _cacheService;

    public OrgChartService(
        CoreApiEfDbContext context,
        IAgencySiteService agencySiteService,
        IAgencyTeamsService agencyTeamsService,
        IUserService userService,
        ITagService tagService,
        ICacheService cacheService,
        IIdentityService identityService)
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
                root.Children.Add(siteNode);
        }

        await _cacheService.SetAsync(cacheKey, root, TimeSpan.FromMinutes(5));
        return root;
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
            Metadata = new OrgChartNodeMetadata { SiteId = site.Id.ToString(), SiteName = site.Name, City = site.City, State = site.State },
            Children = new List<OrgChartNodeDto>()
        };

        foreach (var team in teams)
        {
            var teamNode = await BuildTeamNodeAsync(team, site, query);
            if (teamNode != null)
                siteNode.Children.Add(teamNode);
        }

        return siteNode;
    }

    private async Task<OrgChartNodeDto?> BuildTeamNodeAsync(AgencyTeam team, AgencySite site, OrgChartQueryParams query)
    {
        var users = await _context.Users
            .Where(u => u.AgencyTeamId == team.Id && (query.IncludeInactive || u.AgentStatus == AgentStatusEnum.Active))
            .OrderBy(u => u.Name)
            .ToListAsync();

        var teamNode = new OrgChartNodeDto
        {
            Id = $"team-{team.Id}",
            NodeType = "Team",
            Name = team.Name,
            ParentId = $"site-{site.Id}",
            Depth = 2,
            ManagerId = team.ManagerUserId,
            DirectReportCount = users.Count,
            Metadata = new OrgChartNodeMetadata { TeamId = team.Id.ToString(), TeamName = team.Name, SiteId = site.Id.ToString(), SiteName = site.Name },
            Children = new List<OrgChartNodeDto>()
        };

        var userTagMap = await GetTagsForUsersAsync(users.Select(u => u.Id).ToList());
        var manager = users.FirstOrDefault(u => u.Id == team.ManagerUserId);
        var agents = users.Where(u => u.Id != team.ManagerUserId).ToList();

        if (manager != null)
        {
            var managerNode = BuildUserNode(manager, team, "Manager", userTagMap);
            if (PassesTagFilter(managerNode, query.TagIds))
                teamNode.Children.Add(managerNode);
        }

        foreach (var agent in agents)
        {
            var agentNode = BuildUserNode(agent, team, "Agent", userTagMap);
            if (PassesTagFilter(agentNode, query.TagIds))
                teamNode.Children.Add(agentNode);
        }

        return teamNode;
    }

    private OrgChartNodeDto BuildUserNode(User user, AgencyTeam team, string nodeType, Dictionary<string, List<TagResponse>> userTagMap)
    {
        var tenureYears = user.AgencyStartDate.HasValue
            ? (DateTime.UtcNow - user.AgencyStartDate.Value).TotalDays / 365.0
            : (double?)null;

        return new OrgChartNodeDto
        {
            Id = user.Id,
            NodeType = nodeType,
            Name = user.Name ?? user.Email ?? user.Id,
            ParentId = $"team-{team.Id}",
            Depth = 3,
            Tags = userTagMap.GetValueOrDefault(user.Id, new List<TagResponse>()),
            Metadata = new OrgChartNodeMetadata
            {
                SiteId = team.AgencySiteId?.ToString(),
                TeamId = team.Id.ToString(),
                TeamName = team.Name,
                Title = user.Title.ToString(),
                AgentStatus = user.AgentStatus.ToString(),
                AgentType = user.AgentType.ToString(),
                CommissionTier = user.CommissionTier.ToString(),
                AgencyStartDate = user.AgencyStartDate,
                TenureYears = tenureYears,
                Email = user.Email,
                ImageUrl = user.ImageAvatarUrl
            },
            Children = new List<OrgChartNodeDto>()
        };
    }

    private async Task<Dictionary<string, List<TagResponse>>> GetTagsForUsersAsync(List<string> userIds)
    {
        if (userIds.Count == 0) return new Dictionary<string, List<TagResponse>>();
        var result = new Dictionary<string, List<TagResponse>>();
        foreach (var userId in userIds)
        {
            var tags = await _tagService.GetTagsByEntityIdAsync(EntityTypeEnum.User, userId);
            result[userId] = tags;
        }
        return result;
    }

    private static bool PassesTagFilter(OrgChartNodeDto node, List<int>? tagIds)
    {
        if (tagIds == null || tagIds.Count == 0) return true;
        return node.Tags.Any(t => tagIds.Contains(t.Id));
    }
}

public class OrgChartQueryParams
{
    public int? SiteId { get; set; }
    public int? TeamId { get; set; }
    public string? ManagerId { get; set; }
    public List<int>? TagIds { get; set; }
    public bool IncludeInactive { get; set; }
    public int? MaxDepth { get; set; }

    public string GetCacheKey() =>
        $"{SiteId ?? 0}_{TeamId ?? 0}_{ManagerId ?? ""}_{string.Join(",", TagIds ?? new List<int>())}_{IncludeInactive}_{MaxDepth ?? 99}";
}
```

### 2.5 Org Chart Tag Engine

**File:** `LuminaryLife.Common/Systems/OrgChart/Services/OrgChartTagEngine.cs`

```csharp
namespace LuminaryLife.Common.Systems.OrgChart.Services;

public interface IOrgChartTagEngine
{
    Task<List<int>> ComputeTagsForUserAsync(User user);
    Task<List<int>> ComputeTagsForSiteAsync(AgencySite site);
    Task<List<int>> ComputeTagsForTeamAsync(AgencyTeam team);
    Task SyncTagsForUserAsync(string userId);
    Task SyncTagsForSiteAsync(int siteId);
    Task SyncTagsForTeamAsync(int teamId);
}

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

        if (user.AgentType == AgentTypeEnum.Training)
            TryAddTag(tagIds, "Training");
        if (user.AgentType == AgentTypeEnum.Performance)
            TryAddTag(tagIds, "Performance");
        if (user.CommissionTier == CommissionTierEnum.Tier1)
            TryAddTag(tagIds, "Tier 1");
        if (user.CommissionTier == CommissionTierEnum.Tier2)
            TryAddTag(tagIds, "Tier 2");
        if (user.CommissionTier == CommissionTierEnum.Tier3)
            TryAddTag(tagIds, "Tier 3");
        if (user.AgentStatus == AgentStatusEnum.Active)
            TryAddTag(tagIds, "Active");
        if (user.AgentStatus == AgentStatusEnum.Inactive)
            TryAddTag(tagIds, "Inactive");
        if (user.AgencyStartDate.HasValue && (DateTime.UtcNow - user.AgencyStartDate.Value).TotalDays <= 90)
            TryAddTag(tagIds, "New Hire");
        if (user.AgencyStartDate.HasValue && (DateTime.UtcNow - user.AgencyStartDate.Value).TotalDays > 90)
            TryAddTag(tagIds, "Tenured");

        var isManager = await _context.AgencyTeams.AnyAsync(t => t.ManagerUserId == user.Id && t.DeletedAt == null);
        if (isManager)
            TryAddTag(tagIds, "Manager");

        return tagIds;
    }

    public async Task<List<int>> ComputeTagsForSiteAsync(AgencySite site)
    {
        var tagIds = new List<int>();
        await EnsureTagCacheAsync();

        if (site.Name?.Contains("HQ", StringComparison.OrdinalIgnoreCase) == true ||
            site.Name?.Contains("Headquarters", StringComparison.OrdinalIgnoreCase) == true)
            TryAddTag(tagIds, "HQ");

        return tagIds;
    }

    public async Task<List<int>> ComputeTagsForTeamAsync(AgencyTeam team)
    {
        var tagIds = new List<int>();
        await EnsureTagCacheAsync();

        if (team.MemberCount >= 10)
            TryAddTag(tagIds, "Large Team");

        return tagIds;
    }

    public async Task SyncTagsForUserAsync(string userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return;

        var tagIds = await ComputeTagsForUserAsync(user);
        await _tagRepository.UpdateEntityTagsAsync(EntityTypeEnum.User, userId, tagIds, "system");
    }

    private async Task EnsureTagCacheAsync()
    {
        if (_tagNameToId != null) return;
        var tags = await _context.Tags.AsNoTracking().ToListAsync();
        _tagNameToId = tags.ToDictionary(t => t.Name, t => t.Id, StringComparer.OrdinalIgnoreCase);
    }

    private void TryAddTag(List<int> tagIds, string tagName)
    {
        if (_tagNameToId != null && _tagNameToId.TryGetValue(tagName, out var id))
            tagIds.Add(id);
    }
}
```

### 2.6 Org Chart Controller

**File:** `LuminaryLife.Api/HTTP/Controllers/OrgChartController.cs`

```csharp
namespace LuminaryLifeCoreApi.HTTP.Controllers;

[ApiController]
[Authorize]
[Route("/org-chart")]
[HandleAppExceptions]
public class OrgChartController(IOrgChartService orgChartService, IIdentityService identityService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(OrgChartNodeDto), StatusCodes.Status200OK)]
    public async Task<OrgChartNodeDto> GetOrgChart(
        [FromQuery] int? siteId,
        [FromQuery] int? teamId,
        [FromQuery] string? managerId,
        [FromQuery] string? tagIds,
        [FromQuery] bool includeInactive = false,
        [FromQuery] int? depth = null)
    {
        var query = new OrgChartQueryParams
        {
            SiteId = siteId,
            TeamId = teamId,
            ManagerId = managerId,
            TagIds = tagIds?.Split(',').Select(int.Parse).ToList(),
            IncludeInactive = includeInactive,
            MaxDepth = depth
        };
        return await orgChartService.GetOrgChartTreeAsync(query);
    }

    [HttpGet("flat")]
    [ProducesResponseType(typeof(List<OrgChartNodeDto>), StatusCodes.Status200OK)]
    public async Task<List<OrgChartNodeDto>> GetOrgChartFlat(
        [FromQuery] int? siteId,
        [FromQuery] int? teamId,
        [FromQuery] string? managerId,
        [FromQuery] bool includeInactive = false)
    {
        var query = new OrgChartQueryParams { SiteId = siteId, TeamId = teamId, ManagerId = managerId, IncludeInactive = includeInactive };
        return await orgChartService.GetOrgChartFlatAsync(query);
    }

    [HttpGet("filter-options")]
    [ProducesResponseType(typeof(OrgChartFilterOptions), StatusCodes.Status200OK)]
    public async Task<OrgChartFilterOptions> GetFilterOptions()
    {
        return await orgChartService.GetFilterOptionsAsync();
    }

    [HttpGet("person/{userId}")]
    [ProducesResponseType(typeof(OrgChartNodeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrgChartNodeDto>> GetPerson([FromRoute] string userId)
    {
        var node = await orgChartService.GetPersonNodeAsync(userId);
        return node == null ? NotFound() : Ok(node);
    }
}
```

### 2.7 SubSystemRegistration

**File:** `LuminaryLife.Common/Systems/OrgChart/SubSystemRegistration.cs`

```csharp
namespace LuminaryLife.Common.Systems.OrgChart;

public class SubSystemRegistration : ISubSystemRegistration
{
    public void Register(IServiceCollection services, string environmentName, IConfiguration configuration)
    {
        services.AddScoped<IOrgChartService, OrgChartService>();
        services.AddScoped<IOrgChartTagEngine, OrgChartTagEngine>();
    }
}
```

Register in `Program.cs` or wherever `ISubSystemRegistration` implementations are discovered.

### 2.8 Event Handlers for Tag Sync

Wire `UserEvent`, `AgencyTeamEvent`, `AgencySiteEvent` to call `IOrgChartTagEngine.SyncTagsForUserAsync`, `SyncTagsForTeamAsync`, `SyncTagsForSiteAsync`. Use existing `IEventService` / event handlers pattern. Invalidate cache keys `org-chart:*` on entity changes.

### 2.9 Hangfire Jobs

```csharp
RecurringJob.AddOrUpdate<IOrgChartTagEngine>("org-chart-tag-sync", e => e.SyncAllTagsAsync(), Cron.Daily(2));
```

`SyncAllTagsAsync` iterates users, sites, teams and calls respective sync methods.

---

## Part 3: Vue.js Frontend Concepts

### 3.1 Project Structure

```
src/
├── views/
│   └── OrgChartView.vue
├── components/
│   └── org-chart/
│       ├── OrgChartTree.vue
│       ├── OrgChartNode.vue
│       ├── OrgChartFilters.vue
│       ├── OrgChartDetailPanel.vue
│       └── OrgChartTagPill.vue
├── composables/
│   └── useOrgChart.ts
├── api/
│   └── orgChart.ts
└── types/
    └── orgChart.ts
```

### 3.2 Types

**File:** `src/types/orgChart.ts`

```typescript
export interface OrgChartNode {
  id: string
  nodeType: string
  name: string
  parentId?: string
  depth: number
  managerId?: string
  directReportCount: number
  totalReportCount: number
  tags: Tag[]
  metadata: OrgChartNodeMetadata
  children: OrgChartNode[]
}

export interface OrgChartNodeMetadata {
  siteId?: string
  siteName?: string
  teamId?: string
  teamName?: string
  title?: string
  agentStatus?: string
  agentType?: string
  commissionTier?: string
  agencyStartDate?: string
  tenureYears?: number
  email?: string
  imageUrl?: string
  city?: string
  state?: string
}

export interface Tag {
  id: number
  name: string
  hexColorCode?: string
}

export interface OrgChartFilterOptions {
  sites: FilterOption[]
  teams: FilterOption[]
  managers: FilterOption[]
  tags: Tag[]
}

export interface FilterOption {
  id: string
  name: string
}

export interface OrgChartQueryParams {
  siteId?: number
  teamId?: number
  managerId?: string
  tagIds?: number[]
  includeInactive?: boolean
  depth?: number
}
```

### 3.3 API Client

**File:** `src/api/orgChart.ts`

```typescript
import type { OrgChartNode, OrgChartFilterOptions, OrgChartQueryParams } from '@/types/orgChart'

const baseUrl = import.meta.env.VITE_API_BASE_URL ?? '/api'

export async function fetchOrgChart(params: OrgChartQueryParams): Promise<OrgChartNode> {
  const search = new URLSearchParams()
  if (params.siteId != null) search.set('siteId', String(params.siteId))
  if (params.teamId != null) search.set('teamId', String(params.teamId))
  if (params.managerId) search.set('managerId', params.managerId)
  if (params.tagIds?.length) search.set('tagIds', params.tagIds.join(','))
  if (params.includeInactive) search.set('includeInactive', 'true')
  if (params.depth != null) search.set('depth', String(params.depth))

  const res = await fetch(`${baseUrl}/org-chart?${search}`, {
    headers: { Authorization: `Bearer ${getToken()}` }
  })
  if (!res.ok) throw new Error('Failed to fetch org chart')
  return res.json()
}

export async function fetchOrgChartFlat(params: OrgChartQueryParams): Promise<OrgChartNode[]> {
  const search = new URLSearchParams()
  if (params.siteId != null) search.set('siteId', String(params.siteId))
  if (params.teamId != null) search.set('teamId', String(params.teamId))
  if (params.managerId) search.set('managerId', params.managerId)
  if (params.includeInactive) search.set('includeInactive', 'true')

  const res = await fetch(`${baseUrl}/org-chart/flat?${search}`, {
    headers: { Authorization: `Bearer ${getToken()}` }
  })
  if (!res.ok) throw new Error('Failed to fetch org chart')
  return res.json()
}

export async function fetchFilterOptions(): Promise<OrgChartFilterOptions> {
  const res = await fetch(`${baseUrl}/org-chart/filter-options`, {
    headers: { Authorization: `Bearer ${getToken()}` }
  })
  if (!res.ok) throw new Error('Failed to fetch filter options')
  return res.json()
}

function getToken(): string {
  return localStorage.getItem('accessToken') ?? ''
}
```

### 3.4 Composable

**File:** `src/composables/useOrgChart.ts`

```typescript
import { ref, computed, watch } from 'vue'
import { fetchOrgChart, fetchFilterOptions } from '@/api/orgChart'
import type { OrgChartNode, OrgChartFilterOptions, OrgChartQueryParams } from '@/types/orgChart'

export function useOrgChart() {
  const tree = ref<OrgChartNode | null>(null)
  const filterOptions = ref<OrgChartFilterOptions | null>(null)
  const loading = ref(false)
  const error = ref<Error | null>(null)

  const params = ref<OrgChartQueryParams>({})

  const loadTree = async () => {
    loading.value = true
    error.value = null
    try {
      tree.value = await fetchOrgChart(params.value)
    } catch (e) {
      error.value = e as Error
    } finally {
      loading.value = false
    }
  }

  const loadFilterOptions = async () => {
    try {
      filterOptions.value = await fetchFilterOptions()
    } catch (e) {
      error.value = e as Error
    }
  }

  watch(params, loadTree, { deep: true })

  return {
    tree,
    filterOptions,
    params,
    loading,
    error,
    loadTree,
    loadFilterOptions
  }
}
```

### 3.5 OrgChartView

**File:** `src/views/OrgChartView.vue`

```vue
<template>
  <div class="org-chart-view">
    <OrgChartFilters
      v-if="filterOptions"
      :options="filterOptions"
      v-model:site-id="params.siteId"
      v-model:team-id="params.teamId"
      v-model:manager-id="params.managerId"
      v-model:tag-ids="params.tagIds"
      v-model:include-inactive="params.includeInactive"
    />
    <div v-if="loading" class="loading">Loading...</div>
    <OrgChartTree
      v-else-if="tree"
      :node="tree"
      :expanded-ids="expandedIds"
      :selected-id="selectedId"
      @toggle="toggleNode"
      @select="selectedId = $event"
    />
    <OrgChartDetailPanel
      v-if="selectedNode"
      :node="selectedNode"
      @close="selectedId = null"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import OrgChartFilters from '@/components/org-chart/OrgChartFilters.vue'
import OrgChartTree from '@/components/org-chart/OrgChartTree.vue'
import OrgChartDetailPanel from '@/components/org-chart/OrgChartDetailPanel.vue'
import { useOrgChart } from '@/composables/useOrgChart'
import type { OrgChartNode } from '@/types/orgChart'

const { tree, filterOptions, params, loading, loadTree, loadFilterOptions } = useOrgChart()
const expandedIds = ref<Set<string>>(new Set(['root']))
const selectedId = ref<string | null>(null)

const selectedNode = computed(() => {
  if (!selectedId.value || !tree.value) return null
  return findNode(tree.value, selectedId.value)
})

function findNode(node: OrgChartNode, id: string): OrgChartNode | null {
  if (node.id === id) return node
  for (const child of node.children) {
    const found = findNode(child, id)
    if (found) return found
  }
  return null
}

function toggleNode(id: string) {
  const next = new Set(expandedIds.value)
  if (next.has(id)) next.delete(id)
  else next.add(id)
  expandedIds.value = next
}

onMounted(() => {
  loadFilterOptions()
  loadTree()
})
</script>
```

### 3.6 OrgChartTree (Recursive)

**File:** `src/components/org-chart/OrgChartTree.vue`

```vue
<template>
  <div class="org-chart-tree" :class="[`depth-${node.depth}`, node.nodeType.toLowerCase()]">
    <OrgChartNode
      :node="node"
      :expanded="expandedIds.has(node.id)"
      :selected="selectedId === node.id"
      @click="$emit('select', node.id)"
      @toggle="$emit('toggle', node.id)"
    />
    <div v-if="expandedIds.has(node.id) && node.children.length" class="children">
      <OrgChartTree
        v-for="child in node.children"
        :key="child.id"
        :node="child"
        :expanded-ids="expandedIds"
        :selected-id="selectedId"
        @toggle="$emit('toggle', $event)"
        @select="$emit('select', $event)"
      />
    </div>
  </div>
</template>

<script setup lang="ts">
import OrgChartNode from './OrgChartNode.vue'
import type { OrgChartNode as OrgChartNodeType } from '@/types/orgChart'

defineProps<{
  node: OrgChartNodeType
  expandedIds: Set<string>
  selectedId: string | null
}>()

defineEmits<{
  toggle: [id: string]
  select: [id: string]
}>()
</script>
```

### 3.7 OrgChartNode

**File:** `src/components/org-chart/OrgChartNode.vue`

```vue
<template>
  <div
    class="org-chart-node"
    :class="{ selected }"
    @click="$emit('click')"
  >
    <button
      v-if="node.children.length"
      class="toggle"
      @click.stop="$emit('toggle')"
    >
      {{ expanded ? '−' : '+' }}
    </button>
    <div class="content">
      <img v-if="node.metadata.imageUrl" :src="node.metadata.imageUrl" class="avatar" alt="" />
      <div class="info">
        <span class="name">{{ node.name }}</span>
        <span v-if="node.metadata.title" class="title">{{ node.metadata.title }}</span>
        <span v-if="node.metadata.tenureYears != null" class="tenure">{{ node.metadata.tenureYears.toFixed(1) }} yrs</span>
        <div class="tags">
          <OrgChartTagPill v-for="tag in node.tags" :key="tag.id" :tag="tag" />
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import OrgChartTagPill from './OrgChartTagPill.vue'
import type { OrgChartNode as OrgChartNodeType } from '@/types/orgChart'

defineProps<{
  node: OrgChartNodeType
  expanded: boolean
  selected: boolean
}>()

defineEmits<{
  click: []
  toggle: []
}>()
</script>
```

### 3.8 OrgChartTagPill

**File:** `src/components/org-chart/OrgChartTagPill.vue`

```vue
<template>
  <span
    class="org-chart-tag-pill"
    :style="{ backgroundColor: tag.hexColorCode ? `#${tag.hexColorCode}` : '#ccc' }"
  >
    {{ tag.name }}
  </span>
</template>

<script setup lang="ts">
import type { Tag } from '@/types/orgChart'

defineProps<{ tag: Tag }>()
</script>
```

### 3.9 OrgChartFilters

**File:** `src/components/org-chart/OrgChartFilters.vue`

```vue
<template>
  <div class="org-chart-filters">
    <select v-model="siteId" @change="emitUpdate">
      <option :value="undefined">All Sites</option>
      <option v-for="s in options.sites" :key="s.id" :value="s.id">{{ s.name }}</option>
    </select>
    <select v-model="teamId" @change="emitUpdate">
      <option :value="undefined">All Teams</option>
      <option v-for="t in options.teams" :key="t.id" :value="t.id">{{ t.name }}</option>
    </select>
    <select v-model="managerId" @change="emitUpdate">
      <option value="">All Managers</option>
      <option v-for="m in options.managers" :key="m.id" :value="m.id">{{ m.name }}</option>
    </select>
    <div class="tag-filters">
      <label v-for="tag in options.tags" :key="tag.id" class="tag-check">
        <input type="checkbox" :value="tag.id" v-model="tagIds" @change="emitUpdate" />
        <span :style="{ borderColor: tag.hexColorCode ? `#${tag.hexColorCode}` : '#ccc' }">{{ tag.name }}</span>
      </label>
    </div>
    <label>
      <input type="checkbox" v-model="includeInactive" @change="emitUpdate" />
      Include Inactive
    </label>
  </div>
</template>

<script setup lang="ts">
import type { OrgChartFilterOptions } from '@/types/orgChart'

const props = defineProps<{ options: OrgChartFilterOptions }>()

const siteId = defineModel<number | undefined>('siteId')
const teamId = defineModel<number | undefined>('teamId')
const managerId = defineModel<string>('managerId')
const tagIds = defineModel<number[]>('tagIds', { default: () => [] })
const includeInactive = defineModel<boolean>('includeInactive', { default: false })

const emit = defineEmits<{
  'update:siteId': [number | undefined]
  'update:teamId': [number | undefined]
  'update:managerId': [string]
  'update:tagIds': [number[]]
  'update:includeInactive': [boolean]
}>()

function emitUpdate() {
  emit('update:siteId', siteId.value)
  emit('update:teamId', teamId.value)
  emit('update:managerId', managerId.value)
  emit('update:tagIds', tagIds.value)
  emit('update:includeInactive', includeInactive.value)
}
</script>
```

### 3.10 Styling Notes

- Use CSS variables for depth-based indentation: `--depth-offset: calc(var(--depth) * 1.5rem)`
- Node types: `.site`, `.team`, `.manager`, `.agent` for distinct styling
- Tag pills: small rounded badges with `hexColorCode` as background
- Detail panel: slide-in from right, show full metadata, tenure, tags, performance overlay

---

## Part 4: Tag Definitions (Seed)

| Tag Name | HexColorCode | EntityType | Rule |
|----------|--------------|------------|------|
| Training | FFA500 | User | AgentType == Training |
| Performance | 228B22 | User | AgentType == Performance |
| Tier 1 | 4169E1 | User | CommissionTier == Tier1 |
| Tier 2 | 9370DB | User | CommissionTier == Tier2 |
| Tier 3 | 8B4513 | User | CommissionTier == Tier3 |
| Manager | 2F4F4F | User | agency_teams.ManagerUserId |
| New Hire | FFD700 | User | AgencyStartDate <= 90 days |
| Tenured | 006400 | User | AgencyStartDate > 90 days |
| Active | 32CD32 | User | AgentStatus == Active |
| Inactive | 808080 | User | AgentStatus == Inactive |
| HQ | DC143C | AgencySite | Name contains HQ/Headquarters |
| Large Team | 20B2AA | AgencyTeam | MemberCount >= 10 |

---

## Part 5: Security

Use `IIdentityService` and `AuthPolicies.MinimumTeamManager` (or `MinimumSiteManager`) on `OrgChartController`. Filter tree by permitted sites/teams for SiteManager and TeamManager roles. SuperAdmin sees full org.

---

## Part 6: References

- LuminaryLife tables: `agency_sites`, `agency_teams`, `users`, `entity_tags`, `tags`
- Services: `IAgencySiteService`, `IAgencyTeamsService`, `IUserService`, `ITagService`, `ICacheService`
- Events: `UserEvent`, `AgencyTeamEvent`, `AgencySiteEvent`
- Tags: `LuminaryLife.Common.Systems.Tags` (SubSystemRegistration, TagService, TagRepository)
