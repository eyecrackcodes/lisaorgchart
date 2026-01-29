using LuminaryLife.Common.Systems.Tags.Models;
using LuminaryLife.Common.Systems.Tags.Repositories;

namespace LuminaryLife.Common.Systems.Tags.Services;

/// <summary>
/// Service implementation for tag operations
/// </summary>
public class TagService : ITagService
{
    private readonly ITagRepository _tagRepository;

    public TagService(ITagRepository tagRepository)
    {
        _tagRepository = tagRepository;
    }

    public Task<List<TagResponse>> GetTagsByEntityIdAsync(EntityTypeEnum entityType, int entityId)
    {
        return _tagRepository.GetTagsByEntityIdAsync(entityType, entityId);
    }

    public Task<List<TagResponse>> GetTagsByEntityIdAsync(EntityTypeEnum entityType, string entityId)
    {
        return _tagRepository.GetTagsByEntityIdAsync(entityType, entityId);
    }

    public Task<Dictionary<string, List<TagResponse>>> GetTagsForEntitiesAsync(EntityTypeEnum entityType, List<string> entityIds)
    {
        return _tagRepository.GetTagsForEntitiesAsync(entityType, entityIds);
    }

    public Task<bool> UpdateEntityTagsAsync(EntityTypeEnum entityType, int entityId, List<int> tagIds, string? createdBy = null)
    {
        return _tagRepository.UpdateEntityTagsAsync(entityType, entityId, tagIds, createdBy);
    }

    public Task<bool> UpdateEntityTagsAsync(EntityTypeEnum entityType, string entityId, List<int> tagIds, string? createdBy = null)
    {
        return _tagRepository.UpdateEntityTagsAsync(entityType, entityId, tagIds, createdBy);
    }

    public Task<List<TagResponse>> GetAllTagsAsync()
    {
        return _tagRepository.GetAllTagsAsync();
    }
}
