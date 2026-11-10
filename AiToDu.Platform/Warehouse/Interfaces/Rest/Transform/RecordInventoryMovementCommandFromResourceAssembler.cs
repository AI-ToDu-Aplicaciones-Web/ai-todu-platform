using AiToDu.Platform.Warehouse.Domain.Model.Commands;
using AiToDu.Platform.Warehouse.Domain.Model.ValueObjects;
using AiToDu.Platform.Warehouse.Interfaces.Rest.Resources;
namespace AiToDu.Platform.Warehouse.Interfaces.Rest.Transform;

public static class RecordInventoryMovementCommandFromResourceAssembler
{ public static RecordInventoryMovementCommand ToCommandFromResource(Guid id, RecordInventoryMovementResource r) => new(new InventoryItemId(id), r.OperationId, r.ExpectedVersion, new StockLotId(r.LotId), r.Type, r.Quantity, r.Reason); }
