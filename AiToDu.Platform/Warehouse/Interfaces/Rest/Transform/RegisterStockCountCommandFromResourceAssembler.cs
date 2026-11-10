using AiToDu.Platform.Warehouse.Domain.Model.Commands;
using AiToDu.Platform.Warehouse.Domain.Model.ValueObjects;
using AiToDu.Platform.Warehouse.Interfaces.Rest.Resources;
namespace AiToDu.Platform.Warehouse.Interfaces.Rest.Transform;

public static class RegisterStockCountCommandFromResourceAssembler
{ public static RegisterStockCountCommand ToCommandFromResource(Guid id, RegisterStockCountResource r) => new(new InventoryItemId(id), r.OperationId, r.ExpectedVersion, new StockLotId(r.LotId), r.ExpectedQuantity, r.CountedQuantity, r.Reason); }
