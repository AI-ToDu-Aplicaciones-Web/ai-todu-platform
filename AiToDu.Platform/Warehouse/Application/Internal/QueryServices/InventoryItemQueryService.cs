using AiToDu.Platform.Warehouse.Domain.Model.ValueObjects;
using AiToDu.Platform.Warehouse.Domain.Model.Errors;
using AiToDu.Platform.Shared.Domain.Model.ValueObjects;
using AiToDu.Platform.Warehouse.Application.OutboundServices;
using AiToDu.Platform.Warehouse.Application.QueryServices;
using AiToDu.Platform.Warehouse.Domain.Model.Aggregates;
using AiToDu.Platform.Warehouse.Domain.Model.Queries;
using AiToDu.Platform.Warehouse.Domain.Repositories;
namespace AiToDu.Platform.Warehouse.Application.Internal.QueryServices;

public sealed class InventoryItemQueryService(IInventoryItemRepository repository, ICurrentWarehouseActor actor) : IInventoryItemQueryService
{
    public Task<InventoryItem?> Handle(GetInventoryItemByIdQuery query, CancellationToken ct) =>
        repository.FindAsync(actor.OrganizationId, query.ItemId, false, ct);
    public Task<(IReadOnlyList<InventoryItem> Items, int Total)> Handle(GetInventoryItemsQuery query, CancellationToken ct)
    {
        if (query.Page < 1 || query.Page > 1000000 || query.PageSize < 1 || query.PageSize > 100 || query.Search?.Length > 160)
            throw new WarehouseDomainException(WarehouseErrorCode.InvalidPagination);
        return repository.SearchAsync(actor.OrganizationId, query.Search, query.Page, query.PageSize, ct);
    }
}
