using AiToDu.Platform.Warehouse.Domain.Model.ValueObjects;
using AiToDu.Platform.Warehouse.Domain.Model.Errors;
using AiToDu.Platform.Shared.Domain.Repositories;
using AiToDu.Platform.Warehouse.Domain.Model.Aggregates;

namespace AiToDu.Platform.Warehouse.Domain.Repositories;

public interface IInventoryItemRepository : IBaseRepository<InventoryItem>
{
    Task<InventoryItem?> FindAsync(OrganizationId organizationId, InventoryItemId itemId, bool tracking, CancellationToken ct);
    Task<bool> SkuExistsAsync(OrganizationId organizationId, Sku sku, CancellationToken ct);
    Task<(IReadOnlyList<InventoryItem> Items, int Total)> SearchAsync(OrganizationId organizationId, string? search, int page, int pageSize, CancellationToken ct);
}
