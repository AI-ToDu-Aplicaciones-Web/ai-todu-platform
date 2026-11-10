using AiToDu.Platform.Warehouse.Application.OutboundServices;
using AiToDu.Platform.Warehouse.Interfaces.Acl;
using AiToDu.Platform.Warehouse.Application.CommandServices;
using AiToDu.Platform.Warehouse.Application.QueryServices;
using AiToDu.Platform.Warehouse.Domain.Model.Commands;
using AiToDu.Platform.Warehouse.Domain.Model.Queries;
using AiToDu.Platform.Warehouse.Domain.Model.ValueObjects;
namespace AiToDu.Platform.Warehouse.Application.Acl;

public sealed class WarehouseContextFacade(IInventoryItemQueryService queries, IInventoryItemCommandService commands, IWarehouseClock clock) : IWarehouseContextFacade
{
    public async Task<decimal?> FetchAvailableQuantity(Guid inventoryItemId, CancellationToken ct)
    {
        var item = await queries.Handle(new GetInventoryItemByIdQuery(new InventoryItemId(inventoryItemId)), ct);
        return item?.AvailableQuantityOn(DateOnly.FromDateTime(clock.GetCurrentTime().DateTime));
    }
    public async Task<bool> IssueForSale(Guid inventoryItemId, Guid lotId, Guid saleOperationId, long expectedVersion, decimal quantity, string saleReference, CancellationToken ct) =>
        (await commands.Handle(new IssueStockForSaleCommand(new InventoryItemId(inventoryItemId), saleOperationId, expectedVersion, new StockLotId(lotId), quantity, saleReference), ct)).IsSuccess;
}
