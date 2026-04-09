namespace MetaCompta.Domain.Common;

/// <summary>
/// SaaS Subscription Plans
/// </summary>
public class SubscriptionPlan : BaseEntity
{
    public string Name { get; set; } = string.Empty; // Starter, Professional, Enterprise
    public string? Description { get; set; }
    public decimal MonthlyPrice { get; set; }
    public decimal AnnualPrice { get; set; }
    public int MaxInternalUsers { get; set; } = 3;
    public int MaxClientCompanies { get; set; } = 10;
    public int MaxExtranetUsersPerClient { get; set; } = 10;
    public int MaxStorageMB { get; set; } = 5120; // 5GB
    public bool IsActive { get; set; } = true;
    public string? Features { get; set; } // JSON array of features

    public virtual ICollection<Cabinet> Cabinets { get; set; } = new List<Cabinet>();
}
