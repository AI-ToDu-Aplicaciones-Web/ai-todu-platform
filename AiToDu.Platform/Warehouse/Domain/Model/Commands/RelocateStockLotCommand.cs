using AiToDu.Platform.Warehouse.Domain.Model.ValueObjects;
namespace AiToDu.Platform.Warehouse.Domain.Model.Commands;

public sealed record RelocateStockLotCommand(InventoryItemId ItemId, Guid OperationId, long ExpectedVersion, StockLotId LotId, string Location, string Reason);
