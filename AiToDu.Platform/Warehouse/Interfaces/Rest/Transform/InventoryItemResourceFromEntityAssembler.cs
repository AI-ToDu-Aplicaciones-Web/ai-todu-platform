using AiToDu.Platform.Warehouse.Domain.Model.Aggregates;
using AiToDu.Platform.Warehouse.Interfaces.Rest.Resources;
namespace AiToDu.Platform.Warehouse.Interfaces.Rest.Transform;

public static class InventoryItemResourceFromEntityAssembler
{
    public static InventoryItemResource ToResourceFromEntity(InventoryItem item, DateOnly today)
    {
        var available = item.AvailableQuantityOn(today);
        return new(item.Id.Value, item.Sku.Value, item.Name, item.Category, item.MinimumStock.Value, item.UnitPrice, item.Version, item.TotalQuantity, available,
            available == 0 ? "STOCK_OUT" : available <= item.MinimumStock.Value ? "LOW_STOCK" : "AVAILABLE",
            item.Lots.OrderBy(x => x.Code.Value).Select(x => new StockLotResource(x.Id.Value, x.Code.Value, x.Location.Value, x.ExpiresOn, x.Quantity.Value, x.IsQuarantined, x.ExpiresOn < today)).ToList(),
            item.Movements.OrderByDescending(x => x.OccurredAt).Select(x => new InventoryMovementResource(x.Id, x.OperationId, x.LotId.Value, x.Type.ToString(), x.Change, x.Before, x.After, x.Reason, x.ActorId.Value, x.OccurredAt)).ToList(),
            item.Counts.OrderByDescending(x => x.OccurredAt).Select(x => new StockCountResource(x.Id, x.LotId.Value, x.ExpectedQuantity, x.CountedQuantity, x.Reason, x.OccurredAt)).ToList(),
            item.Returns.OrderByDescending(x => x.OccurredAt).Select(x => new InventoryReturnResource(x.Id, x.SourceLotId.Value, x.DestinationLotId?.Value, x.Quantity, x.Disposition.ToString(), x.Reference, x.Reason, x.OccurredAt)).ToList());
    }
}
