using AiToDu.Platform.Warehouse.Domain.Model.ValueObjects;
namespace AiToDu.Platform.Warehouse.Domain.Model.Commands;

public sealed record RegisterStockCountCommand(InventoryItemId ItemId, Guid OperationId, long ExpectedVersion, StockLotId LotId, decimal ExpectedQuantity, decimal CountedQuantity, string Reason);
