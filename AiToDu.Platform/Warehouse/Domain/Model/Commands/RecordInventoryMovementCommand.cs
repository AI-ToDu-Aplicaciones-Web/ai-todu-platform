using AiToDu.Platform.Warehouse.Domain.Model.ValueObjects;
namespace AiToDu.Platform.Warehouse.Domain.Model.Commands;

public sealed record RecordInventoryMovementCommand(InventoryItemId ItemId, Guid OperationId, long ExpectedVersion, StockLotId LotId, MovementType Type, decimal Quantity, string Reason);
