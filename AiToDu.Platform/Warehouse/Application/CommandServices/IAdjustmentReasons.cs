using AiToDu.Platform.Warehouse.Domain.Model.Entities;
namespace AiToDu.Platform.Warehouse.Application.CommandServices;

public interface IAdjustmentReasons
{
    Task<List<AdjustmentReason>> List(CancellationToken ct);
    Task<AdjustmentReason?> Add(string name, CancellationToken ct);
}
