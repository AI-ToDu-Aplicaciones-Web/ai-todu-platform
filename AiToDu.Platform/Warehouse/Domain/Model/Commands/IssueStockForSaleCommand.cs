using AiToDu.Platform.Warehouse.Domain.Model.ValueObjects;

namespace AiToDu.Platform.Warehouse.Domain.Model.Commands;

/// <summary>Internal Sales use case; not exposed as a general inventory HTTP mutation.</summary>
public sealed record IssueStockForSaleCommand(InventoryItemId ItemId, Guid OperationId, long ExpectedVersion,
    StockLotId LotId, decimal Quantity, string SaleReference);
