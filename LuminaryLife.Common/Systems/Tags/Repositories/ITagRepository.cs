using LuminaryLife.Common.Systems.Tags.Models;

namespace LuminaryLife.Common.Systems.Tags.Repositories;

/// <summary>
/// Repository interface for tag operations
/// </summary>
public interface ITagRepository
{
    /// <summary>
    /// Gets all tags for a specific entity by integer ID
    /// </summary>
    Task<List<TagResponse>> GetTagsByEntityIdAsync(EntityTypeEnum entityType, int entityId);
    
    /// <summary>
    /// Gets all tags for a specific entity by string ID (for User entities with GUID IDs)
    /// </summary>
    Task<List<TagResponse>> GetTagsByEntityIdAsync(EntityTypeEnum entityType, string entityId);
    
    /// <summary>
    /// Gets tags for multiple entities of the same type
    /// </summary>
    Task<Dictionary<string, List<TagResponse>>> GetTagsForEntitiesAsync(EntityTypeEnum entityType, List<string> entityIds);
    
    /// <summary>
    /// Updates tags for an entity (replaces all existing tags)
    /// </summary>
    Task<bool> UpdateEntityTagsAsync(EntityTypeEnum entityType, int entityId, List<int> tagIds, string? createdBy = null);
    
    /// <summary>
    /// Updates tags for an entity by string ID (for User entities with GUID IDs)
    /// </summary>
    Task<bool> UpdateEntityTagsAsync(EntityTypeEnum entityType, string entityId, List<int> tagIds, string? createdBy = null);
    
    /// <summary>
    /// Gets all available tags
    /// </summary>
    Task<List<TagResponse>> GetAllTagsAsync();
}
