namespace LuminaryLife.Common.Entities;

/// <summary>
/// Represents a physical agency site/location
/// </summary>
public class AgencySite
{
    public int Id { get; set; }
    
    /// <summary>
    /// Unique identifier for external references
    /// </summary>
    public string UId { get; set; } = Guid.NewGuid().ToString();
    
    public string Name { get; set; } = string.Empty;
    
    public string? Address1 { get; set; }
    public string? Address2 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Zip { get; set; }
    public string? Phone { get; set; }
    
    /// <summary>
    /// Calculated count of agents at this site
    /// </summary>
    public int AgentCount { get; set; }
    
    /// <summary>
    /// Calculated count of teams at this site
    /// </summary>
    public int TeamCount { get; set; }
    
    /// <summary>
    /// Site goal target for agent count
    /// </summary>
    public int? GoalTarget { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    
    /// <summary>
    /// Navigation property to teams at this site
    /// </summary>
    public virtual ICollection<AgencyTeam> AgencyTeams { get; set; } = new List<AgencyTeam>();
    
    /// <summary>
    /// Navigation property to users at this site
    /// </summary>
    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
