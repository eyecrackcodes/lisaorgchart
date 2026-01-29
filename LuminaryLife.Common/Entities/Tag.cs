namespace LuminaryLife.Common.Entities;

/// <summary>
/// Represents a tag that can be applied to entities
/// </summary>
public class Tag
{
    public int Id { get; set; }
    
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Hex color code without # prefix (e.g., "FFA500")
    /// </summary>
    public string? HexColorCode { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    /// <summary>
    /// Navigation property to entity tags
    /// </summary>
    public virtual ICollection<EntityTag> EntityTags { get; set; } = new List<EntityTag>();
}
