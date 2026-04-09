using MetaCompta.Domain.Entities;

namespace MetaCompta.Domain.Common;

/// <summary>
/// Represents a Moroccan Fiduciary Cabinet (Tenant)
/// </summary>
public class Cabinet : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? ICE { get; set; }
    public string? IF { get; set; }
    public string? RC { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Phone { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public bool IsActive { get; set; } = true;

    // Subscription
    public int? SubscriptionPlanId { get; set; }
    public virtual SubscriptionPlan? SubscriptionPlan { get; set; }
    public DateTime? SubscriptionStartDate { get; set; }
    public DateTime? SubscriptionEndDate { get; set; }

    // Navigation
    public virtual ICollection<Client> Clients { get; set; } = new List<Client>();
    public virtual ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
}
