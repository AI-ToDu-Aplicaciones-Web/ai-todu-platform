using AiToDu.Platform.Warehouse.Domain.Model.ValueObjects;
namespace AiToDu.Platform.Warehouse.Domain.Model.Commands;

public sealed record ChangeLotQuarantineCommand(InventoryItemId ItemId, Guid OperationId, long ExpectedVersion, StockLotId LotId, bool IsQuarantined, string Reason);
