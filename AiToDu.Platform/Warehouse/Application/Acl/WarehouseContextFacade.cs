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
    public async Task<SaleQuote?> Quote(int itemId, CancellationToken ct)
    {
        var item = await queries.Handle(new GetInventoryItemByIdQuery(new InventoryItemId(itemId)), ct);
        if (item is null) return null;
        var today = DateOnly.FromDateTime(clock.GetCurrentTime().DateTime);
        var lots = item.Lots.Where(x => x.IsAvailableOn(today) && x.Quantity.Value > 0)
            .OrderBy(x => x.ExpiresOn ?? DateOnly.MaxValue).ThenBy(x => x.Id.Value)
            .Select(x => new AvailableLot(x.Id.Value, x.Quantity.Value)).ToList();
        return new SaleQuote(itemId, item.Sku.Value, item.Name, item.UnitPrice, item.Version, lots);
    }

    public async Task<bool> AcceptSaleReturn(int itemId, Guid lot, Guid operation, decimal quantity, string disposition, string reference, string reason, CancellationToken ct)
    {
        if (!Enum.TryParse<ReturnDisposition>(disposition, out var kind) || !Enum.IsDefined(kind)) return false;
        var item = await queries.Handle(new GetInventoryItemByIdQuery(new InventoryItemId(itemId)), ct);
        if (item is null) return false;
        var command = new ClassifyInventoryReturnCommand(item.Id, operation, item.Version, new StockLotId(lot), quantity, kind, reference, reason);
        return (await commands.HandleSaleReturn(command, ct)).IsSuccess;
    }

    public async Task<decimal?> FetchAvailableQuantity(int inventoryItemId, CancellationToken ct)
    {
        var item = await queries.Handle(new GetInventoryItemByIdQuery(new InventoryItemId(inventoryItemId)), ct);
        return item?.AvailableQuantityOn(DateOnly.FromDateTime(clock.GetCurrentTime().DateTime));
    }
    public async Task<bool> IssueForSale(int inventoryItemId, Guid lotId, Guid saleOperationId, long expectedVersion, decimal quantity, string saleReference, CancellationToken ct) =>
        (await commands.Handle(new IssueStockForSaleCommand(new InventoryItemId(inventoryItemId), saleOperationId, expectedVersion, new StockLotId(lotId), quantity, saleReference), ct)).IsSuccess;
}
