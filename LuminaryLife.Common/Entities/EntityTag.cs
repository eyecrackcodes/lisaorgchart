using LuminaryLife.Common.Systems.Tags.Models;

namespace LuminaryLife.Common.Entities;

/// <summary>
/// Junction table linking entities to tags
/// </summary>
public class EntityTag
{
    public int Id { get; set; }
    
    /// <summary>
    /// The ID of the entity (can be int as string or GUID string)
    /// </summary>
    public string EntityId { get; set; } = string.Empty;
    
    /// <summary>
    /// Type of entity being tagged
    /// </summary>
    public EntityTypeEnum EntityType { get; set; }
    
    /// <summary>
    /// Foreign key to Tag
    /// </summary>
    public int TagId { get; set; }
    
    /// <summary>
    /// Who created this tag assignment
    /// </summary>
    public string? CreatedBy { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Navigation property to tag
    /// </summary>
    public virtual Tag? Tag { get; set; }
}
