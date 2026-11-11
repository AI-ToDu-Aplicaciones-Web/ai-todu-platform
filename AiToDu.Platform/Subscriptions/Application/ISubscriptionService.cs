using AiToDu.Platform.Subscriptions.Domain.Model;
namespace AiToDu.Platform.Subscriptions.Application;

public interface ISubscriptionService
{
    Task<Subscription> Current(CancellationToken ct);
    Task<int> Users(CancellationToken ct);
    Task<int> Products(CancellationToken ct);
    Task<bool> Choose(string plan, CancellationToken ct);
    Task<bool> WithLock(Func<Task> action, CancellationToken ct);
}
