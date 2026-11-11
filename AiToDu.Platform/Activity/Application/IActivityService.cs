using AiToDu.Platform.Activity.Domain.Model;
namespace AiToDu.Platform.Activity.Application;

public interface IActivityService
{
    Task<List<StockAlert>> Alerts(CancellationToken ct);
    Task<bool> Read(string key, CancellationToken ct);
    Task<List<AuditEntry>> History(CancellationToken ct);
}
