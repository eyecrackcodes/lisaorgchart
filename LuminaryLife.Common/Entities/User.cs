using LuminaryLife.Common.Enums;

namespace LuminaryLife.Common.Entities;

/// <summary>
/// Represents a user in the system (agent, manager, or admin)
/// </summary>
public class User
{
    /// <summary>
    /// Primary key - GUID string
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    public string? Name { get; set; }
    public string? Email { get; set; }
    
    /// <summary>
    /// Foreign key to agency site
    /// </summary>
    public int? AgencySiteId { get; set; }
    
    /// <summary>
    /// Foreign key to agency team
    /// </summary>
    public int? AgencyTeamId { get; set; }
    
    /// <summary>
    /// Job title classification
    /// </summary>
    public TitleEnum Title { get; set; } = TitleEnum.Agent;
    
    /// <summary>
    /// Commission tier level
    /// </summary>
    public CommissionTierEnum CommissionTier { get; set; } = CommissionTierEnum.None;
    
    /// <summary>
    /// Employment status
    /// </summary>
    public AgentStatusEnum AgentStatus { get; set; } = AgentStatusEnum.Active;
    
    /// <summary>
    /// Agent type classification
    /// </summary>
    public AgentTypeEnum AgentType { get; set; } = AgentTypeEnum.Training;
    
    /// <summary>
    /// Date the agent started at the agency
    /// </summary>
    public DateTime? AgencyStartDate { get; set; }
    
    /// <summary>
    /// URL to avatar image
    /// </summary>
    public string? ImageAvatarUrl { get; set; }
    
    /// <summary>
    /// Phone number
    /// </summary>
    public string? Phone { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    
    /// <summary>
    /// Navigation property to agency site
    /// </summary>
    public virtual AgencySite? AgencySite { get; set; }
    
    /// <summary>
    /// Navigation property to agency team
    /// </summary>
    public virtual AgencyTeam? AgencyTeam { get; set; }
}
