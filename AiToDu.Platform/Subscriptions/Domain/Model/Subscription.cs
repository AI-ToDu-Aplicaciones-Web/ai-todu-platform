namespace AiToDu.Platform.Subscriptions.Domain.Model;

public sealed class Subscription
{
    public Guid OrganizationId { get; private set; }
    public string Plan { get; private set; } = "starter";
    public DateTime UpdatedAt { get; private set; }
    public int? UserLimit => Plan switch { "starter" => 3, "growth" => 10, _ => null };
    public int? SkuLimit => Plan == "starter" ? 500 : null;
    private Subscription() { }
    public Subscription(Guid organization) { OrganizationId = organization; UpdatedAt = DateTime.UtcNow; }
    public bool Choose(string plan, int users, int products)
    {
        if (plan is not ("starter" or "growth" or "enterprise")) return false;
        if (plan == "starter" && (users > 3 || products > 500) || plan == "growth" && users > 10) return false;
        Plan = plan;
        UpdatedAt = DateTime.UtcNow;
        return true;
    }
}
