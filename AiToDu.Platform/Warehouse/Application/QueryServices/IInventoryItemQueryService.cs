using AiToDu.Platform.Warehouse.Domain.Model.Aggregates;
using AiToDu.Platform.Warehouse.Domain.Model.Queries;
namespace AiToDu.Platform.Warehouse.Application.QueryServices;

public interface IInventoryItemQueryService
{
    Task<InventoryItem?> FindBySku(string sku, CancellationToken ct);
    Task<InventoryItem?> Handle(GetInventoryItemByIdQuery query, CancellationToken ct);
    Task<(IReadOnlyList<InventoryItem> Items, int Total)> Handle(GetInventoryItemsQuery query, CancellationToken ct);
}
