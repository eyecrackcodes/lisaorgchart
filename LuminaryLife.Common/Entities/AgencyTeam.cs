namespace LuminaryLife.Common.Entities;

/// <summary>
/// Represents a team within an agency site, led by a manager
/// </summary>
public class AgencyTeam
{
    public int Id { get; set; }
    
    /// <summary>
    /// Unique identifier for external references
    /// </summary>
    public string UId { get; set; } = Guid.NewGuid().ToString();
    
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Foreign key to parent agency site
    /// </summary>
    public int? AgencySiteId { get; set; }
    
    /// <summary>
    /// Foreign key to the team manager (User)
    /// </summary>
    public string? ManagerUserId { get; set; }
    
    /// <summary>
    /// Calculated count of team members
    /// </summary>
    public int MemberCount { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    
    /// <summary>
    /// Navigation property to parent site
    /// </summary>
    public virtual AgencySite? AgencySite { get; set; }
    
    /// <summary>
    /// Navigation property to team manager
    /// </summary>
    public virtual User? Manager { get; set; }
    
    /// <summary>
    /// Navigation property to team members
    /// </summary>
    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
