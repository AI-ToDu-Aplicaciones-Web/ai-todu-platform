using AiToDu.Platform.Warehouse.Domain.Model.ValueObjects;
namespace AiToDu.Platform.Warehouse.Domain.Model.Commands;

public sealed record ReceiveStockLotCommand(InventoryItemId ItemId, Guid OperationId, long ExpectedVersion, string LotCode, string Location, DateOnly? ExpiresOn, decimal Quantity);
