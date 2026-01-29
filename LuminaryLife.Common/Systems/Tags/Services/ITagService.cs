using LuminaryLife.Common.Systems.Tags.Models;

namespace LuminaryLife.Common.Systems.Tags.Services;

/// <summary>
/// Service interface for tag operations
/// </summary>
public interface ITagService
{
    Task<List<TagResponse>> GetTagsByEntityIdAsync(EntityTypeEnum entityType, int entityId);
    Task<List<TagResponse>> GetTagsByEntityIdAsync(EntityTypeEnum entityType, string entityId);
    Task<Dictionary<string, List<TagResponse>>> GetTagsForEntitiesAsync(EntityTypeEnum entityType, List<string> entityIds);
    Task<bool> UpdateEntityTagsAsync(EntityTypeEnum entityType, int entityId, List<int> tagIds, string? createdBy = null);
    Task<bool> UpdateEntityTagsAsync(EntityTypeEnum entityType, string entityId, List<int> tagIds, string? createdBy = null);
    Task<List<TagResponse>> GetAllTagsAsync();
}
