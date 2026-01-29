using LuminaryLife.Common.Data;
using LuminaryLife.Common.Entities;
using LuminaryLife.Common.Systems.Tags.Models;
using Microsoft.EntityFrameworkCore;

namespace LuminaryLife.Common.Systems.Tags.Repositories;

/// <summary>
/// Repository implementation for tag operations
/// </summary>
public class TagRepository : ITagRepository
{
    private readonly CoreApiEfDbContext _context;

    public TagRepository(CoreApiEfDbContext context)
    {
        _context = context;
    }

    public async Task<List<TagResponse>> GetTagsByEntityIdAsync(EntityTypeEnum entityType, int entityId)
    {
        return await GetTagsByEntityIdAsync(entityType, entityId.ToString());
    }

    public async Task<List<TagResponse>> GetTagsByEntityIdAsync(EntityTypeEnum entityType, string entityId)
    {
        return await _context.EntityTags
            .Where(et => et.EntityType == entityType && et.EntityId == entityId)
            .Include(et => et.Tag)
            .Select(et => new TagResponse
            {
                Id = et.Tag!.Id,
                Name = et.Tag.Name,
                HexColorCode = et.Tag.HexColorCode
            })
            .ToListAsync();
    }

    public async Task<Dictionary<string, List<TagResponse>>> GetTagsForEntitiesAsync(
        EntityTypeEnum entityType, 
        List<string> entityIds)
    {
        if (entityIds.Count == 0)
            return new Dictionary<string, List<TagResponse>>();

        var entityTags = await _context.EntityTags
            .Where(et => et.EntityType == entityType && entityIds.Contains(et.EntityId))
            .Include(et => et.Tag)
            .ToListAsync();

        var result = entityIds.ToDictionary(
            id => id,
            id => new List<TagResponse>()
        );

        foreach (var entityTag in entityTags)
        {
            if (entityTag.Tag != null && result.ContainsKey(entityTag.EntityId))
            {
                result[entityTag.EntityId].Add(new TagResponse
                {
                    Id = entityTag.Tag.Id,
                    Name = entityTag.Tag.Name,
                    HexColorCode = entityTag.Tag.HexColorCode
                });
            }
        }

        return result;
    }

    public async Task<bool> UpdateEntityTagsAsync(
        EntityTypeEnum entityType, 
        int entityId, 
        List<int> tagIds, 
        string? createdBy = null)
    {
        return await UpdateEntityTagsAsync(entityType, entityId.ToString(), tagIds, createdBy);
    }

    public async Task<bool> UpdateEntityTagsAsync(
        EntityTypeEnum entityType, 
        string entityId, 
        List<int> tagIds, 
        string? createdBy = null)
    {
        // Remove existing tags
        var existingTags = await _context.EntityTags
            .Where(et => et.EntityType == entityType && et.EntityId == entityId)
            .ToListAsync();
        
        _context.EntityTags.RemoveRange(existingTags);

        // Add new tags
        var newTags = tagIds.Select(tagId => new EntityTag
        {
            EntityId = entityId,
            EntityType = entityType,
            TagId = tagId,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow
        });

        await _context.EntityTags.AddRangeAsync(newTags);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<List<TagResponse>> GetAllTagsAsync()
    {
        return await _context.Tags
            .AsNoTracking()
            .OrderBy(t => t.Name)
            .Select(t => new TagResponse
            {
                Id = t.Id,
                Name = t.Name,
                HexColorCode = t.HexColorCode
            })
            .ToListAsync();
    }
}
