using AiToDu.Platform.Warehouse.Domain.Model.ValueObjects;
namespace AiToDu.Platform.Warehouse.Domain.Model.Commands;

public sealed record ClassifyInventoryReturnCommand(InventoryItemId ItemId, Guid OperationId, long ExpectedVersion, StockLotId SourceLotId, decimal Quantity, ReturnDisposition Disposition, string Reference, string Reason);
